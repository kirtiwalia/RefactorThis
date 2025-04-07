using System;
using RefactorThis.Domain.Services.Payments;
using RefactorThis.Persistence.Models;
using RefactorThis.Persistence.Repositories;

namespace RefactorThis.Domain.Services.Invoices
{
	public class InvoiceService : IInvoiceService
	{
		private readonly IInvoiceRepository _invoiceRepository;
		private readonly IPaymentService _paymentService;

		public InvoiceService(IInvoiceRepository invoiceRepository, IPaymentService paymentService)
		{
			_invoiceRepository = invoiceRepository;
			_paymentService = paymentService;
		}
		
		public string ProcessPayment(Payment payment)
		{
			var invoice = _invoiceRepository.GetById(payment.InvoiceId);
			ValidateInvoice(invoice);
			
			var paymentResult = _paymentService.ProcessPayment(invoice, payment);
			
			_invoiceRepository.Save(paymentResult.ProcessedInvoice);
			return paymentResult.ResponseMessage;
		}

		private static void ValidateInvoice(Invoice invoice)
		{
			if (invoice == null)
			{
				throw new InvalidOperationException("There is no invoice matching this payment");
			}

			if (invoice.Amount == 0 && InvoiceHasPayments(invoice))
			{
				throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
			}
			
			if (invoice.AmountPaid != 0 && !InvoiceHasPayments(invoice))
			{
				throw new InvalidOperationException("The invoice is in an invalid state, it has an amount paid, but no payments.");
			}

			if (invoice.Type != InvoiceType.Standard && invoice.Type != InvoiceType.Commercial)
			{
				throw new ArgumentOutOfRangeException(nameof(invoice.Type), "Unsupported invoice type");
			}
		}
		
		private static bool InvoiceHasPayments(Invoice invoice) =>
			invoice.Payments != null && invoice.Payments.Count > 0;
	}
}