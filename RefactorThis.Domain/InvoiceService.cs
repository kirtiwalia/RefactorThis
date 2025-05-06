using RefactorThis.Persistence;
using System;
using System.Linq;

namespace RefactorThis.Domain
{
    public class InvoiceService
    {
        private const decimal TaxPercentage = 0.14m;
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public string ProcessPayment(Payment payment)
        {
            //Retrieve the invoice from repository
            var inv = _invoiceRepository.GetInvoice(payment.Reference);

            //If null invoice, throw exception
            if (inv == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            //If invoice amount is 0, not payments needed (Question here, is it possible for the amount to be less than 0, if so would there still be payments?)
            if (inv.Amount == 0)
            {
                // Avoid using Any for lists, refactored unecessary save (nothing changed on the inv object), also simplified if else
                if (inv.Payments == null || inv.Payments.Count == 0)
                {
                    return "no payment needed";
                }
                throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
            }

            // If there are any existing payments check if the payment amount exceeds/fully pays the invoice
            // Can there be an invoice with an amount paid without a payment?
            if (inv.Payments != null && inv.Payments.Count > 0)
            {
                // If the AmountPaid on the Invoice is accurate this local variable isn't required
                decimal totalAmountPaid = inv.Payments.Sum(x => x.Amount);
                if (inv.Amount == totalAmountPaid)
                {
                    return "invoice was already fully paid";
                }
                // Inconsistancy here, sometimes using the AmountPaid on the Invoice, vs the sum of the payments, is there a reason for this?
                // Additionally if answer to above about invoices that are less than 0 there is a potential bug here, where a negative payment
                // is received, but not processed because it will not be greater than the remainder.
                if (payment.Amount > (inv.Amount - inv.AmountPaid))
                {
                    return "the payment is greater than the partial amount remaining";
                }

                UpdateInvoice(inv, payment);
                //Does payment pay off the invoice?
                if (inv.Amount == inv.AmountPaid)
                {
                    return "final partial payment received, invoice is now fully paid";
                }

                return "another partial payment received, still not fully paid";
            }

            // is the payment greater than the total invoice amount (Potential bug if negative invoices are possible)
            if (payment.Amount > inv.Amount)
            {
                return "the payment is greater than the invoice amount";
            }

            UpdateInvoice(inv, payment);
            // If invoice paid off return appropriate message
            if (inv.Amount == inv.AmountPaid)
            {
                return "invoice is now fully paid";
            }

            return "invoice is now partially paid";
        }

        private void UpdateInvoice(Invoice invoice, Payment payment)
        {
            if (invoice.Type != InvoiceType.Standard && invoice.Type != InvoiceType.Commercial)
            {
                throw new ArgumentOutOfRangeException();
            }
            invoice.AmountPaid += payment.Amount;
            invoice.Payments.Add(payment);

            if (invoice.Type == InvoiceType.Commercial)
            {
                // Division errors here could occur over time, assuming this is GST the payment should be pro-rated accross the invoice,
                // however the total tax amount will never change?  Have written a test for this that will fail for now.
                invoice.TaxAmount += payment.Amount * TaxPercentage;
            }
            invoice.Save();
        }
    }
}