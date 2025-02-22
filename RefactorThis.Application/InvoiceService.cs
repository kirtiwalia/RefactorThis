using RefactorThis.Domain.Invoices;
using RefactorThis.Domain.Invoices.Interfaces;
using RefactorThis.Domain.Payments;
using System;

namespace RefactorThis.Application
{
    public class InvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly InvoiceValidator _validator;
        private readonly InvoiceProcessor _invoiceProcessor;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
            _validator = new InvoiceValidator();
            _invoiceProcessor = new InvoiceProcessor();
        }

        public string ProcessPayment(Payment payment)
        {
            var invoice = _invoiceRepository.GetInvoice(payment.Reference)
                          ?? throw new InvalidOperationException(InvoiceError.NoInvoiceFound);

            var validationResult = _validator.Validate(invoice);
            if (!string.IsNullOrEmpty(validationResult)) return validationResult;

            var result = _invoiceProcessor.Process(invoice, payment);

            _invoiceRepository.SaveInvoice(invoice);
            return result;
        }
    }
}
