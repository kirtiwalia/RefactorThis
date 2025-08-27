using RefactorThis.Persistence;
using System;

namespace RefactorThis.Domain
{
    public static class InvoiceStrategyFactory
    {
        public static IInvoiceStrategy GetStrategy(InvoiceType type)
        {
            switch (type)
            {
                case InvoiceType.Standard: return new StandardInvoiceStrategy();
                case InvoiceType.Commercial: return new CommercialInvoiceStrategy();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Unsupported invoice type");
            }
        }
    }
}
