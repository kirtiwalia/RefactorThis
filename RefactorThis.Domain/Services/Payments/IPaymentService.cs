using RefactorThis.Domain.Models;
using RefactorThis.Persistence.Models;

namespace RefactorThis.Domain.Services.Payments
{
    public interface IPaymentService
    {
        PaymentResult ProcessPayment(Invoice invoice, Payment payment);
    }
}