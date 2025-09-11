using RefactorThis.Domain.Enums;
using System.Collections.Generic;

namespace RefactorThis.Domain.Models
{
	/// <summary>
	/// Represents an invoice
	/// </summary>
	public class Invoice
	{
		/// <summary>
		/// The total amount due on the invoice
		/// </summary>
		public decimal Amount { get; set; } = 0;
		/// <summary>
		/// The total amount that has already been paid towards the invoice
		/// </summary>
		public decimal AmountPaid { get; set; } = 0;
		/// <summary>
		/// The tax portion of the invoice amount
		/// </summary>
		public decimal TaxAmount { get; set; } = 0;
		/// <summary>
		/// A list of payments that have been made towards the invoice
		/// </summary>
		public List<Payment> Payments { get; set; }
		/// <summary>
		/// The type of the invoice
		/// </summary>
		public InvoiceType Type { get; set; }
	}
}