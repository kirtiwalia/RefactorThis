using System;

namespace RefactorThis.Persistence
{
	public interface IInvoiceRepository
	{
		Invoice GetInvoice(Guid id);
		void SaveInvoice(Invoice invoice);
	}
}