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
                //responseMessage = "the payment is greater than the partial amount remaining";
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
				responseMessage = "no payment needed";
				return responseMessage;
			}


			if (inv.Payments != null && inv.Payments.Any())
			{
				if (CheckInvoiceIsPaid(inv))
				{
                    responseMessage = "invoice was already fully paid";
                    return responseMessage;
                }
                
				if (CheckInvoiceOverpaid(inv, payment))
                {
					responseMessage = "the payment is greater than the partial amount remaining";
					return responseMessage;
                }

				if ((inv.Amount - inv.AmountPaid) == payment.Amount)
				{
					CalculatePayment(inv, payment);
                    responseMessage = "final partial payment received, invoice is now fully paid";
                }
				else
				{
                    CalculatePayment(inv, payment);
                    responseMessage = "another partial payment received, still not fully paid";
				}
			}
			else
			{
				if (payment.Amount > inv.Amount)
				{
					responseMessage = "the payment is greater than the invoice amount";
				}
				else if (inv.Amount == payment.Amount)
				{
					switch (inv.Type)
					{
						case InvoiceType.Standard:
							inv.AmountPaid = payment.Amount;
							inv.TaxAmount = payment.Amount * 0.14m;
							inv.Payments.Add(payment);
							responseMessage = "invoice is now fully paid";
							break;
						case InvoiceType.Commercial:
							inv.AmountPaid = payment.Amount;
							inv.TaxAmount = payment.Amount * 0.14m;
							inv.Payments.Add(payment);
							responseMessage = "invoice is now fully paid";
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				else
				{
					switch (inv.Type)
					{
						case InvoiceType.Standard:
							inv.AmountPaid = payment.Amount;
							inv.TaxAmount = payment.Amount * 0.14m;
							inv.Payments.Add(payment);
							responseMessage = "invoice is now partially paid";
							break;
						case InvoiceType.Commercial:
							inv.AmountPaid = payment.Amount;
							inv.TaxAmount = payment.Amount * 0.14m;
							inv.Payments.Add(payment);
							responseMessage = "invoice is now partially paid";
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}

            _invoiceRepository.SaveInvoice(inv);

			return responseMessage;
		}

    }
}