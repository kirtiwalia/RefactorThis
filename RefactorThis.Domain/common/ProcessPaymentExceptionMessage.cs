namespace RefactorThis.Domain.common
{
    public abstract class ProcessPaymentExceptionMessage
    {
        public const string NoInvoiceMatchingPayment = "There is no invoice matching this payment.";
        public const string InvalidInvoiceState = "The invoice is in an invalid state, it has an amount of 0 and it has payments.";
    }
}