namespace RefactorThis.Persistence
{
    public interface IInvoiceRepository
    {
        Invoice GetInvoice(string reference);
        void SaveInvoice(Invoice invoice);
        void AddInvoice(Invoice invoice);
    }

    public class InvoiceRepository : IInvoiceRepository
    {
        private Invoice _invoice;

        public Invoice GetInvoice(string reference)
        {
            return _invoice;
        }

        public void SaveInvoice(Invoice invoice)
        {
            //saves the invoice to the database
        }

        public void AddInvoice(Invoice invoice)
        {
            _invoice = invoice;
        }
    }
}