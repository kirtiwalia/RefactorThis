using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
	public class InvoiceService : IInvoiceService
	{
		private readonly InvoiceRepository _invoiceRepository;

		public InvoiceService( InvoiceRepository invoiceRepository )
		{
			_invoiceRepository = invoiceRepository;
		}

		public PaymentResult ProcessPayment( Payment payment )
		{
			var inv = _invoiceRepository.GetInvoice( payment.InvoiceId );

			if (inv == null)
			{
				return new PaymentResult(false, State.NoInvoiceFound);
			}

			if (inv.Amount == 0 && !inv.Payments.Any())
            {
                return new PaymentResult(false, State.NoPaymentRequred);
			}

            if (inv.Amount == 0 && inv.Payments.Any())
			{
                return new PaymentResult(false, State.InvalidState);
            }

			//
			if (inv.AmountPaid == inv.Amount)
			{
				return new PaymentResult(false, State.NoPaymentRequred);
			}

            if (inv.AmountPaid > inv.Amount)
			{
                return new PaymentResult(false, State.OverPaid);
            }

			if (payment.Amount > inv.Amount - inv.AmountPaid)
			{
                return new PaymentResult(false, State.GreaterThanRemainder);
            }

            switch (inv.Type)
            {
                case InvoiceType.Standard:
                    inv.AmountPaid += payment.Amount;
					if (!inv.Payments.Any())
					{
                        inv.TaxAmount = payment.Amount * 0.14m;
                    }
                    inv.Payments.Add(payment);
                    break;
                case InvoiceType.Commercial:
                    inv.AmountPaid += payment.Amount;
                    inv.TaxAmount += payment.Amount * 0.14m;
                    inv.Payments.Add(payment);
                    break;
                default:
					return new PaymentResult(false, State.Error);
            }

			if (inv.Amount == inv.AmountPaid)
			{
				return new PaymentResult(true, State.FullyPaid);
			}

			if (inv.Amount != inv.AmountPaid)
			{
				return new PaymentResult(true, State.PartialPaid);
			}

            _invoiceRepository.SaveInvoice(inv);

            return new PaymentResult(false, State.Error);
		}
	}
}