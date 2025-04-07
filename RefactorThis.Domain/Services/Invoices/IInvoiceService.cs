using RefactorThis.Persistence.Models;

namespace RefactorThis.Domain.Services.Invoices
{
    public interface IInvoiceService
    {
        string ProcessPayment(Payment payment);
    }
}