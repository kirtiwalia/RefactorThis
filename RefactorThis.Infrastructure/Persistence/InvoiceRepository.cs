
using RefactorThis.Application.Interfaces;
using RefactorThis.Domain.Entities;

namespace RefactorThis.Infrastructure.Persistence
{
    public class InvoiceRepository : IInvoiceRepository
    {
		private Invoice _invoice;

		public Invoice GetInvoice( string reference )
		{
			return _invoice;
		}

		public void Add( Invoice invoice )
		{
			_invoice = invoice;
		}

        public void SaveInvoice(Invoice invoice)
        {
            //saves the invoice to the database
        }
    }
}