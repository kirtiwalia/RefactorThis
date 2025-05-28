namespace RefactorThis.Persistence.models
{
    /// <summary>
    /// Represents a payment made towards an invoice.
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// The amount of money paid in this payment transaction.
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// A reference or identifier for the payment (e.g., transaction ID, check number).
        /// </summary>
        public string Reference { get; set; }
    }
}