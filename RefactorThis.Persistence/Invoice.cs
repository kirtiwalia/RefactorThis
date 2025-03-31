using System;
using System.Collections.Generic;

namespace RefactorThis.Persistence
{
	public class Invoice
	{
public Invoice()
{
Id = Guid.NewGuid();
			Payments = new List<Payment>();
}

public Guid Id { get; private set; }
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