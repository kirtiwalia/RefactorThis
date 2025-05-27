using System.Collections.Concurrent;
using RefactorThis.Domain;

namespace RefactorThis.Persistence
{
	/// <summary>In-memory repository used by tests; replace with DB implementation later.</summary>
	public class InvoiceRepository : IInvoiceRepository
	{
		private readonly ConcurrentDictionary<string, Invoice> _store = new ConcurrentDictionary<string, Invoice>();

		public Invoice GetByReference(string reference) =>
				_store.TryGetValue(reference ?? string.Empty, out var inv) ? inv : null;

		public void Save(Invoice invoice)
		{
			// For an in-memory repo nothing is required,
			// but we keep the method for future persistence.
		}

		/// <summary>Helper used only by tests to pre-seed data.</summary>
		public void Add(string reference, Invoice invoice) => _store[reference ?? string.Empty] = invoice;
	}
}
