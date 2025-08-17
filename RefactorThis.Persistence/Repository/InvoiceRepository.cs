using RefactorThis.Persistence.IRepository;
using RefactorThis.Persistence.Models;

namespace RefactorThis.Persistence.Repository {
	public class InvoiceRepository : IInvoiceRepository
    {
		private Invoice _invoice;

		public Invoice GetInvoice( string reference )
		{
			return _invoice;
		}

		public void SaveInvoice( Invoice invoice )
		{
			//saves the invoice to the database
		}

		public void Add( Invoice invoice )
		{
			_invoice = invoice;
		}
	}
}