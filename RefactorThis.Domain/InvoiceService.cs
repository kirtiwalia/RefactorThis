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
			var invEntity = _invoiceRepository.GetInvoice(payment.Reference);
			var invoice = InvoiceDomain.New(invEntity, payment.Amount);

			if (invoice.IsNoPaymentRequired(out var noPaymentMessage))
				return SaveInvoiceEntity(invEntity, noPaymentMessage);

			if (invoice.IsNoOutStandingAmount(out var fullyPaidMessage))
				return SaveInvoiceEntity(invEntity, fullyPaidMessage);

			if (invoice.IsOverPaid(out var overPaidMessage))
				return SaveInvoiceEntity(invEntity, overPaidMessage);

			var responseMessage = invoice.Paid();
			invEntity.AmountPaid = invoice.AmountPaid;
			invEntity.TaxAmount = invoice.TaxAmount;
			invEntity.Payments.Add(payment);

			return SaveInvoiceEntity(invEntity, responseMessage);
		}

		private static string SaveInvoiceEntity(Invoice invEntity, string responseMessage)
		{
			invEntity.Save();
			
			return responseMessage;
		}
	}

	internal class InvoiceDomain
	{
		private InvoiceType Type { get; }
		private decimal Amount { get; }
		private decimal PaymentAmount { get; }
		private decimal OutstandingAmount { get; }
		private bool IsPartiallyPaid { get; }
		private decimal PaidAmount { get; }
		internal decimal TaxAmount { get; private set; }
		internal decimal AmountPaid { get; private set; }
		
		private InvoiceDomain(Invoice invoice, decimal paid)
		{
			Type = invoice.Type;
			Amount = invoice.Amount;
			TaxAmount = invoice.TaxAmount;
			AmountPaid = invoice.AmountPaid;
			PaymentAmount = invoice.Payments?.Sum( x => x.Amount ) ?? 0;
			OutstandingAmount = invoice.Amount - PaymentAmount;
			IsPartiallyPaid = !(invoice.Payments is null) && invoice.Payments.Any();
			PaidAmount = paid;
		}

		internal static InvoiceDomain New(Invoice invoice, decimal paid)
		{
			if (invoice is null)
				throw new InvalidOperationException("There is no invoice matching this payment");

			if (invoice.Amount == 0 && !(invoice.Payments is null) && invoice.Payments.Any())
				throw new InvalidOperationException(
					"The invoice is in an invalid state, it has an amount of 0 and it has payments.");

			if (invoice.Type != InvoiceType.Commercial && invoice.Type != InvoiceType.Standard)
				throw new ArgumentOutOfRangeException();
			
			return new InvoiceDomain(invoice, paid);
		}

		internal bool IsNoPaymentRequired(out string responseMessage)
		{
			responseMessage = "no payment needed";
			
			return Amount == 0 && PaymentAmount == 0;
		}

		internal bool IsNoOutStandingAmount(out string responseMessage)
		{
			responseMessage = "invoice was already fully paid";
			
			return OutstandingAmount == 0;
		}

		internal bool IsOverPaid(out string responseMessage)
		{
			responseMessage = IsPartiallyPaid
				? "the payment is greater than the partial amount remaining"
				: "the payment is greater than the invoice amount";
			
			return PaidAmount > OutstandingAmount || PaidAmount > Amount;
		}

		internal string Paid()
		{
			var responseMessage = GetResponseMessage();
			if (Type == InvoiceType.Standard)
			{
				var standardInvoice = StandardInvoice.Paid(IsPartiallyPaid, AmountPaid, PaidAmount);
				AmountPaid = standardInvoice.AmountPaid;
				if(standardInvoice.TaxAmount.HasValue)
					TaxAmount = standardInvoice.TaxAmount.Value;

				return responseMessage;
			}

			var commercialInvoice = CommercialInvoice.Paid(IsPartiallyPaid, AmountPaid, TaxAmount, PaidAmount);
			AmountPaid = commercialInvoice.AmountPaid;
			TaxAmount = commercialInvoice.TaxAmount;
			
			return responseMessage;
		}

		private string GetResponseMessage()
		{
			var isFullyPaid = OutstandingAmount == PaidAmount;

			return IsPartiallyPaid
				? $"{(isFullyPaid ? "final" : "another")} partial payment received, {(isFullyPaid ? "invoice is now fully paid" : "still not fully paid")}"
				: $"invoice is now {(isFullyPaid ? "fully" : "partially")} paid";
		}
	}

	internal static class StandardInvoice
	{
		internal static (decimal AmountPaid, decimal? TaxAmount) 
			Paid(bool isPartiallyPaid, decimal amountPaid, decimal paid)
		{
			amountPaid = isPartiallyPaid ? amountPaid + paid : paid;
			var taxAmount = isPartiallyPaid ? (decimal?)null : paid * 0.14m;
			return (amountPaid, taxAmount);
		}
	}

	internal static class CommercialInvoice
	{
		internal static (decimal AmountPaid, decimal TaxAmount) 
			Paid(bool isPartiallyPaid, decimal amountPaid, decimal taxAmount, decimal paid)
		{
			var tax = paid * 0.14m;
			amountPaid = isPartiallyPaid ? amountPaid + paid : paid;
			taxAmount = isPartiallyPaid ? taxAmount + tax : tax;
			return (amountPaid, taxAmount);
		}
	}
}