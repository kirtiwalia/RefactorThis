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
			var invoice = _invoiceRepository.GetInvoice( payment.Reference );

			var responseMessage = string.Empty;

			if ( invoice == null ) // no invoice found for the payment
			{
				throw new InvalidOperationException( "There is no invoice matching this payment" );
			}

			if ( invoice.Amount == 0) // no payment needed invoice is zero
            {
				if ( invoice.Payments == null || !invoice.Payments.Any( ) ) // invoice is paid no payment needed
				{
					responseMessage = "no payment needed";
				}
				else // how did we get here? 
				{
					throw new InvalidOperationException( "The invoice is in an invalid state, it has an amount of 0 and it has payments." );
				}
			}
			else if ( invoice.Payments != null && invoice.Payments.Any( )) // invoice has payments already
            {
				var totalPaid = invoice.Payments.Sum( x => x.Amount ); // total amount paid so far
                var AmmountRemaining = invoice.Amount - invoice.AmountPaid; // remaining amount to be paid for the invoice

				if (totalPaid != 0 && invoice.Amount == totalPaid) // invoice already paid in full
				{
					responseMessage = "invoice was already fully paid";
				}
				else if (totalPaid != 0 && payment.Amount > AmmountRemaining) // payment is greater than the remaining amount to be paid
				{
					responseMessage = "the payment is greater than the partial amount remaining";
				}
                else // remaining payment logic to handle finally paid or partial payments
                {
                    ApplyPayment(invoice, payment);
                    responseMessage = AmmountRemaining == payment.Amount
                        ? "final partial payment received, invoice is now fully paid" // final payment received
                        : "another partial payment received, still not fully paid"; // another partial payment received
                }

            }
            else // invoice has no payments yet
            {
				if ( payment.Amount > invoice.Amount) // payment is greater than the invoice amount
                {
					responseMessage = "the payment is greater than the invoice amount";
				}
                else // first payment logic to handle payments in full or partial
                {
                    ApplyPayment(invoice, payment);
                    responseMessage = payment.Amount == invoice.Amount
                        ? "invoice is now fully paid" // payment matches the invoice amount payment received
                        : "invoice is now partially paid"; // payment is less than the invoice amount partial payment received
                }
            }
			
			invoice.Save();
			return responseMessage;
		}

        private void ApplyPayment(Invoice invoice, Payment payment)
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
