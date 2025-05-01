using RefactorThis.Domain.Models;
using RefactorThis.Persistence.Entities;

namespace RefactorThis.Domain.Services
{
    public interface IInvoiceService
    {
        PaymentResult ProcessPayment(Payment payment);
    }
}