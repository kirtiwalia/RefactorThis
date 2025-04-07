using RefactorThis.Persistence.Models;

namespace RefactorThis.Domain.Models
{
    public class PaymentResult
    {
        public PaymentResult(Invoice processedInvoice, string responseMessage)
        {
            ProcessedInvoice = processedInvoice;
            ResponseMessage = responseMessage;
        }
        
        public Invoice ProcessedInvoice { get; set; }
        public string ResponseMessage { get; set; }
    }
}