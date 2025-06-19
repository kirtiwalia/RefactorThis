using RefactorThis.Domain.Entities;

namespace RefactorThis.Application.Commands
{
    public class ProcessPaymentCommand
    {
        public Payment Payment { get; set; }
        public Invoice Invoice{ get; set; }
    }
}
