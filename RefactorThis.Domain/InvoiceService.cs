using RefactorThis.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Domain
{
    public class InvoiceService
    {
        private readonly InvoiceRepository _invoiceRepository;
        private readonly Dictionary<InvoiceType, IInvoiceHandler> _handlers;

        public InvoiceService(InvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
            _handlers = new Dictionary<InvoiceType, IInvoiceHandler>
            {
                { InvoiceType.Standard, new StandardInvoiceHandler() },
                { InvoiceType.Commercial, new CommercialInvoiceHandler() }
            };
        }

        public string ProcessPayment(Payment payment)
        {
            var inv = _invoiceRepository.GetInvoice(payment.Reference);

            if (inv == null)
                throw new InvalidOperationException("There is no invoice matching this payment");

            if (inv.Amount == 0)
            {
                if (inv.Payments == null || !inv.Payments.Any())
                    return "no payment needed";
                throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
            }

            decimal totalPaid = inv.Payments?.Sum(x => x.Amount) ?? 0;
            decimal remaining = inv.Amount - inv.AmountPaid;

            if (totalPaid > 0)
            {
                if (inv.Amount == totalPaid)
                    return "invoice was already fully paid";
                if (payment.Amount > remaining)
                    return "the payment is greater than the partial amount remaining";
            }
            else if (payment.Amount > inv.Amount)
            {
                return "the payment is greater than the invoice amount";
            }

            var handler = GetHandler(inv.Type);

            if (remaining == payment.Amount)
            {
                handler.ApplyFullPayment(inv, payment);
                inv.Save();
                return totalPaid > 0
                    ? "final partial payment received, invoice is now fully paid"
                    : "invoice is now fully paid";
            }

            handler.ApplyPartialPayment(inv, payment);
            inv.Save();
            return totalPaid > 0
                ? "another partial payment received, still not fully paid"
                : "invoice is now partially paid";
        }

        private IInvoiceHandler GetHandler(InvoiceType type)
        {
            if (!_handlers.TryGetValue(type, out var handler))
                throw new ArgumentOutOfRangeException($"Unsupported invoice type: {type}");
            return handler;
        }
    }
}