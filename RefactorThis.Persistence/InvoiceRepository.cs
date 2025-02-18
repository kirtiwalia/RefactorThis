using System.Collections.Generic;

namespace RefactorThis.Persistence
{

	public interface IInvoiceRepository
	{
		Invoice GetInvoice(string reference);
		void SaveInvoice(Invoice invoice);
		void Add(Invoice invoice);
	}


	public class InvoiceRepository : IInvoiceRepository
	{
		private readonly Dictionary<string, Invoice> _invoices = new Dictionary<string, Invoice>();

		public Invoice GetInvoice( string reference )
		{
			return _invoices.TryGetValue(reference, out var invoice) ? invoice : null;
		}

		public void SaveInvoice( Invoice invoice )
		{
			//saves the invoice to the database,
			// Simulate storing in memory (instead of a real database)
			_invoices[invoice.Reference] = invoice;
		}

		public void Add( Invoice invoice )
		{
			if (!_invoices.ContainsKey(invoice.Reference))
			{
				_invoices[invoice.Reference] = invoice;
			}
		}
	}
}