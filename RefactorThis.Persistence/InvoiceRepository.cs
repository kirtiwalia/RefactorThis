namespace RefactorThis.Persistence
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private Invoice _invoice;

        public Invoice GetInvoice(string reference)
        {
            return _invoice;
        }

        public void SaveInvoice(Invoice invoice)
        {
            // Saves the invoice to the database
        }

        public void Add(Invoice invoice)
        {
            _invoice = invoice;
        }
    }
}
