using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Persistence.Contracts
{
    /// <summary>
    /// Defines the contract for processing payments related to invoices.
    /// </summary>
    public interface IInvoiceService
    {
        /// <summary>
        /// Processes a payment for an invoice.
        /// </summary>
        /// <param name="payment">The payment details to be processed.</param>
        /// <returns>A PaymentResult indicating the result of the payment processing.</returns>
        PaymentResult ProcessPayment(Payment payment);
    }
}


