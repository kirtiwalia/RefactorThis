using System;
using RefactorThis.Persistence.Models;

namespace RefactorThis.Persistence.Repositories
{
    public interface IInvoiceRepository
    {
        Invoice GetById(Guid id);
        void Save(Invoice invoice);
        void Add(Invoice invoice);
    }
}