using System;
using System.Collections.Generic;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
	public class InvoiceService
	{
		private readonly IInvoiceRepository _invoiceRepository;

		public InvoiceService( IInvoiceRepository invoiceRepository )
		{
			_invoiceRepository = invoiceRepository;
		}

		public string ProcessPayment(Payment payment)
        {
            var invoice = _invoiceRepository.GetInvoice(payment.Reference);

            if (invoice == null)
                throw new InvalidOperationException("There is no invoice matching this payment");

            switch (GetPaymentStatus(invoice, payment))
            {
                case PaymentStatus.NoPaymentNeeded:
                    return "no payment needed";
                case PaymentStatus.AlreadyPaid:
                    return "invoice was already fully paid";
                case PaymentStatus.Overpayment:
					return invoice.AmountPaid == 0
						? "the payment is greater than the invoice amount"
						: "the payment is greater than the partial amount remaining";
				default:
                    return ApplyPayment(invoice, payment);
            }
        }

        private static PaymentStatus GetPaymentStatus(Invoice invoice, Payment payment)
        {
            if (IsNoPaymentNeeded(invoice))
                return PaymentStatus.NoPaymentNeeded;

            if (invoice.IsFullyPaid)
                return PaymentStatus.AlreadyPaid;

            if (IsOverpayment(invoice, payment))
                return PaymentStatus.Overpayment;

            return PaymentStatus.Valid;
        }

        private static bool IsNoPaymentNeeded(Invoice invoice)
        {
            return invoice.Amount == 0 && (invoice.Payments == null || !invoice.Payments.Any());
        }

        private static bool IsOverpayment(Invoice invoice, Payment payment)
        {
	        return payment.Amount > (invoice.Amount - invoice.AmountPaid);
		}

        private static decimal CalculateTaxAmount(decimal amount, InvoiceType type)
        {
            return type == InvoiceType.Commercial ? amount * 0.14m : 0;
        }

		private string ApplyPayment(Invoice invoice, Payment payment)
		{
			if (invoice.Payments == null)
			{
				invoice.Payments = new List<Payment>();
			}

			invoice.AmountPaid += payment.Amount;
			invoice.Payments.Add(payment);
			invoice.TaxAmount += CalculateTaxAmount(payment.Amount, invoice.Type);

			var responseMessage = invoice.AmountPaid == invoice.Amount
				? (invoice.AmountPaid == payment.Amount ? "invoice was already fully paid" : "final partial payment received, invoice is now fully paid")
				: (invoice.AmountPaid == payment.Amount ? "invoice is now partially paid" : "another partial payment received, still not fully paid");

			try
			{
				_invoiceRepository.SaveInvoice(invoice);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("Error saving invoice", ex);
			}

			return responseMessage;
		}

		private enum PaymentStatus
        {
            NoPaymentNeeded,
            AlreadyPaid,
            Overpayment,
            Valid
        }
	}
}