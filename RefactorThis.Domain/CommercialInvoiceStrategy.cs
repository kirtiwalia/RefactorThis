using RefactorThis.Persistence;
using System;

namespace RefactorThis.Domain
{
    public class CommercialInvoiceStrategy : IInvoiceStrategy
    {
        private const decimal TaxRate = 0.14m;

        public void ApplyPayment(Invoice invoice, Payment payment, bool hadPreviousPayments, bool isFinalPayment)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            if (hadPreviousPayments)
            {
                // for Commercial, add tax for every applied payment when there are prior payments
                invoice.AmountPaid += payment.Amount;
                invoice.TaxAmount += payment.Amount * TaxRate;
                invoice.Payments.Add(payment);
            }
            else
            {
                // for no previous payments, same as standard for initial tax assignment (original behaviour)
                invoice.AmountPaid = payment.Amount;
                invoice.TaxAmount = payment.Amount * TaxRate;
                invoice.Payments.Add(payment);
            }
        }
    }
}
