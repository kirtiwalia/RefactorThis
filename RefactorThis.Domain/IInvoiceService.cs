using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public interface IInvoiceService
    {
        string ProcessPayment(Payment payment);
        string HandleZeroAmountInvoice(Invoice invoice);
        string HandleExistingPayments(Invoice invoice, Payment payment);
        string HandleFirstPayment(Invoice invoice, Payment payment);
        void ApplyPayment(Invoice invoice, Payment payment, bool isFirstPayment);
    }
}
