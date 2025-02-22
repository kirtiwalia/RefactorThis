using System;

namespace RefactorThis.Domain.Invoices
{
    public class InvoiceValidator
    {
        public string Validate(Invoice invoice)
        {
            if (invoice.Amount == 0 && !invoice.HasPayments())
            {
                return InvoiceError.NoPaymentNeeded;
            }

            if (invoice.Amount == 0 && invoice.HasPayments())
            {
                throw new InvalidOperationException(InvoiceError.InvoiceInvalidState);
            }

            return string.Empty;
        }
    }
}
