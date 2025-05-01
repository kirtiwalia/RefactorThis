using System.Linq;
using RefactorThis.Domain.Models;
using RefactorThis.Persistence.Entities;

namespace RefactorThis.Domain.Validators
{
    public class InvoiceValidator : IInvoiceValidator
    {
        public PaymentResult ValidatePayment(Invoice invoice, Payment payment)
        {
            if (invoice == null)
                return PaymentResult.FailureResult(
                    ResultCode.InvoiceNotFound, 
                    "There is no invoice matching this payment");

            if (invoice.Amount == 0)
                return ValidateZeroAmountInvoice(invoice);

            return ValidateNonZeroAmountInvoice(invoice, payment);
        }

        private PaymentResult ValidateZeroAmountInvoice(Invoice invoice)
        {
            if (invoice.Payments == null || !invoice.Payments.Any())
                return PaymentResult.SuccessResult(
                    ResultCode.NoPaymentNeeded, 
                    "no payment needed");

            return PaymentResult.FailureResult(
                ResultCode.InvalidInvoiceState,
                "The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }

        private PaymentResult ValidateNonZeroAmountInvoice(Invoice invoice, Payment payment)
        {
            if (invoice.Payments.Any())
                return ValidateInvoiceWithExistingPayments(invoice, payment);

            return ValidateInvoiceWithoutPayments(invoice, payment);
        }

        private PaymentResult ValidateInvoiceWithExistingPayments(Invoice invoice, Payment payment)
        {
            var totalPaid = invoice.Payments.Sum(x => x.Amount);
            var remainingAmount = invoice.Amount - totalPaid;

            if (totalPaid == invoice.Amount)
                return PaymentResult.SuccessResult(
                    ResultCode.AlreadyFullyPaid,
                    "invoice was already fully paid");

            if (payment.Amount > remainingAmount)
                return PaymentResult.FailureResult(
                    ResultCode.PaymentExceedsRemainingAmount,
                    "the payment is greater than the partial amount remaining");

            return null; // validation passed
        }

        private PaymentResult ValidateInvoiceWithoutPayments(Invoice invoice, Payment payment)
        {
            if (payment.Amount > invoice.Amount)
                return PaymentResult.FailureResult(
                    ResultCode.PaymentExceedsInvoiceAmount,
                    "the payment is greater than the invoice amount");

            return null; // validation passed
        }
    }
}