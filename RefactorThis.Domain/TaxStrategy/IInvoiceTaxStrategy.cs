namespace RefactorThis.Domain.TaxStrategy
{
	/// <summary>
	/// Represents a strategy for calculating tax on an invoice payment.
	/// </summary>
	public interface IInvoiceTaxStrategy
	{
		/// <summary>
		/// Calculates the tax amount to apply for a given payment.
		/// </summary>
		/// <param name="amount">The payment amount.</param>
		/// <param name="paymentCount">The total number of payments after this one is applied.</param>
		/// <returns>The tax amount to be applied.</returns>
		decimal CalculateTax(decimal amount, int paymentCount);
	}
}
