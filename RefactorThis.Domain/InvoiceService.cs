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

			if ( inv == null )
			{
				throw new InvalidOperationException( "There is no invoice matching this payment" );
			}

			if ( inv.Amount == 0 )
			{
				if ( inv.HasPayments( ) )
				{
					throw new InvalidOperationException(
						"The invoice is in an invalid state, it has an amount of 0 and it has payments.");
				}

				return "no payment needed";
			}

			return ( inv.HasPayments( ) )
				? ProcessPartialPayment( inv, payment )
				: ProcessFullPayment( inv, payment );
		}

		private static string ProcessPartialPayment( Invoice inv, Payment payment )
		{
			if ( inv.Amount == inv.Payments.Sum( x => x.Amount ) )
			{
				return "invoice was already fully paid";
			}

			if ( payment.Amount > ( inv.Amount - inv.AmountPaid ) )
			{
				return "the payment is greater than the partial amount remaining";
			}

			switch ( inv.Type )
			{
				case InvoiceType.Standard:
					// TODO: Discuss with stakeholders
					// I'm preserving existing behaviour here - but it seems wrong!
					// I suspect that either this is supposed to be taxed - or that Standard
					// invoices should not be taxed in the ProcessFullPayment() function either
					inv.AddPayment( payment, false );
					break;
				case InvoiceType.Commercial:
					inv.AddPayment( payment );
					break;
				default:
					throw new ArgumentOutOfRangeException( );
			}

			inv.Save( );

			return ( inv.AmountPaid == inv.Amount )
				? "final partial payment received, invoice is now fully paid"
				: "another partial payment received, still not fully paid";
		}

		private static string ProcessFullPayment( Invoice inv, Payment payment )
		{
			if ( payment.Amount > inv.Amount )
			{
				return "the payment is greater than the invoice amount";
			}

			switch ( inv.Type )
			{
				case InvoiceType.Standard:
				case InvoiceType.Commercial:
					inv.AddPayment( payment );
					break;
				default:
					throw new ArgumentOutOfRangeException( );
			}

			inv.Save( );

			return ( inv.Amount == payment.Amount )
				? "invoice is now fully paid"
				: "invoice is now partially paid";
		}
	}
}