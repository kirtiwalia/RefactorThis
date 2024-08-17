using System;
using System.Linq;
using RefactorThis.Persistence.Model;
using RefactorThis.Persistence.Repository;

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
			var invoice = _invoiceRepository.GetInvoice( payment.Reference );

			var responseMessage = string.Empty;

			if ( invoice == null )
			{
				throw new InvalidOperationException( "There is no invoice matching this payment" );
			}

			if ( invoice.Amount == 0 )
			{
				responseMessage = ProcessInvoiceWithZeroAmount(invoice);
			}
			else
			{
				responseMessage = ProcessInvoiceWithNonZeroAmount(payment, invoice);
			}

			invoice.Save();

			return responseMessage;
		}

		private static string ProcessInvoiceWithNonZeroAmount(Payment payment, Invoice invoice)
		{
			string responseMessage;
			var totalPaymentAmount = invoice.Payments?.Sum(x => x.Amount) ?? 0;
			var remainingAmount = invoice.Amount - invoice.AmountPaid;
			
			if ( totalPaymentAmount > 0 )
			{
				if ( invoice.Amount == totalPaymentAmount )
				{
					return "invoice was already fully paid";
				}
				
				if ( payment.Amount > remainingAmount )
				{
					return "the payment is greater than the partial amount remaining";
				}

				if ( payment.Amount == remainingAmount )
				{
					UpdateInvoice(invoice, payment);
					return "final partial payment received, invoice is now fully paid";
				}

				UpdateInvoice(invoice, payment);
				return "another partial payment received, still not fully paid";
			}

			if ( payment.Amount > invoice.Amount )
			{
				return "the payment is greater than the invoice amount";
			}
				
			if ( payment.Amount == invoice.Amount )
			{
				UpdateInvoice(invoice, payment);
				return "invoice is now fully paid";
			}

			UpdateInvoice(invoice, payment);
			return "invoice is now partially paid";
		}

		private static string ProcessInvoiceWithZeroAmount(Invoice invoice)
		{
			if ( invoice.Payments != null && invoice.Payments.Any( ) )
			{
				throw new InvalidOperationException( "The invoice is in an invalid state, it has an amount of 0 and it has payments." );
			}
			
			return "no payment needed";
		}
		
		private static void UpdateInvoice(Invoice invoice, Payment payment)
		{
			invoice.AmountPaid += payment.Amount;
			invoice.Payments.Add(payment);

			if (invoice.Type == InvoiceType.Commercial)
			{
				invoice.TaxAmount += payment.Amount * 0.14m;
			}
		}
	}
}