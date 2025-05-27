namespace RefactorThis.Domain
{
	/// <summary>
	/// Tax strategy for commercial invoices: tax is applied on every payment.
	/// </summary>
	public class CommercialInvoiceTaxStrategy : IInvoiceTaxStrategy
	{
		/// <inheritdoc />
		public decimal CalculateTax(decimal amount, int paymentCount)
		{
			return amount * 0.14m;
		}
	}
}
