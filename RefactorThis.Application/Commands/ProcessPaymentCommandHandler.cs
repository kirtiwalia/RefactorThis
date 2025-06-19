using RefactorThis.Application.CommandInterface;
using RefactorThis.Persistence.Interfaces;

namespace RefactorThis.Application.Commands
{
    public class ProcessPaymentCommandHandler : ICommandHandler<ProcessPaymentCommand, string>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public ProcessPaymentCommandHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public string Handle(ProcessPaymentCommand command)
        {
            var invoice = _invoiceRepository.GetByReference(command.Invoice);


            if (invoice == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            } 
            var result = invoice.ApplyPayment(command.Payment);


            _invoiceRepository.Save(invoice);

            return result;
        }
    }
}
