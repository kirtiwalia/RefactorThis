using RefactorThis.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Services
{
    public interface IInvoiceService
    {
        /// <summary>
        /// Process the payment for the invoice that matches the payment's reference.
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        string ProcessPayment(Payment payment);
    }
}
