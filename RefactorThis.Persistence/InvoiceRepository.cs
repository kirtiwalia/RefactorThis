using RefactorThis.Domain.Interfaces;
using RefactorThis.Domain.Models;

namespace RefactorThis.Persistence
{
	/// <summary>
	/// Defines a repository that manages invoices
	/// </summary>
	public class InvoiceRepository : IInvoiceRepository
    {
		private Invoice _invoice;

        /// <inheritdoc />
        public Invoice GetInvoice( string reference )
		{
			return _invoice;
		}

        /// <inheritdoc />
        public void SaveInvoice( Invoice invoice )
		{
			//saves the invoice to the database
		}

        /// <inheritdoc />
        public void Add( Invoice invoice )
		{
			_invoice = invoice;
		}
	}
}