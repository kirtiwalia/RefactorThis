using RefactorThis.Core.enums;
using System.Collections.Generic;

namespace RefactorThis.Core.entities
{
    public class Invoice
    {
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Payment> Payments { get; set; }
        public InvoiceType Type { get; set; }
    }

}