namespace RefactorThis.Domain.Invoices.Interfaces
{
    public interface IInvoiceRepository
    {
        Invoice GetInvoice(string reference);
        void SaveInvoice(Invoice invoice);
    }
}
