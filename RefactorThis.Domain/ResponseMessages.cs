
namespace RefactorThis.Domain
{
    /// <summary>
    /// 1. Moved all the hardcoded Response Messages & Rates into new classes for clear readability & flexible to add new Constants.
    /// </summary>
    public class ResponseMessages
    {
        public const string NoInvoiceMatchingPayment = "There is no invoice matching this payment";
        public const string NoPaymentNeeded = "no payment needed";
        public const string InvoiceInvalidZeroAmountWithPayments = "The invoice is in an invalid state, it has an amount of 0 and it has payments.";
        public const string InvoiceAlreadyFullyPaid = "invoice was already fully paid";
        public const string PaymentGreaterPartialAmountRemaining = "the payment is greater than the partial amount remaining";
        public const string PaymentGreaterInvoiceAmount = "the payment is greater than the invoice amount";
        public const string FinalPartialPaymentReceivedInvoiceFullyPaid = "final partial payment received, invoice is now fully paid";
        public const string AnotherPartialPaymentReceivedInvoiceNotFullyPaid = "another partial payment received, still not fully paid";
        public const string InvoiceNowFullyPaid = "invoice is now fully paid";
        public const string InvoiceNowPartiallyPaid = "invoice is now partially paid";
    }

    public class Rates
    {
        public const decimal TaxRate = 0.14m;
        public const decimal ZeroTaxRate = 0m;
    }
}