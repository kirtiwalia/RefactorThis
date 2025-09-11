namespace RefactorThis.Domain.Models
{
	/// <summary>
	/// Represents a payment made towards an invoice
	/// </summary>
	public class Payment
	{
		/// <summary>
		/// The amount of the payment
		/// </summary>
		public decimal Amount { get; set; }
		/// <summary>
		/// A reference for the payment
		/// </summary>
		public string Reference { get; set; }
	}
}