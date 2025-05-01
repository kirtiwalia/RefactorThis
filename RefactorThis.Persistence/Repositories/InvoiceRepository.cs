using System;
using RefactorThis.Persistence.Entities;

namespace RefactorThis.Persistence.Repositories 
{
	public class InvoiceRepository : IInvoiceRepository
	{
		public Invoice GetInvoice( string reference )
		{
			throw new NotImplementedException();
		}

		public void SaveInvoice( Invoice invoice )
		{
			//saves the invoice to the database
			throw new NotImplementedException();
		}

		public void Add( Invoice invoice )
		{
			throw new NotImplementedException();
		}
	}
}