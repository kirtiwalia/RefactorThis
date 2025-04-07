using System;
using System.Collections.Generic;

namespace RefactorThis.Persistence.Models
{
	public class Invoice
	{
		public Guid Id { get; set; }
		public decimal Amount { get; set; }
		public decimal AmountPaid { get; set; }
		public decimal TaxAmount { get; set; }
		public List<Payment> Payments { get; set; } = new List<Payment>();
		
		public InvoiceType Type { get; set; }
	}

	public enum InvoiceType
	{
		Standard,
		Commercial
	}
}