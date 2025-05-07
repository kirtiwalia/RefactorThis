using System;
using RefactorThis.Persistence.models;

namespace RefactorThis.Persistence.repositories
{
    public interface IInvoiceRepository
    {
        // I have made this use an id rather than check the payment reference because it seems to make more sense.
        Invoice GetInvoice(Guid id);
        // This function assumes that paymentReference is a unique value. This is not guaranteed though, and represents a risk.
        // If we want to be able to check this, the reference must either be unique, or we need to update this to use a unique id instead.
        Invoice FindInvoiceWithPayment(string paymentReference);
        void SaveInvoice(Invoice invoice);
    }
}
