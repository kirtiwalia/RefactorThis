using RefactorThis.Persistence.Entity;

namespace RefactorThis.Persistence.Interface
{
	public interface IInvoiceRepository
	{
		Invoice GetInvoice(string reference);
		void SaveInvoice(Invoice invoice);
		void Add(Invoice invoice);
	}
}