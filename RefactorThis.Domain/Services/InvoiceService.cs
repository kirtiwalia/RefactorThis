using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Services
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

            ValidateInvoiceExists(invoice);

            if (IsInvoiceZeroAmount(invoice))
                return "no payment needed";

            if (HasPreviousPayments(invoice))
            {
                if (IsInvoiceFullyPaid(invoice))
                    return "invoice was already fully paid";

                if (IsOverPayment(invoice, payment))
                    return "the payment is greater than the partial amount remaining";

                ApplyPartialPayment(invoice, payment);

                if (invoice.AmountPaid == invoice.Amount)
                    return "final partial payment received, invoice is now fully paid";
                else
                    return "another partial payment received, still not fully paid";
            }
            else
            {
                if (payment.Amount > invoice.Amount)
                    return "the payment is greater than the invoice amount";

                ApplyFirstPayment(invoice, payment);

                if (invoice.Amount == invoice.AmountPaid)
                    return "invoice is now fully paid";
                else
                    return "invoice is now partially paid";
            }

		}


        #region Private Helper Methods
        private void ValidateInvoiceExists(Invoice invoice)
        {
            if (invoice == null)
                throw new InvalidOperationException("There is no invoice matching this payment");
        }

        private bool IsInvoiceZeroAmount(Invoice invoice)
        {
            if (invoice.Amount == 0)
            {
                if (invoice.Payments == null || !invoice.Payments.Any())
                    return true;
                else
                    throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
            }
            return false;
        }

        private bool HasPreviousPayments(Invoice invoice)
        {
            return invoice.Payments != null && invoice.Payments.Any();
        }

        private bool IsInvoiceFullyPaid(Invoice invoice)
        {
            return invoice.Payments.Sum(x => x.Amount) == invoice.Amount;
        }

        private bool IsOverPayment(Invoice invoice, Payment payment)
        {
            return payment.Amount > (invoice.Amount - invoice.AmountPaid);
        }

        private void ApplyPartialPayment(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid += payment.Amount;
            ApplyTaxIfCommercial(invoice, payment.Amount);
            invoice.Payments.Add(payment);
            _invoiceRepository.SaveInvoice(invoice);
        }

        private void ApplyFirstPayment(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid = payment.Amount;
            ApplyTaxIfCommercial(invoice, payment.Amount);
            invoice.Payments.Add(payment);
            _invoiceRepository.SaveInvoice(invoice);
        }

        private void ApplyTaxIfCommercial(Invoice invoice, decimal paymentAmount)
        {
            if (invoice.Type == InvoiceType.Commercial)
                invoice.TaxAmount += paymentAmount * 0.14m;
        }

        #endregion
    }
}