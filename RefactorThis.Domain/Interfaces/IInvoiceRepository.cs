using RefactorThis.Domain.Models;

namespace RefactorThis.Domain.Interfaces
{
    /// <summary>
    /// Defines an interface for a repository that manages invoices
    /// </summary>
    public interface IInvoiceRepository
    {
        /// <summary>
        /// Gets an invoice by its reference
        /// </summary>
        /// <param name="reference">The reference of the invoice</param>
        /// <returns>The invoice matching the reference</returns>
        Invoice GetInvoice(string reference);
        /// <summary>
        /// Saves an invoice
        /// </summary>
        /// <param name="invoice">The invoice to save</param>
        void SaveInvoice(Invoice invoice);
        /// <summary>
        /// Adds a new invoice to the repository
        /// </summary>
        /// <param name="invoice">The invoice to add</param>
        void Add(Invoice invoice);
    }
}
