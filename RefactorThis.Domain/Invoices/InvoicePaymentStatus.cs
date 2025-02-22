namespace RefactorThis.Domain.Payments
{
    public class InvoicePaymentStatus
    {
        public const string FullyPaid = "invoice is now fully paid";
        public const string PartiallyPaid = "invoice is now partially paid";
        public const string FinalPaymentFullyPaid = "final partial payment received, invoice is now fully paid";
        public const string PartialPaymentReceived = "another partial payment received, still not fully paid";
    }
}
