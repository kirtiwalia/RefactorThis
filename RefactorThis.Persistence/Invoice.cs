using System;
using System.Collections.Generic;

namespace RefactorThis.Persistence
{
	public class Invoice
	{
		public Invoice()
		{
			Id = Guid.NewGuid();
		}

		public Guid Id { get; private set; }
		public decimal Amount { get; set; }
		public decimal TaxAmount { get; set; }
		public InvoiceType Type { get; set; }
	}

	public enum InvoiceType
	{
		Standard,
		Commercial
	}
}