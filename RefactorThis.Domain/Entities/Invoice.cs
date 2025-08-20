using RefactorThis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Domain.Entities
{
    public class Invoice
    {
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Payment> Payments { get; set; }
        public InvoiceType Type { get; set; }
        public decimal TotalPaid => Payments.Where(p => p != null).Sum(p => p.Amount);
        public bool IsFullyPaid => Payments.Sum(p => p.Amount) == Amount;
        public decimal RemainingAmount => Amount - TotalPaid;
    }
}