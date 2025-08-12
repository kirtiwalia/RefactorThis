using System.Collections.Generic;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public static class InvoiceExtensions
    {
        public static void EnsurePaymentsCollection(this Invoice invoice)
        {
            if (invoice.Payments == null)
                invoice.Payments = new List<Payment>();
        }
    }
}