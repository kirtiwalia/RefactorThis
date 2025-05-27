using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
	public class InvoiceService
	{
		private readonly InvoiceRepository _invoiceRepository;

		public InvoiceService( InvoiceRepository invoiceRepository )
		{
			_invoiceRepository = invoiceRepository;
		}

		public string ProcessPayment( Payment payment )
		{
			var inv = _invoiceRepository.GetInvoice( payment.Reference );

			var responseMessage = string.Empty;

			if ( inv == null )
			{
				throw new InvalidOperationException( "There is no invoice matching this payment" );
			}
			else
			{
				if ( inv.Amount == 0 )
                {
                    responseMessage = InvoiceAmountIsZero(inv);
                }
                else
				{
					if ( inv.Payments != null && inv.Payments.Count != 0)
					{
						if( inv.Payments.Sum(x=>x.Amount) != inv.AmountPaid )
						{
							inv.AmountPaid = inv.Payments.Sum(x => x.Amount);
						}
						if (inv.Payments.Sum(x => x.Amount) != 0 && inv.Amount == inv.AmountPaid)
						{
							responseMessage = "invoice was already fully paid";
						}
						else if (inv.Payments.Sum(x => x.Amount) != 0 && payment.Amount > (inv.Amount - inv.AmountPaid))
						{
							responseMessage = "the payment is greater than the partial amount remaining";
						}
						else
						{
							if ((inv.Amount - inv.AmountPaid) == payment.Amount)
							{
								responseMessage = "final partial payment received, invoice is now fully paid";
							}
							else
							{
								responseMessage = "another partial payment received, still not fully paid";
							}
                            AddPaymentToInvoice(payment, inv, false);
						}
					}
					else
					{
						if (payment.Amount > inv.Amount)
						{
							responseMessage = "the payment is greater than the invoice amount";
						}
						else
						{
							if (inv.Amount == payment.Amount)
							{
								responseMessage = "invoice is now fully paid";
							}
							else
							{
								responseMessage = "invoice is now partially paid";
							}
                            AddPaymentToInvoice(payment, inv, true);
						}
					}
				}
			}
			
			inv.Save();

			return responseMessage;
		}

        private static void AddPaymentToInvoice(Payment payment, Invoice inv, bool addTaxAmountToStandard)
        {
            switch (inv.Type)
            {
                case InvoiceType.Standard:
                    inv.AmountPaid += payment.Amount;
					inv.TaxAmount += addTaxAmountToStandard ? payment.Amount * 0.14m : 0;
                    inv.Payments.Add(payment);
                    break;
                case InvoiceType.Commercial:
                    inv.AmountPaid += payment.Amount;
                    inv.TaxAmount += payment.Amount * 0.14m;
                    inv.Payments.Add(payment);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string InvoiceAmountIsZero(Invoice inv)
        {
            string responseMessage;
            if (inv.Payments == null || !inv.Payments.Any())
            {
                responseMessage = "no payment needed";
            }
            else
            {
                throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
            }

            return responseMessage;
        }
    }
}