using System;

namespace RefactorThis.Persistence
{
    public class Payment
    {
        public Payment()
        {
            Id = Guid.NewGuid();
            Date = DateTime.UtcNow;
        }

        public Payment(string reference, decimal amount) : this()
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                throw new ArgumentException("Payment reference cannot be null or empty", nameof(reference));
            }

            if (amount <= 0)
            {
                throw new ArgumentException("Payment amount must be greater than zero", nameof(amount));
            }

            Reference = reference;
            Amount = amount;
        }

        public Guid Id { get; set; }
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}