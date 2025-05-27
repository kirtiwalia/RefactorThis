namespace RefactorThis.Domain
{
	/// <summary>
	/// Encapsulates the outcome of applying a payment to an invoice.
	/// </summary>
	public sealed class PaymentResult
	{
		private PaymentResult(bool success, string message)
		{
			IsSuccess = success;
			Message = message;
		}

		/// <summary>True when the payment mutated invoice state; false when it was rejected.</summary>
		public bool IsSuccess { get; }
		/// <summary>A user-friendly description of the outcome.</summary>
		public string Message { get; }

		public static PaymentResult Success(string message) => new PaymentResult(true, message);
		public static PaymentResult Failure(string message) => new PaymentResult(false, message);
	}
}
