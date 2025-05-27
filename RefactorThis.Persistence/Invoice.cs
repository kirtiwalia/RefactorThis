using System.Collections.Generic;

namespace RefactorThis.Persistence
{
	public class Invoice
	{
		private readonly IInvoiceRepository _repository;
		public Invoice( IInvoiceRepository repository )
		{
			_repository = repository;
		}

		public void Save( )
		{
			_repository.SaveInvoice( this );
		}

		public decimal Amount { get; set; }
		public decimal AmountPaid { get; set; }
		public decimal TaxAmount { get; set; }
		public List<Payment> Payments { get; set; }
		
		public InvoiceType Type { get; set; }
	}

	public enum InvoiceType
	{
		Standard,
		Commercial
	}
}