using System;
using RefactorThis.Persistence.models;

namespace RefactorThis.Persistence.repositories {
	// This is very much a placeholder class. It needs to be implemented correctly for the repository we end up using. It is currently ignored in all tests.
	public class InvoiceRepository : IInvoiceRepository
	{
		private Invoice _invoice;

		public Invoice GetInvoice( Guid id )
		{
			return _invoice;
		}

        public Invoice FindInvoiceWithPayment(string paymentReference)
        {
            return _invoice;
        }

        public void SaveInvoice( Invoice invoice )
		{
			//saves the invoice to the database
		}

		// I have removed Add because it isn't necessary. If we want to Add an invoice, we can just Save a new invoice.
    }
}