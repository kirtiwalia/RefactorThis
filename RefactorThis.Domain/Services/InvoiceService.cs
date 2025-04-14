using System;
using System.Linq;
using RefactorThis.Persistence.Enums;
using RefactorThis.Persistence.Models;
using RefactorThis.Persistence.Repositories;

namespace RefactorThis.Domain.Services
{
    public class InvoiceService
	{
		private readonly InvoiceRepository _invoiceRepository;

		public InvoiceService( InvoiceRepository invoiceRepository )
		{
			_invoiceRepository = invoiceRepository;
		}
        private void CheckInvoiceIsNull(Invoice inv)
        {
            if (inv == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }
        }
		private bool CheckInvoiceIsZero(Invoice inv)
        {
            if (inv.Amount == 0)
            {
                if (inv.Payments == null || !inv.Payments.Any())
                {
                    return true;
                }
                else
                {
                    throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
                }
            }
			return false;
        }
		private bool CheckInvoiceIsPaid(Invoice inv)
		{
            if (inv.Payments.Sum(x => x.Amount) != 0 && inv.Amount == inv.Payments.Sum(x => x.Amount))
            {
				return true;
            }
            return false;
        }
		private bool CheckInvoiceOverpaid(Invoice inv, Payment payment)
		{

            if (inv.Payments.Sum(x => x.Amount) != 0 && payment.Amount > (inv.Amount - inv.AmountPaid))
            {
				return true;
            }
            return false;
        }
		private bool CheckPaymentHistory(Invoice inv)
		{
            if (inv.Payments != null && inv.Payments.Any())
			{
				return true;
			}
			return false;
        }

		private void CalculatePayment(Invoice inv, Payment payment)
		{
			try
			{
				inv.AmountPaid += payment.Amount;
				if (inv.Type == InvoiceType.Commercial) inv.TaxAmount += payment.Amount * 0.14m;
				inv.Payments.Add(payment);
                _invoiceRepository.SaveInvoice(inv);
            }
            catch (Exception)
			{
				throw new ArgumentOutOfRangeException();
			}
        }

        private void CalculateFirstPayment(Invoice inv, Payment payment)
		{
            try
            {
                inv.AmountPaid = payment.Amount;
                inv.TaxAmount = payment.Amount * 0.14m;
                inv.Payments.Add(payment);
                _invoiceRepository.SaveInvoice(inv);
            }
            catch (Exception)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public string ProcessPayment( Payment payment )
		{
			var inv = _invoiceRepository.GetInvoice( payment.Reference );

			var responseMessage = string.Empty;

			CheckInvoiceIsNull(inv);

			if (CheckInvoiceIsZero(inv))
			{
				return "no payment needed";
			}

			if (CheckPaymentHistory(inv))
			{
				if (CheckInvoiceIsPaid(inv))
				{
                    return "invoice was already fully paid";                              
				}
                
				if (CheckInvoiceOverpaid(inv, payment))
                {
					return "the payment is greater than the partial amount remaining";
                }

                if ((inv.Amount - inv.AmountPaid) == payment.Amount)
				{
                    CalculatePayment(inv, payment);
                    return "final partial payment received, invoice is now fully paid";
                }
				else
				{
                    CalculatePayment(inv, payment);
                    return "another partial payment received, still not fully paid";
				}
			}

            if (payment.Amount > inv.Amount){ 
                return "the payment is greater than the invoice amount"; 
            }

            CalculateFirstPayment(inv, payment);

            if (inv.Amount == payment.Amount)
			{
                return "invoice is now fully paid";
            }
			else
			{
				return "invoice is now partially paid";
			}
		}
    }
}