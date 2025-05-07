using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence.models
{
	public class Invoice
	{
		public Guid Id { get; set; }
		public decimal Amount { get; set; }
		public decimal AmountPaid => Payments != null ? Payments.Sum(p => p.Amount) : 0;
		public decimal TaxAmount => Type == InvoiceType.Commercial && Payments != null ? Payments.Sum(p => p.Amount * 0.14m) : 0;
		public List<Payment> Payments { get; set; }
		public bool HasPayments => Payments != null && Payments.Any();
		
		public InvoiceType Type { get; set; }
	}

	public enum InvoiceType
	{
		Standard,
		Commercial
	}
}