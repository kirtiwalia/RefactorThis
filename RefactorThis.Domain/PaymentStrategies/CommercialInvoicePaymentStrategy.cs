using RefactorThis.Application.Interfaces;
using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain.PaymentStrategies
{
    public class CommercialInvoicePaymentStrategy : IInvoicePaymentStrategy
    {
        public PaymentResultCode ApplyPayment(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid += payment.Amount;
            invoice.TaxAmount += payment.Amount * 0.14m;
            invoice.Payments.Add(payment);

            return invoice.AmountPaid == invoice.Amount
                ? PaymentResultCode.FullyPaid
                : PaymentResultCode.PartiallyPaid;
        }
    }
}
