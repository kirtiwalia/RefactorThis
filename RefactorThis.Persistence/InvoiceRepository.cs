using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence {
	public class InvoiceRepository : IInvoiceRepository
	{
		private readonly IList<Invoice> _invoice;

		public InvoiceRepository()
		{
			_invoice = new List<Invoice>();
		}

		public Invoice GetInvoice( Guid id )
		{
			return _invoice.FirstOrDefault( x => x.Id == id );
		}

		public void SaveInvoice( Invoice invoice )
		{
			_invoice.Add(invoice);
		}
	}
}