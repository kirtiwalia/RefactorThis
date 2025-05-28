using System.Collections.Generic;
using RefactorThis.Persistence.enums;

namespace RefactorThis.Persistence.models
{
    /// <summary>
    /// Represents an invoice with details about the amount, tax, payments, and type.
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// The total amount due for the invoice, before any payments or taxes.
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// The total amount that has been paid towards the invoice.
        /// </summary>
        public decimal AmountPaid { get; set; }
        
        /// <summary>
        /// The amount of tax included in the invoice.
        /// </summary>
        public decimal TaxAmount { get; set; }
        
        /// <summary>
        /// A list of individual payments made towards the invoice.
        /// </summary>
        public List<Payment> Payments { get; set; }
        
        /// <summary>
        /// The type of the invoice (Standard, Commercial)
        /// </summary>
        public InvoiceType Type { get; set; }
    }
}