using System;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public interface ITaxCalculator
    {
        decimal Calculate(decimal amount, InvoiceType invoiceType);
    }

    public class TaxCalculator : ITaxCalculator
    {
        private const decimal StandardTaxRate = 0.14m;
        private const decimal CommercialTaxRate = 0.14m;

        public decimal Calculate(decimal amount, InvoiceType invoiceType)
        {
            switch (invoiceType)
            {
                case InvoiceType.Standard:
                    return amount * StandardTaxRate;
                case InvoiceType.Commercial:
                    return amount * CommercialTaxRate;
                default:
                    throw new ArgumentOutOfRangeException(nameof(invoiceType), invoiceType, "Unknown invoice type");
            }
        }
    }
}