using RefactorThis.Application.Interfaces;
using RefactorThis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain.PaymentStrategies
{
    public interface IInvoicePaymentStrategyFactory
    {
        IInvoicePaymentStrategy GetStrategy(InvoiceType type);
    }
}
