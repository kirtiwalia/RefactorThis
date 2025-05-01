using RefactorThis.Persistence.Entities;
using RefactorThis.Persistence.Repositories;

namespace RefactorThis.Domain.Tests.Mocks
{
    public class MockInvoiceRepository : IInvoiceRepository
    {
        private Invoice _invoice;

        public Invoice GetInvoice(string reference)
        {
            return _invoice;
        }

        public void SaveInvoice(Invoice invoice)
        {
            // In memory implementation for testing
        }

        public void Add(Invoice invoice)
        {
            _invoice = invoice;
        }
    }
}