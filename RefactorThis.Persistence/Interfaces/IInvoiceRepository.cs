using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefactorThis.Domain.Entities;
namespace RefactorThis.Persistence.Interfaces
{
    public interface IInvoiceRepository
    {
        Invoice GetByReference(Invoice invoice);
        void Save(Invoice invoice);
    }

}
