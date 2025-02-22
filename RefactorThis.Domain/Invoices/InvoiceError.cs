namespace RefactorThis.Domain.Invoices
{
    public class InvoiceError
    {
        public const string NoInvoiceFound = "There is no invoice matching this payment";
        public const string NoPaymentNeeded = "no payment needed";
        public const string InvoiceInvalidState = "The invoice is in an invalid state, it has an amount of 0 and it has payments.";
        public const string InvoiceAlreadyFullyPaid = "invoice was already fully paid";
        public const string PaymentIsGreaterThanRemainingBalance = "the payment is greater than the partial amount remaining";
        public const string PaymentIsGreaterThanInvoiceAmount = "the payment is greater than the invoice amount";
    }
}
