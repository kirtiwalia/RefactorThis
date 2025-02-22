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

        public string ProcessPayment(Payment payment)
        {
            var invoice = _invoiceRepository.GetInvoice(payment.Reference)
                ?? throw new InvalidOperationException("There is no invoice matching this payment.");

            if (invoice.Amount == 0)
                return HandleInvoiceWithZeroAmount(invoice);

            if (invoice.Payments.Any())
                return ProcessInvoiceWithExistingPayment(payment, invoice);

            if (payment.Amount > invoice.Amount)
                return "The payment is greater than the invoice amount.";

            if (invoice.Amount == payment.Amount)
                return ProcessPaymentWithExactAmount(payment, invoice);

            return ProcessPaymentWithRemainingBalance(payment, invoice);

        }

        private static string ProcessPaymentWithRemainingBalance(Payment payment, Invoice invoice)
        {
            UpdateInvoicePayment(payment, invoice);
            return "Invoice is now partially paid.";
        }

        private static string ProcessPaymentWithExactAmount(Payment payment, Invoice invoice)
        {
            UpdateInvoicePayment(payment, invoice);
            return "Invoice is now fully paid.";
        }
        private static string ProcessInvoiceWithExistingPayment(Payment payment, Invoice invoice)
        {
            var totalPaidInInvoice = invoice.Payments.Sum(x => x.Amount);
            var invoiceBalance = invoice.Amount - invoice.AmountPaid;

            if (invoice.Amount == totalPaidInInvoice)
                return "Invoice was already fully paid.";

            if (payment.Amount > invoiceBalance)
                return "The payment is greater than the partial amount remaining.";

            if (invoiceBalance == payment.Amount)
                return HandlePaymentInFull(payment, invoice);

            return HandlePaymentWithRemainingBalance(payment, invoice);
        }

        private static string HandlePaymentWithRemainingBalance(Payment payment, Invoice invoice)
        {
            UpdateInvoicePayment(payment, invoice);
            return "Another partial payment received, still not fully paid.";
        }

        private static string HandlePaymentInFull(Payment payment, Invoice invoice)
        {
            UpdateInvoicePayment(payment, invoice);
            return "Final partial payment received, invoice is now fully paid.";
        }

        private static string HandleInvoiceWithZeroAmount(Invoice invoice)
        {
            if (invoice.Payments == null || !invoice.Payments.Any())
                return "No payment needed.";

            throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }
        private static void UpdateInvoicePayment(Payment payment, Invoice invoice)
        {
            invoice.AmountPaid += payment.Amount;

            if (invoice.Type == InvoiceType.Commercial)
                invoice.TaxAmount += payment.Amount * 0.14m;

            invoice.Payments.Add(payment);
            invoice.Save();
        }
    }
}