using System;

namespace RefactorThis.Domain
{
	/// <summary>
	/// Factory responsible for returning the appropriate tax strategy
	/// based on the invoice type.
	/// </summary>
	public static class InvoiceTaxStrategyFactory
	{
		/// <summary>
		/// Returns a tax strategy implementation for the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The type of invoice (e.g., Standard or Commercial).</param>
		/// <returns>An instance of <see cref="IInvoiceTaxStrategy"/> suitable for the invoice type.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when the provided invoice type is not supported.
		/// </exception>
		public static IInvoiceTaxStrategy GetStrategy(InvoiceType type)
		{
			switch (type)
			{
				case InvoiceType.Standard:
					return new StandardInvoiceTaxStrategy();
				case InvoiceType.Commercial:
					return new CommercialInvoiceTaxStrategy();
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported invoice type.");
			}
		}
	}
}
