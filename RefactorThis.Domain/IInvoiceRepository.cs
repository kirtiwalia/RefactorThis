namespace RefactorThis.Domain
{
	/// <summary>Abstraction over the persistence mechanism for invoices.</summary>
	public interface IInvoiceRepository
	{
		/// <summary>Fetches an invoice by its external reference.</summary>
		Invoice GetByReference(string reference);

		/// <summary>Saves the supplied invoice instance.</summary>
		void Save(Invoice invoice);
	}
}
