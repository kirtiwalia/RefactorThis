using System.Collections.Generic;
using RefactorThis.Persistence.Contracts;
using RefactorThis.Persistence.Enums;

namespace RefactorThis.Persistence
{
    public class Invoice
    {
        private readonly IInvoiceRepository _repository;
        public Invoice(IInvoiceRepository repository)
        {
            _repository = repository;
            Payments = new List<Payment>();
        }

        public void Save()
        {
            _repository.SaveInvoice(this);
        }

        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Payment> Payments { get; set; }
        public string Reference { get; set; }

        public InvoiceType Type { get; set; }
    }
}

