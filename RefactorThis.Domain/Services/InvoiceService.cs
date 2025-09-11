using System;
using System.Linq;
using RefactorThis.Domain.Enums;
using RefactorThis.Domain.Interfaces;
using RefactorThis.Domain.Models;
using RefactorThis.Domain.Services.Interfaces;

namespace RefactorThis.Domain.Services
{
    /// <summary>
    /// Provides business logic for processing payments against invoices
    /// </summary>
	public class InvoiceService : IInvoiceService
	{
		private readonly IInvoiceRepository _invoiceRepository;

		public InvoiceService(IInvoiceRepository invoiceRepository )
		{
			_invoiceRepository = invoiceRepository;
		}

        /// <inheritdoc />
        public string ProcessPayment(Payment payment)
		{
			var invoice = _invoiceRepository.GetInvoice(payment.Reference) ?? throw new InvalidOperationException("There is no invoice matching this payment");

            if (invoice.Amount == 0)
				return ZeroAmountInvoice(invoice);

			bool existingPayments = invoice.Payments != null && invoice.Payments.Any();
            decimal totalPaid = existingPayments ? invoice.Payments.Sum(x => x.Amount) : 0;
            decimal remainingAmount = invoice.Amount - invoice.AmountPaid;

			if (totalPaid != 0 && totalPaid == invoice.Amount)
                return "invoice was already fully paid";

            if (totalPaid != 0 && payment.Amount > remainingAmount)
                return "the payment is greater than the partial amount remaining";

            if (payment.Amount > remainingAmount)
                return "the payment is greater than the invoice amount";

            invoice.AmountPaid += payment.Amount;

            if (invoice.Type == InvoiceType.Commercial)
                invoice.TaxAmount += payment.Amount * 0.14m;

            invoice.Payments.Add(payment);

            _invoiceRepository.SaveInvoice(invoice);

			return GetPaymentMessage(invoice, existingPayments);
		}

        /// <summary>
        /// Handles the case where the inoice has a total amount of 0
        /// </summary>
        /// <param name="invoice">The invoice to check</param>
        /// <returns>A message saying no payment is needed</returns>
        /// <exception cref="InvalidOperationException">Thrown if invoice has payments but an amount of 0</exception>
        private string ZeroAmountInvoice(Invoice invoice)
        {
            if (invoice.Payments == null || !invoice.Payments.Any())
            {
                return "no payment needed";
            }
            throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }

        /// <summary>
        /// Gets the message to return after a payment is processed
        /// </summary>
        /// <param name="invoice">The invoice being updated</param>
        /// <param name="existingPayments">Whether the invoice has previous payments</param>
        /// <returns>A message describing the result of the payment</returns>
		private string GetPaymentMessage(Invoice invoice, bool existingPayments)
		{
            decimal newRemaining = invoice.Amount - invoice.AmountPaid;

            if (newRemaining == 0)
                return existingPayments ? "final partial payment received, invoice is now fully paid" : "invoice is now fully paid";

            return existingPayments ? "another partial payment received, still not fully paid" : "invoice is now partially paid";
        }
    }
}