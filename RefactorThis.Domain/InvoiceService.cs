using System;
using System.Linq;
using RefactorThis.Persistence;
using RefactorThis.Persistence.Contracts;
using RefactorThis.Persistence.Enums;

namespace RefactorThis.Domain
{
    /// <summary>
    /// An implementation of <see cref="IInvoiceService"/> that processes payments for invoices.
    /// </summary>
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        ///<inheritdoc/>
        public PaymentResult ProcessPayment(Payment payment)
        {
            if (payment == null)
                return new PaymentResult(PaymentStatus.Error, "There is no payment to process");

            var invoice = _invoiceRepository.GetInvoice(payment.Reference);

            if (invoice == null)
                throw new InvalidOperationException("There is no invoice matching this payment");

            return ProcessInvoicePayment(payment, invoice);
        }

        private PaymentResult ProcessInvoicePayment(Payment payment, Invoice invoice)
        {
            if (invoice.Amount == 0)
                return HandleZeroAmountInvoice(invoice);

            if (invoice.Payments != null && invoice.Payments.Any())
                return HandleExistingPayments(invoice, payment);

            return HandleNewPayment(invoice, payment);
        }

        private PaymentResult HandleZeroAmountInvoice(Invoice invoice)
        {
            if (invoice.Payments == null || !invoice.Payments.Any())
                return new PaymentResult(PaymentStatus.NoPaymentNeeded, "No payment needed");

            throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }

        private PaymentResult HandleExistingPayments(Invoice invoice, Payment payment)
        {
            var totalPaid = invoice.Payments.Sum(x => x.Amount);

            if (totalPaid != 0 && invoice.Amount == totalPaid)
                return new PaymentResult(PaymentStatus.AlreadyPaid, "Invoice was already fully paid");

            if (totalPaid != 0 && payment.Amount > (invoice.Amount - invoice.AmountPaid))
                return new PaymentResult(PaymentStatus.Overpaid, "The payment is greater than the partial amount remaining");

            return ProcessPartialPayment(invoice, payment);
        }

        private PaymentResult ProcessPartialPayment(Invoice invoice, Payment payment)
        {
            if ((invoice.Amount - invoice.AmountPaid) == payment.Amount)
                return FinalPartialPayment(invoice, payment);

            return AnotherPartialPayment(invoice, payment);
        }

        private PaymentResult FinalPartialPayment(Invoice invoice, Payment payment)
        {
            switch (invoice.Type)
            {
                case InvoiceType.Standard:
                    invoice.AmountPaid += payment.Amount;
                    invoice.Payments.Add(payment);
                    break;
                case InvoiceType.Commercial:
                    invoice.AmountPaid += payment.Amount;
                    invoice.TaxAmount += payment.Amount * 0.14m;
                    invoice.Payments.Add(payment);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            invoice.Save();
            return new PaymentResult(PaymentStatus.FullyPaid, "Final partial payment received, invoice is now fully paid");
        }

        private PaymentResult AnotherPartialPayment(Invoice invoice, Payment payment)
        {
            switch (invoice.Type)
            {
                case InvoiceType.Standard:
                    invoice.AmountPaid += payment.Amount;
                    invoice.Payments.Add(payment);
                    break;
                case InvoiceType.Commercial:
                    invoice.AmountPaid += payment.Amount;
                    invoice.TaxAmount += payment.Amount * 0.14m;
                    invoice.Payments.Add(payment);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            invoice.Save();
            return new PaymentResult(PaymentStatus.PartiallyPaid, "Another partial payment received, still not fully paid");
        }

        private PaymentResult HandleNewPayment(Invoice invoice, Payment payment)
        {
            if (payment.Amount > invoice.Amount)
                return new PaymentResult(PaymentStatus.Overpaid, "The payment is greater than the invoice amount");

            if (invoice.Amount == payment.Amount)
                return FullyPaidInvoice(invoice, payment);

            return PartiallyPaidInvoice(invoice, payment);
        }

        private PaymentResult FullyPaidInvoice(Invoice invoice, Payment payment)
        {
            switch (invoice.Type)
            {
                case InvoiceType.Standard:
                    invoice.AmountPaid = payment.Amount;
                    invoice.TaxAmount = payment.Amount * 0.14m;
                    invoice.Payments.Add(payment);
                    break;
                case InvoiceType.Commercial:
                    invoice.AmountPaid = payment.Amount;
                    invoice.TaxAmount = payment.Amount * 0.14m;
                    invoice.Payments.Add(payment);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            invoice.Save();
            return new PaymentResult(PaymentStatus.FullyPaid, "Invoice is now fully paid");
        }

        private PaymentResult PartiallyPaidInvoice(Invoice invoice, Payment payment)
        {
            switch (invoice.Type)
            {
                case InvoiceType.Standard:
                    invoice.AmountPaid = payment.Amount;
                    invoice.TaxAmount = payment.Amount * 0.14m;
                    invoice.Payments.Add(payment);
                    break;
                case InvoiceType.Commercial:
                    invoice.AmountPaid = payment.Amount;
                    invoice.TaxAmount = payment.Amount * 0.14m;
                    invoice.Payments.Add(payment);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            invoice.Save();
            return new PaymentResult(PaymentStatus.PartiallyPaid, "Invoice is now partially paid");
        }
    }
}