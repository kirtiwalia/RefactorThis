using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefactorThis.Domain.ValueObject;

namespace RefactorThis.Domain.Entities
{
    public class Invoice
    {
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Payment> Payments { get; set; } = new List<Payment>();
        public InvoiceType Type { get; set; }

        public string ApplyPayment(Payment payment = null)
        {
            if (Amount == 0 && (Payments == null || Payments.Count == 0))
                return "no payment needed";

            if (Payments.Sum(p => p.Amount) != 0 && Amount == Payments.Sum(p => p.Amount))
                return "invoice was already fully paid";

            if (Payments.Sum(p => p.Amount) != 0 && payment.Amount > (Amount - AmountPaid))
                return "the payment is greater than the partial amount remaining";

            if (Payments.Count == 0 && payment.Amount > Amount)
                return "the payment is greater than the invoice amount";

            var remaining = Amount - AmountPaid;

            if (payment.Amount == remaining)
            {
                Apply(payment);
                return "final partial payment received, invoice is now fully paid";
            }
            else if (Payments.Count > 0 && payment.Amount < remaining)
            {
                Apply(payment);
                return "another partial payment received, still not fully paid";
            }
            else if (Payments.Count == 0 && payment.Amount == Amount)
            {
                Apply(payment);
                return "invoice is now fully paid";
            }
            else if (Payments.Count == 0 && payment.Amount < Amount)
            {
                Apply(payment);
                return "invoice is now partially paid";
            }
            throw new InvalidOperationException("Unhandled payment case");
        }

        private void Apply(Payment payment)
        {
            AmountPaid += payment.Amount;
            if (Type == InvoiceType.Commercial || Type == InvoiceType.Standard)
                TaxAmount += payment.Amount * 0.14m;
            Payments.Add(payment);
        }
    }
}
