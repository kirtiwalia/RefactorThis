using RefactorThis.Application.Interfaces;
using RefactorThis.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.UnitTests
{
    public class FakeInvoiceRepository : IInvoiceRepository
    {
        private readonly List<Invoice> _invoices = new List<Invoice>();

        public void Add(Invoice invoice)
        {
            _invoices.Add(invoice);
        }

        public Invoice GetInvoiceByPayment(Payment payment)
        {
            // For simplicity, return first invoice (or null if none)
            return _invoices.FirstOrDefault();
        }

        public IEnumerable<Invoice> GetAllInvoices()
        {
            return _invoices;
        }

        public Invoice GetInvoice(string reference)
        {
            // Return invoice by reference if needed
            return _invoices.FirstOrDefault();
        }

        public void SaveInvoice(Invoice invoice)
        {
            // In-memory, nothing needed. Could replace the invoice if needed.
        }
    }
}
