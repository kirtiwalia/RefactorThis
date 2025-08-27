using RefactorThis.Persistence;
using System;

namespace RefactorThis.Domain
{
    public interface IInvoiceStrategy
    {
        /// <summary>
        /// Apply a payment to the invoice.
        /// hadPreviousPayments: true if invoice already had payments.
        /// isFinalPayment: true if this payment will make the invoice fully paid.
        /// </summary>
        void ApplyPayment(Invoice invoice, Payment payment, bool hadPreviousPayments, bool isFinalPayment);
    }
}