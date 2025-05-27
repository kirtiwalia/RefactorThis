using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
	public class InvoiceService
	{
		private readonly IInvoiceRepository _invoiceRepository;
        private readonly MemoryCache _invoiceCache = MemoryCache.Default;

        public InvoiceService(IInvoiceRepository invoiceRepository)
		{
			_invoiceRepository = invoiceRepository;
		}

        /// <summary>
        /// Processes a payment against an invoice.
        /// </summary>
		public string ProcessPayment(Payment payment)
		{
            // 2.Added NULL & Zero Invoice checks to avoid Exceptions & to achieve Test coverage.
            // 3.Reduce Nested IfElse to avoid confusion and improve readability
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            if (string.IsNullOrWhiteSpace(payment.Reference))
                throw new InvalidOperationException(ResponseMessages.NoInvoiceMatchingPayment);

            var invoice = GetInvoiceWithCache(payment.Reference);
            var responseMessage = string.Empty;

            if (invoice == null)
				throw new InvalidOperationException(ResponseMessages.NoInvoiceMatchingPayment);

            if (invoice.Amount <= 0)
            {
                if (invoice.Payments == null || !invoice.Payments.Any())
                    return ResponseMessages.NoPaymentNeeded;
                else
                    throw new InvalidOperationException(ResponseMessages.InvoiceInvalidZeroAmountWithPayments);
            }

            if (invoice.Payments == null)
                invoice.Payments = new List<Payment>();

            // 4.Repeated calculations are stored in seperate variables to reduce DB calls & improve Page Performance
            var isPaymentExists = invoice.Payments != null && invoice.Payments.Any();
            var totalPayments = isPaymentExists ? invoice.Payments.Sum(x => x.Amount) : Rates.ZeroTaxRate;
            var amountRemaining = invoice.Amount - invoice.AmountPaid;

            if (isPaymentExists)
            {
                if (totalPayments != 0 && invoice.Amount == totalPayments)
                    return ResponseMessages.InvoiceAlreadyFullyPaid;

                if (totalPayments != 0 && payment.Amount > amountRemaining)
                    return ResponseMessages.PaymentGreaterPartialAmountRemaining;

                bool isFinalPayment = amountRemaining == payment.Amount;
                return CalculateAndSavePayment(invoice, payment, isPartialPayment: true, isFinalPayment: isFinalPayment,
                    partialPaymentResponse: ResponseMessages.AnotherPartialPaymentReceivedInvoiceNotFullyPaid,
                    finalPaymentResponse: ResponseMessages.FinalPartialPaymentReceivedInvoiceFullyPaid
                );
            }

            if (payment.Amount > invoice.Amount)
                return ResponseMessages.PaymentGreaterInvoiceAmount;

            bool isFullPayment = invoice.Amount == payment.Amount;
            return CalculateAndSavePayment(invoice, payment, isPartialPayment: false, isFinalPayment: isFullPayment,
                partialPaymentResponse: ResponseMessages.InvoiceNowPartiallyPaid,
                finalPaymentResponse: ResponseMessages.FinalPartialPaymentReceivedInvoiceFullyPaid
            );
        }

        /// <summary>
        /// 5.Created private method to Calculate the payment and saves the invoice (Removed repetitive code & calculations)
        /// </summary>
        private string CalculateAndSavePayment(Invoice invoice,Payment payment,bool isPartialPayment,bool isFinalPayment,string partialPaymentResponse,string finalPaymentResponse)
        {
            if (invoice.Payments == null)
                invoice.Payments = new List<Payment>();

            if (isPartialPayment)
                invoice.AmountPaid += payment.Amount;
            else
                invoice.AmountPaid = payment.Amount;

            invoice.TaxAmount = payment.Amount * Rates.TaxRate;
            invoice.Payments.Add(payment);

            // 7.Used repository to save instead of direct DB save.
            _invoiceRepository.SaveInvoice(invoice);

            return isFinalPayment ? finalPaymentResponse : partialPaymentResponse;
        }

        /// <summary>
        /// 8.Added Cache to store the invoice & to avoid multiple DB calls for the same invoice reference. 
        /// If invoice is not found in the cache, then its retrieved from the repository (DB call) and caches it for 10 minutes.
        /// </summary>
        private Invoice GetInvoiceWithCache(string reference)
        {
            var invoice = _invoiceCache.Get(reference) as Invoice;

            if (invoice == null)
            {
                invoice = _invoiceRepository.GetInvoice(reference);

                if (invoice != null)
                    _invoiceCache.Set(reference, invoice, DateTimeOffset.Now.AddMinutes(10));
            }

            return invoice;
        }
    }
}