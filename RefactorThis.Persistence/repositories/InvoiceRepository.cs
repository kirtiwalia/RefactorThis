using RefactorThis.Persistence.models;

namespace RefactorThis.Persistence.repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private Invoice _invoice;

        public Invoice GetByReference(string reference)
        {
            return _invoice;
        }

        public void Update(Invoice invoice)
        {
            // saves the invoice to the database
        }

        public void Add(Invoice invoice)
        {
            _invoice = invoice;
        }
    }
}