using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class InvoiceService : IInvoiceService
    {
        private const decimal TAX_RATE = 0.14m;
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository) =>
            _invoiceRepository = invoiceRepository;

        public string ProcessPayment(Payment payment)
        {
            var invoice = _invoiceRepository.GetInvoice(payment.Reference);
            if (invoice == null)
                throw new InvalidOperationException("There is no invoice matching this payment");

            // Ensure Payments collection is initialized.
            invoice.Payments = invoice.Payments ?? new System.Collections.Generic.List<Payment>();

            if (invoice.Amount == 0)
            {
                if (invoice.Payments.Any())
                    throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");

                return "no payment needed";
            }

            // Calculate the already paid amount.
            decimal paidAmount = invoice.Payments.Sum(x => x.Amount);
            decimal remainingAmount = invoice.Amount - paidAmount;

            if (remainingAmount == 0)
                return "invoice was already fully paid";

            bool hasExistingPayments = invoice.Payments.Any();
            if (hasExistingPayments && payment.Amount > remainingAmount)
                return "the payment is greater than the partial amount remaining";

            if (!hasExistingPayments && payment.Amount > invoice.Amount)
                return "the payment is greater than the invoice amount";

            bool completesInvoice = payment.Amount == remainingAmount;
            ApplyPayment(invoice, payment);
            invoice.Save();

            if (completesInvoice)
                return hasExistingPayments
                    ? "final partial payment received, invoice is now fully paid"
                    : "invoice is now fully paid";
            else
                return hasExistingPayments
                    ? "another partial payment received, still not fully paid"
                    : "invoice is now partially paid";
        }

        /// <summary>
        /// Applies the payment to the invoice. For commercial invoices, tax is applied.
        /// </summary>
        private void ApplyPayment(Invoice invoice, Payment payment)
        {
            invoice.Payments.Add(payment);
            invoice.AmountPaid = invoice.Payments.Sum(x => x.Amount);

            if (invoice.Type == InvoiceType.Commercial)
                invoice.TaxAmount += payment.Amount * TAX_RATE;
        }
    }
}
