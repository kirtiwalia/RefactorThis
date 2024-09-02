using RefactorThis.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Extensions
{
    public static class InvoiceExtensions
    {
        public static bool HasZeroAmount(this Invoice invoice)
        {
            return invoice.Amount == 0;
        }
        public static bool HasPayments(this Invoice invoice)
        {
            return invoice.Payments != null && invoice.Payments.Any();
        }
        public static bool IsAlreadyFullyPaid(this Invoice invoice)
        {
            return invoice.Payments.Sum(x => x.Amount) != 0 && invoice.Amount == invoice.Payments.Sum(x => x.Amount);
        }

        public static bool IsFullyPaid(this Invoice invoice, Payment payment)
        {
            if (HasPayments(invoice))
            {
                return (invoice.Amount - invoice.AmountPaid) == payment.Amount;

            }

            return invoice.Amount == payment.Amount;
        }

        public static bool IsPaymentGreater(this Invoice invoice, Payment payment)
        {
            if (HasPayments(invoice))
            {
                return invoice.Payments.Sum(x => x.Amount) != 0 && payment.Amount > (invoice.Amount - invoice.AmountPaid);
            }

            return payment.Amount > invoice.Amount;
        }
       
    }
}
