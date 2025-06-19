using System;
using System.Collections.Generic;
using System.Linq;
using RefactorThis.Domain.Entities;
using RefactorThis.Persistence.Interfaces;

namespace RefactorThis.Persistence.Repository {
	public class InvoiceRepository : IInvoiceRepository
	{
        //private readonly Dictionary<string, Invoice> _invoices = new Dictionary<string, Invoice>();
        private Invoice _invoice;

        public void Add(Invoice invoice)
        {
            _invoice = invoice;
        }

        public Invoice GetByReference(Invoice invoice)
        {
            return _invoice;
            //return reference != null && _invoices.TryGetValue(reference, out var invoice)
            //    ? invoice
            //    : null;
        }

        public void Save(Invoice invoice)
        {
            //return invoice;
        }
    }
}