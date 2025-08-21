using System;
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

        private string ValidateInvoice(Invoice invoice, Payment payment)
        {
            if (invoice == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }
            if (invoice.Amount == 0 && invoice.HasPayments)
            {
                throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
            }

            List<InvoiceType> allowedInvoiceTypes = new List<InvoiceType>() { InvoiceType.Commercial, InvoiceType.Commercial };
            if (allowedInvoiceTypes.Contains(invoice.Type))
            {
                throw new ArgumentOutOfRangeException();
            }

            if (invoice.Amount == 0)
            {
                return "no payment needed";
            }

            if (invoice.Amount == invoice.PaymentsTotalAmount)
            {
                return "invoice was already fully paid";
            }

            if (payment.Amount > invoice.AmountToPay)
            {
                return invoice.HasPayments ? "the payment is greater than the partial amount remaining" : "the payment is greater than the invoice amount";
            }

            return string.Empty;
        }

        public string ProcessPayment(Payment payment)
        {
            var invoice = _invoiceRepository.GetInvoice(payment.Reference);

            var responseMessage = this.ValidateInvoice(invoice, payment);

            if (string.IsNullOrWhiteSpace(responseMessage))
            {
                string partialPaymentMessage = invoice.HasPayments ? "another partial payment received, still not fully paid" : "invoice is now partially paid";
                string fullPaymentMessage = invoice.HasPayments ? "final partial payment received, invoice is now fully paid" : "invoice is now fully paid";
                bool fullyPaid = invoice.AmountToPay == payment.Amount;
                responseMessage = fullyPaid ? fullPaymentMessage : partialPaymentMessage;

                invoice.AmountPaid += payment.Amount;
                if (invoice.Type == InvoiceType.Commercial)
                {
                    invoice.TaxAmount += payment.Amount * 0.14m;
                }
                invoice.Payments.Add(payment);
            }

            invoice.Save();

            return responseMessage;
        }
    }
}