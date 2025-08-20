using RefactorThis.Application.Common;
using RefactorThis.Application.Interfaces;
using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Enums;
using RefactorThis.Domain.PaymentStrategies;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RefactorThis.Domain
{
	public class InvoiceService
	{
		private readonly IInvoiceRepository _invoiceRepository;
        private readonly IInvoicePaymentStrategyFactory _strategyFactory;

        public InvoiceService(IInvoiceRepository invoiceRepository, IInvoicePaymentStrategyFactory strategyFactory)
		{
			_invoiceRepository = invoiceRepository;
			_strategyFactory = strategyFactory;

        }

		public string ProcessPayment( Payment payment )
		{
            var invoice = _invoiceRepository.GetInvoice(payment.Reference);

            if (invoice == null)
                throw new InvalidOperationException("There is no invoice matching this payment");

            var totalPaid = invoice.Payments?.Sum(x => x.Amount) ?? 0;
            var remainingAmount = invoice.Amount - totalPaid;

            PaymentResultCode resultCode;

            if (invoice.Amount == 0)
            {
                if (totalPaid == 0)
                    resultCode = PaymentResultCode.NoPaymentNeeded;
                else
                    throw new InvalidOperationException("Invalid invoice: amount is 0 but it already has payments.");
            }
            else if (totalPaid == invoice.Amount)
            {
                resultCode = PaymentResultCode.InvoiceAlreadyPaid;
            }
            else if (payment.Amount > invoice.Amount)
            {
                resultCode = PaymentResultCode.PaymentGreaterThanInvoiceAmount;
            }
            else if (totalPaid != 0 && payment.Amount > remainingAmount)
            {
                resultCode = PaymentResultCode.PaymentGreaterThanRemaining;
            }
            else
            {
                var strategy = new InvoicePaymentStrategyFactory()
                                    .GetStrategy(invoice.Type);

                resultCode = strategy.ApplyPayment(invoice, payment);
            }

            return PaymentResultMessages.ToMessage(resultCode, invoice);
        }
    }
}