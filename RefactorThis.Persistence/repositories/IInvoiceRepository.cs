using RefactorThis.Persistence.models;

namespace RefactorThis.Persistence.repositories
{
    public interface IInvoiceRepository
    {
        Invoice GetByReference(string reference);
        void Update(Invoice invoice);
        void Add(Invoice invoice);
    }
}
