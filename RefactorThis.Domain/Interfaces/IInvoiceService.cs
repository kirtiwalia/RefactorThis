using RefactorThis.Domain.Models;

namespace RefactorThis.Domain.Services.Interfaces
{
    /// <summary>
    /// Defines an interface for invoice related operations
    /// </summary>
    public interface IInvoiceService
    {
        /// <summary>
        /// Process a payment against an invoice
        /// </summary>
        /// <param name="payment">The payment to process</param>
        /// <returns>A string indicating the result of the processed payment</returns>
        string ProcessPayment(Payment payment);
    }
}
