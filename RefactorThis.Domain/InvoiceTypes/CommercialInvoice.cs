namespace RefactorThis.Domain.InvoiceTypes
{
    public class CommercialInvoice : InvoiceType
    {
        public override decimal CalculateTax(decimal amount) => amount * 0.14m;
    }
}
