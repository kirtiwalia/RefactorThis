using RefactorThis.Application.Interfaces;
using RefactorThis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain.PaymentStrategies
{
    public class InvoicePaymentStrategyFactory : IInvoicePaymentStrategyFactory
    {
        public IInvoicePaymentStrategy GetStrategy(InvoiceType type)
        {
            switch (type)
            {
                case InvoiceType.Standard:
                    return new StandardInvoicePaymentStrategy();
                case InvoiceType.Commercial:
                    return new CommercialInvoicePaymentStrategy();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported invoice type");
            }
        }
    }
}
