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

		   public string ProcessPayment(Payment payment)
		   {
			   var invoice = _invoiceRepository.GetInvoice(payment.Reference);
			   if (invoice == null)
				   throw new InvalidOperationException("There is no invoice matching this payment");

			   if (IsZeroAmountInvoice(invoice))
				   return HandleZeroAmountInvoice(invoice);

			   if (HasPayments(invoice))
				   return HandleInvoiceWithPayments(invoice, payment);

			   return HandleInvoiceWithoutPayments(invoice, payment);
		   }

		   private bool IsZeroAmountInvoice(Invoice invoice) => invoice.Amount == 0;

		   private bool HasPayments(Invoice invoice) => invoice.Payments != null && invoice.Payments.Any();

		   private string HandleZeroAmountInvoice(Invoice invoice)
		   {
			   if (!HasPayments(invoice))
				   return "no payment needed";
			   throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
		   }

		   private string HandleInvoiceWithPayments(Invoice invoice, Payment payment)
		   {
			   var totalPaid = invoice.Payments.Sum(x => x.Amount);
			   var remaining = invoice.Amount - invoice.AmountPaid;

			   if (totalPaid != 0 && invoice.Amount == totalPaid)
				   return "invoice was already fully paid";

			   if (totalPaid != 0 && payment.Amount > remaining)
				   return "the payment is greater than the partial amount remaining";

			   if (remaining == payment.Amount)
				   return ApplyFinalPartialPayment(invoice, payment);

			   return ApplyAnotherPartialPayment(invoice, payment);
		   }

		   private string HandleInvoiceWithoutPayments(Invoice invoice, Payment payment)
		   {
			   if (payment.Amount > invoice.Amount)
				   return "the payment is greater than the invoice amount";

			   if (invoice.Amount == payment.Amount)
				   return ApplyFullPayment(invoice, payment);

			   return ApplyPartialPayment(invoice, payment);
		   }

		   private string ApplyFullPayment(Invoice invoice, Payment payment)
		   {
			   ApplyPayment(invoice, payment, payment.Amount, isFull: true);
			   return "invoice is now fully paid";
		   }

		   private string ApplyPartialPayment(Invoice invoice, Payment payment)
		   {
			   ApplyPayment(invoice, payment, payment.Amount, isFull: false);
			   return "invoice is now partially paid";
		   }

		   private string ApplyFinalPartialPayment(Invoice invoice, Payment payment)
		   {
			   ApplyPayment(invoice, payment, payment.Amount, isFull: true);
			   return "final partial payment received, invoice is now fully paid";
		   }

		   private string ApplyAnotherPartialPayment(Invoice invoice, Payment payment)
		   {
			   ApplyPayment(invoice, payment, payment.Amount, isFull: false);
			   return "another partial payment received, still not fully paid";
		   }

		   private void ApplyPayment(Invoice invoice, Payment payment, decimal amount, bool isFull)
		   {
			   invoice.AmountPaid += amount;
			   if (invoice.Type == InvoiceType.Commercial || invoice.Type == InvoiceType.Standard)
				   invoice.TaxAmount += amount * 0.14m;
			   else
				   throw new ArgumentOutOfRangeException();

			   if (invoice.Payments == null)
				   invoice.Payments = new System.Collections.Generic.List<Payment>();
			   invoice.Payments.Add(payment);

			   invoice.Save();
		   }
	}
}