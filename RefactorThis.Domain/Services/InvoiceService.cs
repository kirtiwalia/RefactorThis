using System;
using System.Linq;
using RefactorThis.Domain.Services;
using RefactorThis.Domain.Extensions;
using RefactorThis.Persistence.Entities;
using RefactorThis.Persistence.Repositories;
using RefactorThis.Domain.Services.Interface;

namespace RefactorThis.Domain
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public string ProcessPayment(Payment payment)
        {
            var inv = _invoiceRepository.GetInvoice(payment.Reference);

            if (inv == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            if (inv.HasZeroAmount())
            {
                if (!inv.HasPayments())
                {
                    return "no payment needed";
                }
                throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
            }

            if (inv.HasPayments())
            {
                return ProcessInvoiceWithPayments(inv, payment);
            }

            if (inv.IsPaymentGreater(payment))
            {
                return "the payment is greater than the invoice amount";
            }

            UpdateInvoice(inv, payment);

            return inv.IsFullyPaid(payment) ? "invoice is now fully paid" : "invoice is now partially paid";
        }

        private string ProcessInvoiceWithPayments(Invoice inv, Payment payment)
        {
            if (inv.IsAlreadyFullyPaid())
            {
                return "invoice was already fully paid";
            }

            if (inv.IsPaymentGreater(payment))
            {
                return "the payment is greater than the partial amount remaining";
            }

            UpdateInvoice(inv, payment);

            return inv.IsFullyPaid(payment) ?
                "final partial payment received, invoice is now fully paid"
                : "another partial payment received, still not fully paid";
        }

        private void UpdateInvoice(Invoice invoice, Payment payment)
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

            _invoiceRepository.SaveInvoice(invoice);
        }
    }
}