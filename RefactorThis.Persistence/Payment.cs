using System;

namespace RefactorThis.Persistence
{
	public class Payment
	{
		public decimal Amount { get; set; }
		public Guid InvoiceId { get; set; }
	}
}