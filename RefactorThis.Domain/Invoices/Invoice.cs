using RefactorThis.Domain.InvoiceTypes;
using RefactorThis.Domain.Payments;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Domain.Invoices
{
    public class Invoice
    {
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Payment> Payments { get; set; } = new List<Payment>();
        public InvoiceType Type { get; set; }
        public decimal TotalAmountPaid
        {
            get
            {
                return Payments?.Sum(x => x.Amount) ?? 0;
            }
        }

        public decimal Balance
        {
            get
            {
                return Amount - AmountPaid;
            }
        }

        public bool FullyPaid
        {
            get
            {
                return TotalAmountPaid == Amount;
            }
        }

        public bool HasPayments() => Payments != null && Payments.Any();
    }
}
