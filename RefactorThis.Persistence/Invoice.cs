using System.Collections.Generic;

namespace RefactorThis.Persistence
{
    public class Invoice
    {
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Payment> Payments { get; set; }
        public InvoiceType Type { get; set; }

        private readonly IRepository _repository;

        public Invoice(IRepository repository)
        {
            _repository = repository;
        }

        public void Save()
        {
            _repository.SaveInvoice(this);
        }

    }
}