namespace RefactorThis.Domain.InvoiceTypes
{
    public class StandardInvoice : InvoiceType
    {
        public override decimal CalculateTax(decimal amount) => 0;
    }
}
