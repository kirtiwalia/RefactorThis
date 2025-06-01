using RefactorThis.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Domain
{
	public class InvoiceService
	{
		private readonly InvoiceRepository _invoiceRepository;

		public InvoiceService( InvoiceRepository invoiceRepository )
		{
			_invoiceRepository = invoiceRepository;
		}

        // Entry point for processing a payment against an invoice
        public string ProcessPayment(Payment payment)
        {
            // Step 1: Get the invoice by reference
            var invoice = _invoiceRepository.GetInvoice(payment.Reference)
                         ?? throw new InvalidOperationException("There is no invoice matching this payment");

            // Step 2: Ensure invoice is not in an invalid state
            ValidateInvoiceState(invoice);

            // Step 3: Return early if payment is unnecessary
            if (IsNoPaymentNeeded(invoice))
                return "no payment needed";

            // Step 4: Return if already fully paid
            if (IsFullyPaid(invoice))
                return "invoice was already fully paid";

            // Step 5: Reject overpayment
            if (IsOverpayment(invoice, payment))
                return invoice.Payments.Any()
                    ? "the payment is greater than the partial amount remaining"
                    : "the payment is greater than the invoice amount";

            // Step 6: Apply the payment to the invoice
            ApplyPayment(invoice, payment);

            // Step 7: Save changes
            invoice.Save();

            // Step 8: Return appropriate response message
            return GeneratePaymentMessage(invoice, payment);
        }

        // Check for inconsistent invoice state
        private void ValidateInvoiceState(Invoice invoice)
        {
            if (invoice.Amount == 0 && invoice.Payments != null && invoice.Payments.Any())
                throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }

        private bool IsNoPaymentNeeded(Invoice invoice)
            => invoice.Amount == 0 && (invoice.Payments == null || !invoice.Payments.Any());

        private bool IsFullyPaid(Invoice invoice)
            => invoice.Payments?.Sum(p => p.Amount) == invoice.Amount;

        private bool IsOverpayment(Invoice invoice, Payment payment)
            => payment.Amount > (invoice.Amount - invoice.AmountPaid);

        // Apply the payment and update fields accordingly
        private void ApplyPayment(Invoice invoice, Payment payment)
        {
            if (invoice.Payments == null)
                invoice.Payments = new List<Payment>();

            invoice.AmountPaid += payment.Amount;
            invoice.Payments.Add(payment);

            // Apply tax to both Standard and Commercial types
            if (invoice.Type == InvoiceType.Commercial || invoice.Type == InvoiceType.Standard)
            {
                invoice.TaxAmount += payment.Amount * 0.14m;
            }
        }

        // Generate a readable status message based on the current invoice state
        private string GeneratePaymentMessage(Invoice invoice, Payment payment)
        {
            var remaining = invoice.Amount - invoice.AmountPaid;

            if (remaining == 0 && invoice.AmountPaid == invoice.Amount)
                return invoice.Payments.Count > 1 ? "final partial payment received, invoice is now fully paid" : "invoice is now fully paid";

            return invoice.Payments.Count > 1
                ? "another partial payment received, still not fully paid"
                : "invoice is now partially paid";
        }

        //public string ProcessPayment( Payment payment )
        //{
        //	var inv = _invoiceRepository.GetInvoice( payment.Reference );

        //	var responseMessage = string.Empty;

        //	if ( inv == null )
        //	{
        //		throw new InvalidOperationException( "There is no invoice matching this payment" );
        //	}
        //	else
        //	{
        //		if ( inv.Amount == 0 )
        //		{
        //			if ( inv.Payments == null || !inv.Payments.Any( ) )
        //			{
        //				responseMessage = "no payment needed";
        //			}
        //			else
        //			{
        //				throw new InvalidOperationException( "The invoice is in an invalid state, it has an amount of 0 and it has payments." );
        //			}
        //		}
        //		else
        //		{
        //			if ( inv.Payments != null && inv.Payments.Any( ) )
        //			{
        //				if ( inv.Payments.Sum( x => x.Amount ) != 0 && inv.Amount == inv.Payments.Sum( x => x.Amount ) )
        //				{
        //					responseMessage = "invoice was already fully paid";
        //				}
        //				else if ( inv.Payments.Sum( x => x.Amount ) != 0 && payment.Amount > ( inv.Amount - inv.AmountPaid ) )
        //				{
        //					responseMessage = "the payment is greater than the partial amount remaining";
        //				}
        //				else
        //				{
        //					if ( ( inv.Amount - inv.AmountPaid ) == payment.Amount )
        //					{
        //						switch ( inv.Type )
        //						{
        //							case InvoiceType.Standard:
        //								inv.AmountPaid += payment.Amount;
        //								inv.Payments.Add( payment );
        //								responseMessage = "final partial payment received, invoice is now fully paid";
        //								break;
        //							case InvoiceType.Commercial:
        //								inv.AmountPaid += payment.Amount;
        //								inv.TaxAmount += payment.Amount * 0.14m;
        //								inv.Payments.Add( payment );
        //								responseMessage = "final partial payment received, invoice is now fully paid";
        //								break;
        //							default:
        //								throw new ArgumentOutOfRangeException( );
        //						}

        //					}
        //					else
        //					{
        //						switch ( inv.Type )
        //						{
        //							case InvoiceType.Standard:
        //								inv.AmountPaid += payment.Amount;
        //								inv.Payments.Add( payment );
        //								responseMessage = "another partial payment received, still not fully paid";
        //								break;
        //							case InvoiceType.Commercial:
        //								inv.AmountPaid += payment.Amount;
        //								inv.TaxAmount += payment.Amount * 0.14m;
        //								inv.Payments.Add( payment );
        //								responseMessage = "another partial payment received, still not fully paid";
        //								break;
        //							default:
        //								throw new ArgumentOutOfRangeException( );
        //						}
        //					}
        //				}
        //			}
        //			else
        //			{
        //				if ( payment.Amount > inv.Amount )
        //				{
        //					responseMessage = "the payment is greater than the invoice amount";
        //				}
        //				else if ( inv.Amount == payment.Amount )
        //				{
        //					switch ( inv.Type )
        //					{
        //						case InvoiceType.Standard:
        //							inv.AmountPaid = payment.Amount;
        //							inv.TaxAmount = payment.Amount * 0.14m;
        //							inv.Payments.Add( payment );
        //							responseMessage = "invoice is now fully paid";
        //							break;
        //						case InvoiceType.Commercial:
        //							inv.AmountPaid = payment.Amount;
        //							inv.TaxAmount = payment.Amount * 0.14m;
        //							inv.Payments.Add( payment );
        //							responseMessage = "invoice is now fully paid";
        //							break;
        //						default:
        //							throw new ArgumentOutOfRangeException( );
        //					}
        //				}
        //				else
        //				{
        //					switch ( inv.Type )
        //					{
        //						case InvoiceType.Standard:
        //							inv.AmountPaid = payment.Amount;
        //							inv.TaxAmount = payment.Amount * 0.14m;
        //							inv.Payments.Add( payment );
        //							responseMessage = "invoice is now partially paid";
        //							break;
        //						case InvoiceType.Commercial:
        //							inv.AmountPaid = payment.Amount;
        //							inv.TaxAmount = payment.Amount * 0.14m;
        //							inv.Payments.Add( payment );
        //							responseMessage = "invoice is now partially paid";
        //							break;
        //						default:
        //							throw new ArgumentOutOfRangeException( );
        //					}
        //				}
        //			}
        //		}
        //	}

        //	inv.Save();

        //	return responseMessage;
        //}
    }
}