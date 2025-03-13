using RefactorThis.Persistence.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence
{
    /// <summary>
    /// An implementation of <see cref="IInvoiceRepository"/> that stores invoices in memory.
    /// </summary>
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly List<Invoice> _invoices = new List<Invoice>();

        ///<inheritdoc/>
        public Invoice GetInvoice(string reference)
        {
            return _invoices.Where(invoice => invoice.Reference == reference).FirstOrDefault();
        }

        ///<inheritdoc/>
        public void SaveInvoice(Invoice invoice)
        {
            var existingInvoice = GetInvoice(invoice.Reference);
            if (existingInvoice != null)
            {
                _invoices.Remove(existingInvoice);
            }
            _invoices.Add(invoice);
        }

        ///<inheritdoc/>
        public void Add(Invoice invoice)
        {
            _invoices.Add(invoice);
        }
    }
}
