using RefactorThis.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Persistence.Repositories
{
    public interface IInvoiceRepository
    {
        /// <summary>
        /// Returns the invoice that matched the payment reference.
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        Invoice GetInvoice(string reference);
        /// <summary>
        /// Update the invoice.
        /// </summary>
        /// <param name="invoice"></param>
        void SaveInvoice(Invoice invoice);
        /// <summary>
        /// Create a new invoice
        /// </summary>
        /// <param name="invoice"></param>
        void Add(Invoice invoice);
    }
}
