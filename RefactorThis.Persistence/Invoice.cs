using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence
{
	public class Invoice
	{
		private readonly InvoiceRepository _repository;
		public Invoice( InvoiceRepository repository )
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

		public bool HasPayments( )
		{
			return Payments != null && Payments.Any( );
		}
		public void AddPayment( Payment payment, bool isTaxable = true )
		{
			AmountPaid += payment.Amount;
			if (isTaxable)
			{
				TaxAmount += payment.Amount * 0.14m;
			}
			Payments.Add( payment );
		}
	}

	public enum InvoiceType
	{
		Standard,
		Commercial
	}
}