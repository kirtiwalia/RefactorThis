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
			decimal totalPaymentAmount = 0;
			var remainingAmount = invoice.Amount - invoice.AmountPaid;
			
			if (invoice.Payments != null)
			{
				totalPaymentAmount = invoice.Payments.Sum(x => x.Amount);
			}
			
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
					switch ( invoice.Type )
					{
						case InvoiceType.Standard:
							invoice.AmountPaid += payment.Amount;
							invoice.Payments.Add( payment );
							responseMessage = "final partial payment received, invoice is now fully paid";
							break;
						case InvoiceType.Commercial:
							invoice.AmountPaid += payment.Amount;
							invoice.TaxAmount += payment.Amount * 0.14m;
							invoice.Payments.Add( payment );
							responseMessage = "final partial payment received, invoice is now fully paid";
							break;
						default:
							throw new ArgumentOutOfRangeException( );
					}
								
				}
				else
				{
					switch ( invoice.Type )
					{
						case InvoiceType.Standard:
							invoice.AmountPaid += payment.Amount;
							invoice.Payments.Add( payment );
							responseMessage = "another partial payment received, still not fully paid";
							break;
						case InvoiceType.Commercial:
							invoice.AmountPaid += payment.Amount;
							invoice.TaxAmount += payment.Amount * 0.14m;
							invoice.Payments.Add( payment );
							responseMessage = "another partial payment received, still not fully paid";
							break;
						default:
							throw new ArgumentOutOfRangeException( );
					}
				}
			}
			else
			{
				if ( payment.Amount > invoice.Amount )
				{
					return "the payment is greater than the invoice amount";
				}
				
				if ( payment.Amount == invoice.Amount )
				{
					switch ( invoice.Type )
					{
						case InvoiceType.Standard:
							invoice.AmountPaid = payment.Amount;
							invoice.TaxAmount = payment.Amount * 0.14m;
							invoice.Payments.Add( payment );
							responseMessage = "invoice is now fully paid";
							break;
						case InvoiceType.Commercial:
							invoice.AmountPaid = payment.Amount;
							invoice.TaxAmount = payment.Amount * 0.14m;
							invoice.Payments.Add( payment );
							responseMessage = "invoice is now fully paid";
							break;
						default:
							throw new ArgumentOutOfRangeException( );
					}
				}
				else
				{
					switch ( invoice.Type )
					{
						case InvoiceType.Standard:
							invoice.AmountPaid = payment.Amount;
							invoice.TaxAmount = payment.Amount * 0.14m;
							invoice.Payments.Add( payment );
							responseMessage = "invoice is now partially paid";
							break;
						case InvoiceType.Commercial:
							invoice.AmountPaid = payment.Amount;
							invoice.TaxAmount = payment.Amount * 0.14m;
							invoice.Payments.Add( payment );
							responseMessage = "invoice is now partially paid";
							break;
						default:
							throw new ArgumentOutOfRangeException( );
					}
				}
			}

			return responseMessage;
		}

		private static string ProcessInvoiceWithZeroAmount(Invoice invoice)
		{
			if ( invoice.Payments != null && invoice.Payments.Any( ) )
			{
				throw new InvalidOperationException( "The invoice is in an invalid state, it has an amount of 0 and it has payments." );
			}
			
			return "no payment needed";
		}
	}
}