
namespace RefactorThis.Domain.messaging
{
    public class ProcessPaymentErrorMessages
    {
        public const string NoPaymentNecessary = "This invoice has no amount, so no payment is needed.";
        public const string InvalidInvoice_NoAmountButHasPayments = "The invoice is in an invalid state, it has an amount of 0 and it has payments.";
        public const string InvoiceAlreadyPaid = "The invoice was already fully paid";
        public const string PaymentTooGreat = "The payment is greater than the partial amount remaining";
        public const string InvoiceWithPaymentAlreadyExists = "An invoice with this payment already exists";
    }
}
