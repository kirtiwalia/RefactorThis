using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public interface IPaymentValidator
    {
        ValidationResult Validate(Invoice invoice, Payment payment);
    }

    public class PaymentValidator : IPaymentValidator
    {
        public ValidationResult Validate(Invoice invoice, Payment payment)
        {
            if (invoice == null)
                return ValidationResult.Invalid("Invoice not found");

            if (payment == null)
                return ValidationResult.Invalid("Payment cannot be null");

            if (payment.Amount <= 0)
                return ValidationResult.Invalid("Payment amount must be greater than zero");

            return ValidateInvoiceState(invoice, payment);
        }

        private ValidationResult ValidateInvoiceState(Invoice invoice, Payment payment)
        {
            if (invoice.Amount == 0)
            {
                if (invoice.Payments != null && invoice.Payments.Any())
                    throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");

                return ValidationResult.Invalid("no payment needed");
            }

            var currentAmountPaid = 0m;
            if (invoice.Payments != null)
                currentAmountPaid = invoice.Payments.Sum(p => p.Amount);

            if (IsInvoiceFullyPaid(invoice, currentAmountPaid))
                return ValidationResult.Invalid("invoice was already fully paid");

            return ValidatePaymentAmount(invoice, payment, currentAmountPaid);
        }

        private bool IsInvoiceFullyPaid(Invoice invoice, decimal currentAmountPaid)
        {
            return currentAmountPaid > 0 && invoice.Amount == currentAmountPaid;
        }

        private ValidationResult ValidatePaymentAmount(Invoice invoice, Payment payment, decimal currentAmountPaid)
        {
            var remainingAmount = invoice.Amount - currentAmountPaid;

            if (payment.Amount > remainingAmount)
            {
                var message = HasExistingPayments(invoice)
                    ? "the payment is greater than the partial amount remaining"
                    : "the payment is greater than the invoice amount";

                return ValidationResult.Invalid(message);
            }

            return ValidationResult.Valid();
        }

        private bool HasExistingPayments(Invoice invoice)
        {
            return invoice.Payments != null && invoice.Payments.Any();
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        private ValidationResult(bool isValid, string errorMessage = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Valid() => new ValidationResult(true);
        public static ValidationResult Invalid(string message) => new ValidationResult(false, message);
    }
}