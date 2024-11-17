using System;
using System.Linq;
using RefactorThis.Persistence;
using RefactorThis.Persistence.Entity;
using RefactorThis.Persistence.Enum;
using RefactorThis.Persistence.Interface;

namespace RefactorThis.Domain
{
	public class InvoiceService
	{
		private readonly IInvoiceRepository _invoiceRepository;

		public InvoiceService(IInvoiceRepository invoiceRepository)
		{
			_invoiceRepository = invoiceRepository;
		}

        /// <summary>
        /// Processes a payment for the specified invoice.
        /// </summary>
        /// <param name="payment"></param> The payment to be processed, containing the payment amount and reference to the invoice.
        /// <returns>
		/// A message indicating the result of the payment process
		/// </returns>
        /// <exception cref="InvalidOperationException">
		/// Thrown if no invoice is found matching the payment reference, or if the invoice is in an invalid state.
		/// </exception>
        public string ProcessPayment(Payment payment)
		{
			// Retrieve invoice record
			var invoice = _invoiceRepository.GetInvoice(payment.Reference);

			// Validate invoice
			var message = ValidateInvoiceState(invoice, payment);
			if (!String.Empty.Equals(message))
			{
				// If message is not empty, there's an error.
				// Validation has failed
				return message;
			}

            switch (invoice.Type)
            {
                case InvoiceType.Standard:
                    message = ProcessStandardPayment(invoice, payment);
                    break;
                case InvoiceType.Commercial:
                    message = ProcessCommercialPayment(invoice, payment);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(invoice.Type));
            }

			invoice.Save();

			return message;
		}

        /// <summary>
        /// Validates the invoice before processing the payment
        /// </summary>
        /// <param name="invoice">The invoice to be validated.</param>
        /// <param name="payment">The payment being processed.</param>
        /// <returns>
		/// A message indicating the result of the validation process</returns>
        /// <exception cref="InvalidOperationException">
		/// Thrown if the invoice is in an invalid state. eg: there are payments but there's no amount
		/// </exception>
        private string ValidateInvoiceState(Invoice invoice, Payment payment)
		{
			if (invoice == null)
			{
				throw new InvalidOperationException("There is no invoice matching this payment");
			}
			else if (invoice.Amount == 0)
			{
				if (invoice.Payments == null || !invoice.Payments.Any())
				{
					return "no payment needed";
				}
				else
				{
					throw new InvalidOperationException("The invoice is in an invalid state.");
				}
			}
			else if (invoice.Payments != null && invoice.Payments.Any())
			{
				var totalPayments = invoice.Payments.Sum(x => x.Amount);

				if (totalPayments != 0 && totalPayments == invoice.Amount)
				{
					return "invoice was already fully paid";
				}
				else if (totalPayments != 0 && payment.Amount > (invoice.Amount - invoice.AmountPaid))
				{
					return "the payment is greater than the partial amount remaining";
				}
			}
			else if (payment.Amount > invoice.Amount)
			{
				return "the payment is greater than the invoice amount";
			}
			return string.Empty;
		}
		/// <summary>
		/// Process payment for Standard InvoiceType
		/// </summary>
		/// <param name="invoice"></param>
		/// <param name="payment"></param>
		/// <returns></returns>
        private string ProcessStandardPayment(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid += payment.Amount;
            invoice.Payments.Add(payment);
            return DeterminePaymentStatus(invoice);
        }

		/// <summary>
		/// Process payment for Commercial InvoiceType
		/// </summary>
		/// <param name="invoice"></param>
		/// <param name="payment"></param>
		/// <returns></returns>
        private string ProcessCommercialPayment(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid += payment.Amount;
            invoice.TaxAmount += payment.Amount * 0.14m;
            invoice.Payments.Add(payment);
            return DeterminePaymentStatus(invoice);
        }


        private string DeterminePaymentStatus(Invoice invoice)
		{
			if (invoice.Amount == invoice.AmountPaid)
			{
				return invoice.Payments.Count > 1 ? "final partial payment received, invoice is now fully paid" : "invoice is now fully paid";
			}
			else
			{
				return invoice.Payments.Count > 1 ? "another partial payment received, still not fully paid" : "invoice is now partially paid";
			}
		}
	}
}