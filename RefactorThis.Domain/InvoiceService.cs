using System;
using System.Linq;
using RefactorThis.Domain.common;
using RefactorThis.Domain.common.enums;
using RefactorThis.Persistence.enums;
using RefactorThis.Persistence.models;
using RefactorThis.Persistence.repositories;

namespace RefactorThis.Domain
{
    public class InvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private const decimal TaxRate = 0.14m;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }
        
        /// <summary>
        /// Processes a payment by applying it to the matching invoice if valid.
        /// </summary>
        /// <param name="payment">The payment information to be applied to an invoice.</param>
        /// <returns>
        /// A <see cref="ProcessPaymentStatus"/> indicating the result of the operation,
        /// such as full payment, partial payment, overpayment, or invoice not found.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no invoice matches the payment reference or the invoice is in an invalid state.
        /// </exception>
        public ProcessPaymentStatus ProcessPayment(Payment payment)
        {
            var invoice = _invoiceRepository.GetByReference(payment.Reference) ??
                          throw new InvalidOperationException(ProcessPaymentExceptionMessage.NoInvoiceMatchingPayment);
            
            if (invoice.Amount == 0)
            {
                if (invoice.Payments == null || !invoice.Payments.Any())
                {
                    return ProcessPaymentStatus.NoPaymentNeeded;
                }

                throw new InvalidOperationException(ProcessPaymentExceptionMessage.InvalidInvoiceState);
            }

            var totalPaid = invoice.Payments?.Sum(x => x.Amount) ?? 0;
            var remaining = invoice.Amount - invoice.AmountPaid;

            if (totalPaid >= invoice.Amount)
            {
                return ProcessPaymentStatus.InvoiceAlreadyFullyPaid;
            }

            if (totalPaid > 0 && payment.Amount > remaining)
            {
                return ProcessPaymentStatus.PartialPaymentExistsAndAmountPaidExceedsAmountDue;
            }

            if (totalPaid == 0 && payment.Amount > invoice.Amount)
            {
                return ProcessPaymentStatus.NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount;
            }

            var isFullPayment = payment.Amount == remaining;

            ApplyPayment(invoice, payment, isFullPayment);

            return totalPaid == 0
                ? isFullPayment
                    ? ProcessPaymentStatus.InvoiceAlreadyFullyPaid
                    : ProcessPaymentStatus.PartialPaymentExistsAndAmountPaidIsLessThanAmountDue
                : isFullPayment
                    ? ProcessPaymentStatus.PartialPaymentExistsAndAmountPaidEqualsAmountDue
                    : ProcessPaymentStatus.PartialPaymentExistsAndAmountPaidIsLessThanAmountDue;
        }

        /// <summary>
        /// Applies a payment to the given invoice and updates tax if applicable.
        /// </summary>
        /// <param name="invoice">The invoice to apply the payment to.</param>
        /// <param name="payment">The payment details.</param>
        /// <param name="applyTax">Indicates whether tax should be applied for this payment.</param>
        private void ApplyPayment(Invoice invoice, Payment payment, bool applyTax)
        {
            invoice.AmountPaid += payment.Amount;
            invoice.Payments.Add(payment);

            if (invoice.Type == InvoiceType.Commercial || applyTax)
            {
                invoice.TaxAmount += payment.Amount * TaxRate;
            }

            _invoiceRepository.Update(invoice);
        }
    }
}