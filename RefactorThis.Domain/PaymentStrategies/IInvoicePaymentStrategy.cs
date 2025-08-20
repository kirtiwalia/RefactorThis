using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Application.Interfaces
{
    public interface IInvoicePaymentStrategy
    {
        PaymentResultCode ApplyPayment(Invoice invoice, Payment payment);
    }
}
