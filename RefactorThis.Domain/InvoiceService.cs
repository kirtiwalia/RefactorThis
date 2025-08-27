using System;
using System.Linq;
using System.Collections.Generic;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class InvoiceService
    {
        private readonly InvoiceRepository _invoiceRepository;

        public InvoiceService(InvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public string ProcessPayment(Payment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            var inv = _invoiceRepository.GetInvoice(payment.Reference);

            if (inv == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            // Ensure Payments list exists to avoid null-ref when we add a payment
            if (inv.Payments == null)
            {
                inv.Payments = new List<Payment>();
            }

            // Case: invoice amount is zero
            if (inv.Amount == 0)
            {
                if (inv.Payments == null || !inv.Payments.Any())
                {
                    return "no payment needed";
                }

                throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
            }

            // Compute current totals
            var paymentsExist = inv.Payments != null && inv.Payments.Any();
            var totalPaid = inv.Payments?.Sum(x => x.Amount) ?? 0m;

            // If there are payments and totalPaid equals invoice amount => already fully paid
            if (paymentsExist && totalPaid != 0m && inv.Amount == totalPaid)
            {
                return "invoice was already fully paid";
            }

            // Remaining amount uses inv.AmountPaid as original code uses inv.AmountPaid in that calculation
            var remaining = inv.Amount - inv.AmountPaid;

            // If payments exist and this payment is bigger than the remaining partial amount
            if (paymentsExist && totalPaid != 0m && payment.Amount > remaining)
            {
                return "the payment is greater than the partial amount remaining";
            }

            // Choose strategy based on invoice type
            var strategy = InvoiceStrategyFactory.GetStrategy(inv.Type);

            // Two main flows: invoice already had payments OR this is the very first payment
            if (paymentsExist)
            {
                // If this payment equals the remaining amount, it's the final partial payment
                var isFinal = remaining == payment.Amount;

                strategy.ApplyPayment(inv, payment, hadPreviousPayments: true, isFinalPayment: isFinal);

                // Persist only when we've modified the invoice
                inv.Save();

                return isFinal
                    ? "final partial payment received, invoice is now fully paid"
                    : "another partial payment received, still not fully paid";
            }
            else
            {
                // No existing payments on this invoice
                if (payment.Amount > inv.Amount)
                {
                    return "the payment is greater than the invoice amount";
                }

                if (inv.Amount == payment.Amount)
                {
                    strategy.ApplyPayment(inv, payment, hadPreviousPayments: false, isFinalPayment: true);
                    inv.Save();
                    return "invoice is now fully paid";
                }
                else
                {
                    strategy.ApplyPayment(inv, payment, hadPreviousPayments: false, isFinalPayment: false);
                    inv.Save();
                    return "invoice is now partially paid";
                }
            }
        }
    }
}
