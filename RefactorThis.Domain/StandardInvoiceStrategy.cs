using RefactorThis.Persistence;
using System;

namespace RefactorThis.Domain
{
    public class StandardInvoiceStrategy : IInvoiceStrategy
    {
        private const decimal TaxRate = 0.14m;

        public void ApplyPayment(Invoice invoice, Payment payment, bool hadPreviousPayments, bool isFinalPayment)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            if (hadPreviousPayments)
            {
                // original behaviour: when there are existing payments, Standard does NOT modify TaxAmount
                invoice.AmountPaid += payment.Amount;
                invoice.Payments.Add(payment);
            }
            else
            {
                // original behaviour (no previous payments): Standard DID set TaxAmount = payment * 0.14m
                invoice.AmountPaid = payment.Amount;
                invoice.TaxAmount = payment.Amount * TaxRate;
                invoice.Payments.Add(payment);
            }
        }
    }
}
