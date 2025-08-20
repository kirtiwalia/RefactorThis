using RefactorThis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Application.Interfaces
{
    public interface IInvoiceRepository
    {
        Invoice GetInvoice(string reference);
        void Add(Invoice invoice);
        void SaveInvoice(Invoice invoice);
    }
}
