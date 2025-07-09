using RefactorThis.Core.entities;
using RefactorThis.Core.Interfaces;

namespace RefactorThis.Persistence {
	public class InvoiceRepository : IInvoiceRepository
    {
		private Invoice _invoice = new Invoice();

		public Invoice GetInvoice( string reference )
		{
			return  _invoice;
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