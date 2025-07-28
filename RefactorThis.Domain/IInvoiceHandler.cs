using RefactorThis.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain
{
    public interface IInvoiceHandler
    {
        void ApplyFullPayment(Invoice invoice, Payment payment);
        void ApplyPartialPayment(Invoice invoice, Payment payment);
    }
}