using System;

namespace RefactorThis.Persistence.Models
{
	public class Payment
	{
		public Guid Id { get; set; }
		public decimal Amount { get; set; }
		public string Reference { get; set; }
		public DateTime CreatedOn { get; set; }
		public Guid InvoiceId { get; set; }
		public Invoice Invoice { get; set; }
	}
}