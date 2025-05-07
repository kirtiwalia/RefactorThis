using System;
using System.Linq;
using RefactorThis.Domain.enums;
using RefactorThis.Domain.messaging;
using RefactorThis.Persistence.models;
using RefactorThis.Persistence.repositories;

namespace RefactorThis.Domain
{
	public class InvoiceService
	{
		private readonly IInvoiceRepository _invoiceRepository;

		public InvoiceService( IInvoiceRepository invoiceRepository )
		{
			_invoiceRepository = invoiceRepository;
		}

		// Just having the Payment here doesn't make sense to me. There seems to be an assumption that the payment would already
		// be associated with the Invoice, in which case, adding it again makes no sense. It makes more sense to provide the invoiceId
		// which this payment must be associated with. This is something I would normally check with product, but here, I am assuming it's
		// a bad code design decision, and fixing it accordingly.
		// The general assumption of what this method must do is it must add a payment to an invoice, ensuring that the payment is valid
		// It then returns the invoice status - is it partially paid, or fully paid.
		public InvoicePaymentStatus ProcessPayment(Payment payment, Guid invoiceId)
		{
			var invoice = _invoiceRepository.GetInvoice(invoiceId);

			ValidateInvoicePayment(payment, invoice);

            AddPaymentToInvoice(payment, invoice);

			return (invoice.Amount == invoice.AmountPaid) ? InvoicePaymentStatus.Paid : InvoicePaymentStatus.PartiallyPaid;
		}

        // In the validation, rather than returning status messages for invalid Payment processing options, throw an exception instead.
        // In general, don't rely on statuses to check whether an operation succeeded or not.
        private void ValidateInvoicePayment( Payment payment, Invoice invoice )
        {
            if (invoice.Amount == 0)
            {
                if (invoice.HasPayments)
                {
                    throw new InvalidOperationException(ProcessPaymentErrorMessages.InvalidInvoice_NoAmountButHasPayments);
                }

                throw new InvalidOperationException(ProcessPaymentErrorMessages.NoPaymentNecessary);
            }

            if (invoice.AmountPaid == invoice.Amount)
			{
                throw new InvalidOperationException(ProcessPaymentErrorMessages.InvoiceAlreadyPaid);
            }

            if (invoice.AmountPaid + payment.Amount > invoice.Amount)
            {
                throw new InvalidOperationException(ProcessPaymentErrorMessages.PaymentTooGreat);
            }

            if (_invoiceRepository.FindInvoiceWithPayment(payment.Reference) != null)
            {
                throw new InvalidOperationException(ProcessPaymentErrorMessages.InvoiceWithPaymentAlreadyExists);
            }
        }

		private void AddPaymentToInvoice(Payment payment, Invoice invoice)
		{
            invoice.Payments.Add(payment);
            _invoiceRepository.SaveInvoice(invoice);
        }
    }
}