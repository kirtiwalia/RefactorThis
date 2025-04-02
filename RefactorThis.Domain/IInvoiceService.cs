using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public interface IInvoiceService
    {
        string ProcessPayment(Payment payment);
    }
}
