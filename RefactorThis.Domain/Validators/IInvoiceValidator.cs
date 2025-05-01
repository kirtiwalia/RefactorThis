using RefactorThis.Domain.Models;
using RefactorThis.Persistence.Entities;

namespace RefactorThis.Domain.Validators
{
    public interface IInvoiceValidator
    {
        PaymentResult ValidatePayment(Invoice invoice, Payment payment);
    }
}