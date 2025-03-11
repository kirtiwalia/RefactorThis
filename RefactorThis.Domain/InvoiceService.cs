using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
	public class InvoiceService
	{
		private readonly IInvoiceRepository _invoiceRepository;

		public InvoiceService(IInvoiceRepository invoiceRepository)
    	{
        	_invoiceRepository = invoiceRepository;
    	}

		public string ProcessPayment(Payment payment)
		{
			var invoice = _invoiceRepository.GetInvoice(payment.Reference) ?? throw new InvalidOperationException("There is no invoice matching this payment");
			
			if (invoice.Amount == 0)
			{
				return ValidateNoInvoiceAmount(invoice);
			}

			if (invoice.Payments != null && invoice.Payments.Count > 0)
			{
				return HandleWithPayment(invoice, payment);
			}
			else
			{
				return HandleWithoutPayment(invoice, payment);
			}
		}

		private string ValidateNoInvoiceAmount(Invoice invoice)
		{
			if (invoice.Payments == null || invoice.Payments.Count == 0)
			{
				return SaveAndOutputMessage(invoice, PaymentMessages.NoPaymentNeeded);
			}
			else
			{
				throw new InvalidOperationException(PaymentMessages.InvalidZeroAmountState);
			}
		}

		public string HandleWithPayment(Invoice invoice, Payment payment)
    	{	
			decimal totalPaidAmount = invoice.Payments?.Sum(p => p.Amount) ?? 0;
			decimal remainingAmount = invoice.Amount - totalPaidAmount;

			if (totalPaidAmount == invoice.Amount)
				return SaveAndOutputMessage(invoice, PaymentMessages.InvoiceAlreadyFullyPaid);

			if (payment.Amount > remainingAmount)
				return SaveAndOutputMessage(invoice, PaymentMessages.PaymentGreaterThanRemaining);

			if ((invoice.Amount - invoice.AmountPaid) == payment.Amount)
				return SaveAndOutputMessage(invoice, PaymentMessages.FinalPartialPaymentReceived);

			if (payment.Amount > invoice.Amount)
				return SaveAndOutputMessage(invoice, PaymentMessages.PaymentGreaterThanInvoice);

			invoice.AmountPaid += payment.Amount;
			invoice.Payments ??= new List<Payment>();

			if (invoice.Type == InvoiceType.Commercial)
				invoice.TaxAmount += payment.Amount * 0.14m;

			invoice.Payments.Add(payment);
			return SaveAndOutputMessage(invoice, (invoice.Amount - invoice.AmountPaid) == payment.Amount ? PaymentMessages.FinalPartialPaymentReceived : InvoiceService.PaymentMessages.AnotherPartialPaymentReceived);
		}

		public string HandleWithoutPayment(Invoice invoice, Payment payment)
    	{
			if (payment.Amount > invoice.Amount)
				return SaveAndOutputMessage(invoice, PaymentMessages.PaymentGreaterThanInvoice);

			invoice.AmountPaid = payment.Amount;
			invoice.Payments ??= new List<Payment>();

			invoice.TaxAmount = payment.Amount * 0.14m;
			invoice.Payments.Add(payment);
			return SaveAndOutputMessage(invoice, invoice.Amount == payment.Amount ? PaymentMessages.InvoiceNowFullyPaid : PaymentMessages.InvoicePartiallyPaid);
    	}

		private string SaveAndOutputMessage(Invoice invoice, string message)
		{
			_invoiceRepository.SaveInvoice(invoice);
			return message;
		}

		public static class PaymentMessages
    	{
			public const string NoPaymentNeeded = "no payment needed";
			public const string InvalidZeroAmountState = "The invoice is in an invalid state, it has an amount of 0 and it has payments.";
			public const string InvoiceAlreadyFullyPaid = "invoice was already fully paid";
			public const string PaymentGreaterThanRemaining = "the payment is greater than the partial amount remaining";
			public const string PaymentGreaterThanInvoice = "the payment is greater than the invoice amount";
			public const string FinalPartialPaymentReceived = "final partial payment received, invoice is now fully paid";
			public const string InvoicePartiallyPaid = "invoice is now partially paid";
			public const string InvoiceNowFullyPaid = "invoice is now fully paid";
			public const string AnotherPartialPaymentReceived = "another partial payment received, still not fully paid";
    	}
	}
}