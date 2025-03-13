using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Persistence.Contracts
{
    /// <summary>
    /// Defines the contract for handling invoice-related data operations.
    /// </summary>
    public interface IInvoiceRepository
    {
        /// <summary>
        /// Retrieves an invoice based on its reference number.
        /// </summary>
        /// <param name="reference">The unique reference number of the invoice.</param>
        /// <returns>The <see cref="Invoice"/> object if found; otherwise, null.</returns>
        Invoice GetInvoice(string reference);

        /// <summary>
        /// Saves an existing invoice.
        /// </summary>
        /// <param name="invoice">The invoice to be saved.</param>
        void SaveInvoice(Invoice invoice);

        /// <summary>
        /// Adds a new invoice to the system.
        /// </summary>
        /// <param name="invoice">The invoice to be added.</param>
        void Add(Invoice invoice);
    }
}

