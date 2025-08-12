using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class InvoiceService
    {
        private readonly InvoiceRepository _invoiceRepository;
        private readonly ITaxCalculator _taxCalculator;
        private readonly IPaymentValidator _paymentValidator;

        public InvoiceService(InvoiceRepository invoiceRepository)
            : this(invoiceRepository, new TaxCalculator(), new PaymentValidator())
        {
        }

        public InvoiceService(InvoiceRepository invoiceRepository, ITaxCalculator taxCalculator, IPaymentValidator paymentValidator)
        {
            _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
            _taxCalculator = taxCalculator ?? throw new ArgumentNullException(nameof(taxCalculator));
            _paymentValidator = paymentValidator ?? throw new ArgumentNullException(nameof(paymentValidator));
        }

        public string ProcessPayment(Payment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            var invoice = GetInvoice(payment.Reference);

            var validationResult = _paymentValidator.Validate(invoice, payment);
            if (!validationResult.IsValid)
                return validationResult.ErrorMessage;

            var result = ApplyPayment(invoice, payment);

            invoice.Save();

            return result.Message;
        }

        private Invoice GetInvoice(string reference)
        {
            var invoice = _invoiceRepository.GetInvoice(reference);
            if (invoice == null)
                throw new InvalidOperationException("There is no invoice matching this payment");

            return invoice;
        }

        private PaymentResult ApplyPayment(Invoice invoice, Payment payment)
        {
            var currentAmountPaid = GetCurrentAmountPaid(invoice);
            var remainingAmount = invoice.Amount - currentAmountPaid;

            // Add payment to invoice
            invoice.EnsurePaymentsCollection();
            invoice.Payments.Add(payment);
            invoice.AmountPaid = currentAmountPaid + payment.Amount;

            // Calculate tax
            var taxAmount = _taxCalculator.Calculate(payment.Amount, invoice.Type);
            invoice.TaxAmount += taxAmount;

            return DeterminePaymentResult(invoice, payment, remainingAmount);
        }

        private decimal GetCurrentAmountPaid(Invoice invoice)
        {
            if (invoice.Payments == null)
                return 0;

            return invoice.Payments.Sum(p => p.Amount);
        }

        private PaymentResult DeterminePaymentResult(Invoice invoice, Payment payment, decimal remainingAmount)
        {
            if (payment.Amount == remainingAmount)
            {
                return HasExistingPayments(invoice)
                    ? new PaymentResult("final partial payment received, invoice is now fully paid")
                    : new PaymentResult("invoice is now fully paid");
            }

            return HasExistingPayments(invoice)
                ? new PaymentResult("another partial payment received, still not fully paid")
                : new PaymentResult("invoice is now partially paid");
        }

        private bool HasExistingPayments(Invoice invoice)
        {
            return invoice.Payments != null && invoice.Payments.Any();
        }
    }

    public class PaymentResult
    {
        public PaymentResult(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}