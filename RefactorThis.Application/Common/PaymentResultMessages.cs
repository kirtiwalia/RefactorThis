using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Application.Common
{
    public static class PaymentResultMessages
    {
        public static string ToMessage(PaymentResultCode code, Invoice invoice)
        {
            switch (code)
            {
                case PaymentResultCode.NoPaymentNeeded:
                    return "No payment needed";

                case PaymentResultCode.InvoiceAlreadyPaid:
                    return "Invoice was already fully paid";

                case PaymentResultCode.PaymentGreaterThanRemaining:
                    return "The payment is greater than the partial amount remaining";

                case PaymentResultCode.PaymentGreaterThanInvoiceAmount:
                    return "The payment is greater than the invoice amount";

                case PaymentResultCode.FullyPaid:
                    return invoice.Payments != null && invoice.Payments.Count > 1
                        ? "Final partial payment received, invoice is now fully paid"
                        : "Invoice is now fully paid";

                case PaymentResultCode.PartiallyPaid:
                    return invoice.Payments != null && invoice.Payments.Count > 1
                        ? "Another partial payment received, still not fully paid"
                        : "Invoice is now partially paid";

                default:
                    return "Unknown payment status";
            }
        }
    }
}
