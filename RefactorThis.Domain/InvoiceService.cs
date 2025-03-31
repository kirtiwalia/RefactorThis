using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
	public class InvoiceService : IInvoiceService
	{
		private readonly InvoiceRepository _invoiceRepository;
		private readonly PaymentRepository _paymentRepository;

		public InvoiceService( InvoiceRepository invoiceRepository, PaymentRepository paymentRepository)
		{
			_invoiceRepository = invoiceRepository;
			_paymentRepository = paymentRepository;
		}

		public PaymentResult ProcessPayment( Payment payment )
		{
			var inv = _invoiceRepository.GetInvoice( payment.InvoiceId );
			var payments = _paymentRepository.GetPaymentsByInvoiceId( payment.InvoiceId );
			var amountPaid = payments.Sum(x => x.Amount);

			if (inv == null)
			{
				return new PaymentResult(false, State.NoInvoiceFound);
			}

			if (inv.Amount == 0 && !payments.Any())
			{
				return new PaymentResult(false, State.NoPaymentRequred);
			}

			if (inv.Amount == 0 && payments.Any())
			{
				return new PaymentResult(false, State.InvalidState);
			}

			//
			if (amountPaid == inv.Amount)
			{
				return new PaymentResult(false, State.NoPaymentRequred);
			}

			if (amountPaid > inv.Amount)
			{
				return new PaymentResult(false, State.OverPaid);
			}

			if (payment.Amount > inv.Amount - amountPaid)
			{
				return new PaymentResult(false, State.GreaterThanRemainder);
			}

			switch (inv.Type)
			{
				case InvoiceType.Standard:
					if (!payments.Any())
					{
						inv.TaxAmount = payment.Amount * 0.14m;
					}
					break;
				case InvoiceType.Commercial:
					inv.TaxAmount += payment.Amount * 0.14m;
					break;
				default:
					return new PaymentResult(false, State.Error);
			}

			_paymentRepository.SavePayment(payment);

			var totalPayment = _paymentRepository.GetPaymentsByInvoiceId(payment.InvoiceId).Select(x => x.Amount).Sum();

			if (inv.Amount == totalPayment)
			{
				return new PaymentResult(true, State.FullyPaid);
			}

			if (inv.Amount != totalPayment)
			{
				return new PaymentResult(true, State.PartialPaid);
			}

			_invoiceRepository.SaveInvoice(inv);

			return new PaymentResult(false, State.Error);
		}
	}
}