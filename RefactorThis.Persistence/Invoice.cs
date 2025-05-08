using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence
{
    public class Invoice
    {
        private readonly List<Payment> _payments = new List<Payment>();
        private readonly IInvoiceRepository _repository;
        private const decimal COMMERCIAL_TAX_RATE = 0.14m;

        // Constructor for new invoices
        public Invoice(IInvoiceRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        // Constructor for loading from repository or testing
        public Invoice(IInvoiceRepository repository, string reference, decimal amount, InvoiceType type)
            : this(repository)
        {
            Reference = reference ?? throw new ArgumentNullException(nameof(reference));
            Amount = amount;
            Type = type;
        }

        // Constructor for deserialization and testing
        protected Invoice() { }

        // Required properties
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; private set; }
        public decimal TaxAmount { get; private set; }
        public InvoiceType Type { get; set; }

        // Read-only collections
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

        // Calculated properties
        public decimal RemainingAmount => Amount - AmountPaid;
        public bool IsFullyPaid => AmountPaid >= Amount && Amount > 0;
        public bool RequiresPayment => Amount > 0;

        // Domain methods for payment processing
        public PaymentResult ProcessPayment(Payment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }

            // Validation logic
            if (!RequiresPayment)
            {
                return PaymentResult.NoPaymentNeeded;
            }

            if (IsFullyPaid)
            {
                return PaymentResult.AlreadyPaid;
            }

            if (payment.Amount > RemainingAmount)
            {
                return HasPartialPayments()
                    ? PaymentResult.ExceedsPartialRemaining
                    : PaymentResult.ExceedsInvoiceAmount;
            }

            // Process the payment
            bool isFullPayment = payment.Amount == RemainingAmount;
            bool hasExistingPayments = HasPartialPayments();

            ApplyPayment(payment);

            // Determine result type
            if (isFullPayment)
            {
                return hasExistingPayments
                    ? PaymentResult.FinalPartialPayment
                    : PaymentResult.FullPayment;
            }
            else
            {
                return hasExistingPayments
                    ? PaymentResult.AdditionalPartialPayment
                    : PaymentResult.FirstPartialPayment;
            }
        }

        // Helper methods for payment processing
        private bool HasPartialPayments()
        {
            return _payments != null && _payments.Any();
        }

        private void ApplyPayment(Payment payment)
        {
            AmountPaid += payment.Amount;

            if (Type == InvoiceType.Commercial)
            {
                TaxAmount += payment.Amount * COMMERCIAL_TAX_RATE;
            }

            _payments.Add(payment);
        }

        // Data access methods
        public void Save()
        {
            if (_repository == null)
            {
                throw new InvalidOperationException("Cannot save invoice: no repository configured");
            }

            if (string.IsNullOrWhiteSpace(Reference))
            {
                throw new InvalidOperationException("Cannot save invoice: Reference is required");
            }

            _repository.SaveInvoice(this);
        }

        // Required for deserialization and testing
        public void SetPayments(IEnumerable<Payment> payments)
        {
            _payments.Clear();
            if (payments != null)
            {
                _payments.AddRange(payments);
            }
        }

        // Required for deserialization and testing
        public void SetAmountPaid(decimal amountPaid)
        {
            if (amountPaid < 0)
            {
                throw new ArgumentException("Amount paid cannot be negative", nameof(amountPaid));
            }

            AmountPaid = amountPaid;
        }

        // Required for deserialization and testing
        public void SetTaxAmount(decimal taxAmount)
        {
            if (taxAmount < 0)
            {
                throw new ArgumentException("Tax amount cannot be negative", nameof(taxAmount));
            }

            TaxAmount = taxAmount;
        }
    }

    public enum InvoiceType
    {
        Standard,
        Commercial
    }

    public enum PaymentResult
    {
        NoPaymentNeeded,
        AlreadyPaid,
        ExceedsInvoiceAmount,
        ExceedsPartialRemaining,
        FullPayment,
        FirstPartialPayment,
        AdditionalPartialPayment,
        FinalPartialPayment
    }
}