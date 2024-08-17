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

			if ( invoice == null )
			{
				throw new InvalidOperationException( "There is no invoice matching this payment" );
			}

			if ( invoice.Amount == 0 )
			{
				return ProcessInvoiceWithZeroAmount(invoice);
			}

			return ProcessInvoiceWithNonZeroAmount(payment, invoice);
			
		}

		private static string ProcessInvoiceWithNonZeroAmount(Payment payment, Invoice invoice)
		{
			var totalPaymentAmount = GetTotalPaymentAmount(invoice);
			var remainingAmount = GetRemainingAmount(invoice);
			
			if ( totalPaymentAmount > 0 )
			{
				return ProcessWithExistingPaymentBalance(payment, invoice, totalPaymentAmount, remainingAmount);
			}

			return ProcessWithoutExistingPaymentBalance(payment, invoice);
		}

		private static string ProcessWithoutExistingPaymentBalance(Payment payment, Invoice invoice)
		{
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

		private static string ProcessWithExistingPaymentBalance(Payment payment, Invoice invoice, decimal totalPaymentAmount,
			decimal remainingAmount)
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

		private static decimal GetRemainingAmount(Invoice invoice)
		{
			return invoice.Amount - invoice.AmountPaid;
		}

		private static decimal GetTotalPaymentAmount(Invoice invoice)
		{
			return invoice.Payments?.Sum(x => x.Amount) ?? 0;
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
			
			invoice.Save();
		}
	}
}