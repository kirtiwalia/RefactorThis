using RefactorThis.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain
{
    public class CommercialInvoiceHandler : IInvoiceHandler
    {
        public void ApplyFullPayment(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid += payment.Amount;
            if (invoice.Payments == null)
                invoice.Payments = new System.Collections.Generic.List<Payment>();
            invoice.Payments.Add(payment);
            invoice.TaxAmount += invoice.AmountPaid * 0.14m;
        }

        public void ApplyPartialPayment(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid += payment.Amount;
            if (invoice.Payments == null)
                invoice.Payments = new System.Collections.Generic.List<Payment>();
            invoice.Payments.Add(payment);
            invoice.TaxAmount += invoice.AmountPaid * 0.14m;
        }
    }
}