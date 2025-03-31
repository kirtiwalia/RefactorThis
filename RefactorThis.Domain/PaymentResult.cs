namespace RefactorThis.Domain
{
	public class PaymentResult
	{
		public PaymentResult(bool success, State paymentState)
		{
			Success = success;
			PaymentState = paymentState;
		}

		public bool Success { get; set; }
		public State PaymentState  { get; set; }
	}
	public enum State
	{
		NoInvoiceFound = 0,
		PartialPaid = 1,
		NoPaymentRequred = 2,
		InvalidState = 3,
		OverPaid = 4,
		GreaterThanRemainder = 5,
		Error = 6,
		FullyPaid = 7
	}
}