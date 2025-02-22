namespace RefactorThis.Domain.InvoiceTypes
{
    public abstract class InvoiceType
    {
        public abstract decimal CalculateTax(decimal amount);
    }
}
