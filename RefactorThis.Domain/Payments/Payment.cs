namespace RefactorThis.Domain.Payments
{
    public class Payment
    {
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public PaymentTypeEnum PaymentType { get; set; }
        public bool IsInitialPayment { get; set; } = true;
    }
}
