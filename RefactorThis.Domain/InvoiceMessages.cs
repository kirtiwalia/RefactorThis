namespace RefactorThis.Domain
{
	/// <summary>
	/// Provides centralised messages used in invoice payment results.
	/// </summary>
	public static class InvoiceMessages
	{
		public const string NoPaymentNeeded = "no payment needed";
		public const string InvalidZeroAmountWithPayments = "The invoice is in an invalid state, it has an amount of 0 and it has payments.";
		public const string InvoiceAlreadyFullyPaid = "invoice was already fully paid";
		public const string PaymentExceedsRemainingBalance = "the payment is greater than the partial amount remaining";
		public const string PaymentExceedsInvoiceAmount = "the payment is greater than the invoice amount";
		public const string FinalPartialPaymentComplete = "final partial payment received, invoice is now fully paid";
		public const string InvoiceNowFullyPaid = "invoice is now fully paid";
		public const string AdditionalPartialPayment = "another partial payment received, still not fully paid";
		public const string InitialPartialPayment = "invoice is now partially paid";
	}
}
