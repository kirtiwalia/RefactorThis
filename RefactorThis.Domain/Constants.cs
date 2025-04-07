namespace RefactorThis.Domain
{
    public static class Constants
    {
        public const decimal TaxRate = 0.14m;
        public const string NoPaymentRequiredMessage = "No payment needed";
        public const string InvoiceAlreadyFullyPaidMessage = "Invoice was already fully paid";
        public const string InitialOverpaymentMessage = "The payment is greater than the invoice amount";
        public const string FinalInitialPaymentMessage = "Invoice is now fully paid";
        public const string PartialInitialPaymentMessage = "Invoice is now partially paid";
        public const string PartialOverpaymentMessage = "The payment is greater than the partial amount remaining";
        public const string PartialPaymentMessage = "Another partial payment received, still not fully paid";
        public const string FinalPartialPaymentMessage = "Final partial payment received, invoice is now fully paid";
    }
}