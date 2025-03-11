using System;
using System.Linq;
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

        /// <summary>
        /// Processes a payment for a given invoice, handling zero amount, first payment, and subsequent payments.
        /// </summary>
        /// <param name="payment">The payment to process.</param>
        /// <returns>A message indicating the result of the payment processing.</returns>
        public string ProcessPayment(Payment payment)
        {
            var inv = _invoiceRepository.GetInvoice(payment.Reference)
                ?? throw new InvalidOperationException("There is no invoice matching this payment");

            string responseMessage;

            if (inv.Amount == 0)
            {
                responseMessage = HandleZeroAmountInvoice(inv);
            }
            else if (inv.Payments != null && inv.Payments.Any())
            {
                responseMessage = HandleSubsequentPayments(inv, payment);
            }
            else
            {
                responseMessage = HandleFirstPayment(inv, payment);
            }

            inv.Save();

            return responseMessage;
        }

        /// <summary>
        /// Handles invoices with an amount of 0, determining if a payment is needed or invalid.
        /// </summary>
        /// <param name="inv">The invoice to process.</param>
        /// <returns>A message indicating no payment needed, or throws an exception if invalid.</returns>
        private string HandleZeroAmountInvoice(Invoice inv)
        {
            if (inv.Payments == null || !inv.Payments.Any())
            {
                return "no payment needed";
            }

            throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }

        /// <summary>
        /// Handles payments for invoices that already have previous payments, including partial and full payments.
        /// </summary>
        /// <param name="inv">The invoice to process.</param>
        /// <param name="payment">The new payment to apply.</param>
        /// <returns>A message describing the result of the payment processing.</returns>
        private string HandleSubsequentPayments(Invoice inv, Payment payment)
        {
            var totalPayments = inv.Payments.Sum(x => x.Amount);

            if (totalPayments != 0 && inv.Amount == totalPayments)
            {
                return "invoice was already fully paid";
            }
            else if (totalPayments != 0 && payment.Amount > (inv.Amount - inv.AmountPaid))
            {
                return "the payment is greater than the partial amount remaining";
            }
            else
            {
                ApplyPayment(inv, payment);

                if (inv.Amount == inv.AmountPaid)
                    return "final partial payment received, invoice is now fully paid";

                return "another partial payment received, still not fully paid";
            }
        }

        /// <summary>
        /// Handles the first payment for an invoice with no prior payments.
        /// </summary>
        /// <param name="inv">The invoice to process.</param>
        /// <param name="payment">The first payment to apply.</param>
        /// <returns>A message indicating whether the invoice is fully or partially paid.</returns>
        private string HandleFirstPayment(Invoice inv, Payment payment)
        {
            if (payment.Amount > inv.Amount)
            {
                return "the payment is greater than the invoice amount";
            }

            ApplyPayment(inv, payment);

            if (payment.Amount == inv.Amount)
                return "invoice is now fully paid";

            return "invoice is now partially paid";
        }

        /// <summary>
        /// Applies a payment to an invoice and updates tax if the invoice type is Commercial.
        /// </summary>
        /// <param name="inv">The invoice to update.</param>
        /// <param name="payment">The payment to apply.</param>
        private void ApplyPayment(Invoice inv, Payment payment)
        {
            inv.AmountPaid += payment.Amount;
            inv.Payments.Add(payment);

            if (inv.Type == InvoiceType.Commercial)
            {
                inv.TaxAmount += payment.Amount * 0.14m;
            }
        }
    }
}
