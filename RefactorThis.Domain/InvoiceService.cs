using System;
using System.Collections.Generic;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
	public class InvoiceService
	{
		private readonly InvoiceRepository _invoiceRepository;

		public InvoiceService(InvoiceRepository invoiceRepository)
		{
			_invoiceRepository = invoiceRepository;
		}

		public string ProcessPayment(Payment payment)
        {
            var inv = _invoiceRepository.GetInvoice(payment.Reference);

            var message = HandleInvalidPayments(inv);

            if(message !="")
                return message;

            var totalPaid = inv.Payments.Sum(x => x.Amount);
            var remaining = inv.Amount - inv.AmountPaid;

            if (totalPaid != 0 && inv.Amount == totalPaid)
            {
                return "invoice was already fully paid";
            }
            if (totalPaid != 0 && payment.Amount > remaining)
            {
                return "the payment is greater than the partial amount remaining";
            }
            if (totalPaid == 0 && payment.Amount > inv.Amount)
            {
                return "the payment is greater than the invoice amount";
            }

            // Apply the payment
            ApplyPayment(inv, payment);

            _invoiceRepository.SaveInvoice(inv);

            var isFinal = (remaining == payment.Amount || inv.Amount == payment.Amount);

            if (isFinal)
            {
                if (inv.Payments.Count > 1)
                {
                    return "final partial payment received, invoice is now fully paid";
                }
                else
                {
                    return "invoice is now fully paid";
                }
            }
            else
            {

                if (inv.Payments.Count > 1)
                {
                    return "another partial payment received, still not fully paid";
                }
                else
                {
                    return "invoice is now partially paid";
                }
            }
        }

        private static string HandleInvalidPayments(Invoice inv)
        {
            if (inv == null)
            {
                return "There is no invoice matching this payment";
            }
            if (inv.Payments == null)
            {
                inv.Payments = new List<Payment>();
            }
            if (inv.Amount == 0 && !inv.Payments.Any())
            {
                return "no payment needed";
            }
            if (inv.Amount == 0 && inv.Payments.Any())
            {
                return "The invoice is in an invalid state, it has an amount of 0 and it has payments.";
            }
            return "";
        }

        private void ApplyPayment(Invoice inv, Payment payment)
		{
			inv.AmountPaid += payment.Amount;
			inv.Payments.Add(payment);

			if (inv.Type == InvoiceType.Standard || inv.Type == InvoiceType.Commercial)
			{
				inv.TaxAmount += payment.Amount * 0.14m;
			}
			else
			{
				throw new ArgumentOutOfRangeException();
			}
		}
	}
}