using System;
using System.Linq;
using RefactorThis.Persistence;
using System.Collections.Generic;

namespace RefactorThis.Domain
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IRepository _invoiceRepository;

        public InvoiceService(IRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public string ProcessPayment(Payment payment)
        {
            var invoice = _invoiceRepository.GetInvoice(payment.Reference);
            if (invoice == null)
                throw new InvalidOperationException("There is no invoice matching this payment");

            if (invoice.Amount == 0)
                return HandleZeroAmountInvoice(invoice);

            if (invoice.Payments != null && invoice.Payments.Any())
                return HandleExistingPayments(invoice, payment);

            return HandleFirstPayment(invoice, payment);
        }

        public string HandleZeroAmountInvoice(Invoice invoice)
        {
            if (invoice.Payments == null || !invoice.Payments.Any())
                return "no payment needed";

            throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }

        public string HandleExistingPayments(Invoice invoice, Payment payment)
        {
            var totalPaid = invoice.Payments.Sum(p => p.Amount);
            var amountRemaining = invoice.Amount - invoice.AmountPaid;

            if (totalPaid != 0 && invoice.Amount == totalPaid)
                return "invoice was already fully paid";

            if (payment.Amount > amountRemaining)
                return "the payment is greater than the partial amount remaining";

            bool isFinalPayment = payment.Amount == amountRemaining;
            ApplyPayment(invoice, payment);

            return isFinalPayment
                ? "final partial payment received, invoice is now fully paid"
                : "another partial payment received, still not fully paid";
        }
        public string HandleFirstPayment(Invoice invoice, Payment payment)
        {
            if (payment.Amount > invoice.Amount)
                return "the payment is greater than the invoice amount";

            bool isFullPayment = payment.Amount == invoice.Amount;
            ApplyPayment(invoice, payment, isFirstPayment: true);

            return isFullPayment
                ? "invoice is now fully paid"
                : "invoice is now partially paid";
        }

        public void ApplyPayment(Invoice invoice, Payment payment, bool isFirstPayment = false)
        {
            invoice.AmountPaid += payment.Amount;
            if (invoice.Type == InvoiceType.Commercial || isFirstPayment)
                invoice.TaxAmount += payment.Amount * 0.14m;

            if (invoice.Payments == null)
                invoice.Payments = new List<Payment>();

            invoice.Payments.Add(payment);
            invoice.Save();
        }

    }
}