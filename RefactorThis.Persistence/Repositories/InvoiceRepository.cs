using System;
using System.Collections.Generic;
using System.Linq;
using RefactorThis.Persistence.Models;

namespace RefactorThis.Persistence.Repositories {
	public class InvoiceRepository : IInvoiceRepository
	{
		private readonly ICollection<Invoice> _invoices = new List<Invoice>();

		public Invoice GetById(Guid id)
		{
			return _invoices.SingleOrDefault(inv => inv.Id == id);
		}

		public void Save(Invoice invoice)
		{
			//saves the invoice to the database
		}

		public void Add(Invoice invoice)
		{
			_invoices.Add(invoice);
		}
	}
}