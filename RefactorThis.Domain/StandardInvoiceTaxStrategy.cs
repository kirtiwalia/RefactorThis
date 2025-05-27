namespace RefactorThis.Domain
{
	/// <summary>
	/// Tax strategy for standard invoices: tax is applied only on the first payment.
	/// </summary>
	public class StandardInvoiceTaxStrategy : IInvoiceTaxStrategy
	{
		/// <inheritdoc />
		public decimal CalculateTax(decimal amount, int paymentCount)
		{
			return paymentCount == 1 ? amount * 0.14m : 0m;
		}
	}
}
