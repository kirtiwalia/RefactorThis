using System.Collections.Generic;
using System.Linq;
using RefactorThis.Domain.Models;
using RefactorThis.Persistence.Models;

namespace RefactorThis.Domain.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        public PaymentResult ProcessPayment(Invoice invoice, Payment payment)
        {
            if (IsNoPaymentRequired(invoice))
                return new PaymentResult(invoice, Constants.NoPaymentRequiredMessage);
            
            if (IsAlreadyFullyPaid(invoice))
                return new PaymentResult(invoice, Constants.InvoiceAlreadyFullyPaidMessage);

            if (IsOverpayment(invoice, payment))
                return new PaymentResult(invoice, GetOverpaymentMessage(invoice));

            return ApplyPayment(invoice, payment);
        }

        private static PaymentResult ApplyPayment(Invoice invoice, Payment payment)
        {
            var remainingInvoiceAmount = invoice.Amount - invoice.AmountPaid;
            var isFirstPayment = !InvoiceHasExistingPayments(invoice);
            var isFinalPayment = remainingInvoiceAmount == payment.Amount;

            invoice.AmountPaid += payment.Amount;
            if (invoice.Payments == null) // probably overkill as list is initialised inline in Invoice.cs, but just to be safe
                invoice.Payments = new List<Payment>();
            invoice.Payments.Add(payment);
            
            ApplyTaxToInvoice(invoice, payment, isFirstPayment);
            
            var responseMessage = GetResponseMessage(isFirstPayment, isFinalPayment);
            
            return new PaymentResult(invoice, responseMessage);
        }

        /// <summary>
        /// Provides the desired response message based on the payment status of the invoice.
        /// The message is determined by considering if this is the first payment on the invoice,
        /// and whether the current payment fully settles the remaining balance. 
        /// </summary>
        /// <param name="isFirstPayment">Indicates if this is the first payment made on the invoice</param>
        /// <param name="isFinalPayment">Indicates if the current payment will settle the remaining balance</param>
        /// <returns>A string corresponding to the payment situation</returns>
        private static string GetResponseMessage(bool isFirstPayment, bool isFinalPayment)
        {
            if (isFirstPayment)
            {
                return isFinalPayment ? Constants.FinalInitialPaymentMessage : Constants.PartialInitialPaymentMessage;
            }

            return isFinalPayment ? Constants.FinalPartialPaymentMessage : Constants.PartialPaymentMessage;
        }

        private static void ApplyTaxToInvoice(Invoice invoice, Payment payment, bool isFirstPayment)
        {
            if (isFirstPayment || invoice.Type == InvoiceType.Commercial)
                invoice.TaxAmount += payment.Amount * Constants.TaxRate;
        }

        private static bool IsNoPaymentRequired(Invoice invoice) =>
            invoice.Amount == 0 && !InvoiceHasExistingPayments(invoice);

        private static bool IsAlreadyFullyPaid(Invoice invoice) =>
            InvoiceHasExistingPayments(invoice) &&
            InvoiceHasPositivePaymentTotal(invoice) &&
            IsInvoicePaidInFull(invoice);
        
        private static bool IsInvoicePaidInFull(Invoice invoice) => 
            invoice.Payments?.Sum(p => p.Amount) == invoice.Amount;

        private static bool IsOverpayment(Invoice invoice, Payment payment)
        {
            var isPaymentExceedingBalance = payment.Amount > (invoice.Amount - invoice.AmountPaid);
            if (InvoiceHasExistingPayments(invoice))
            {
                return InvoiceHasPositivePaymentTotal(invoice) && isPaymentExceedingBalance;
            }
            
            return isPaymentExceedingBalance;
        }

        private static string GetOverpaymentMessage(Invoice invoice) =>
            InvoiceHasExistingPayments(invoice)
                ? Constants.PartialOverpaymentMessage
                : Constants.InitialOverpaymentMessage;
        
        private static bool InvoiceHasExistingPayments(Invoice invoice) =>
            invoice.Payments != null && invoice.Payments.Count > 0;

        private static bool InvoiceHasPositivePaymentTotal(Invoice invoice) =>
            invoice.Payments.Sum(x => x.Amount) != 0;
    }
}