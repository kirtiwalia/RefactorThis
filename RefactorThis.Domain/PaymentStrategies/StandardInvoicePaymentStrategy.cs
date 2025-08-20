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
    public class StandardInvoicePaymentStrategy : IInvoicePaymentStrategy
    {
        public PaymentResultCode ApplyPayment(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid += payment.Amount;
            invoice.Payments.Add(payment);

            return invoice.AmountPaid == invoice.Amount
             ? PaymentResultCode.FullyPaid
             : PaymentResultCode.PartiallyPaid;
        }
    }
}
