using RefactorThis.Persistence.Enums;

namespace RefactorThis.Persistence
{
    public class PaymentResult
    {
        public PaymentStatus Status { get; }
        public string Message { get; }

        public PaymentResult(PaymentStatus status, string message)
        {
            Status = status;
            Message = message;
        }
    }
}
