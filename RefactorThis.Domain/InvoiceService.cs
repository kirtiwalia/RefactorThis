using System;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class InvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
        }

        public string ProcessPayment(Payment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }

            // Get invoice from repository
            var invoice = _invoiceRepository.GetInvoice(payment.Reference);

            if (invoice == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            // Process payment on invoice domain object
            var result = invoice.ProcessPayment(payment);

            // Save invoice
            invoice.Save();

            // Map domain result to response message
            return GetResponseMessage(result);
        }

        // Map payment result to user-friendly messages
        private string GetResponseMessage(PaymentResult result)
        {
            switch (result)
            {
                case PaymentResult.NoPaymentNeeded:
                    return "no payment needed";

                case PaymentResult.AlreadyPaid:
                    return "invoice was already fully paid";

                case PaymentResult.ExceedsInvoiceAmount:
                    return "the payment is greater than the invoice amount";

                case PaymentResult.ExceedsPartialRemaining:
                    return "the payment is greater than the partial amount remaining";

                case PaymentResult.FullPayment:
                    return "invoice is now fully paid";

                case PaymentResult.FirstPartialPayment:
                    return "invoice is now partially paid";

                case PaymentResult.AdditionalPartialPayment:
                    return "another partial payment received, still not fully paid";

                case PaymentResult.FinalPartialPayment:
                    return "final partial payment received, invoice is now fully paid";

                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, "Unknown payment result");
            }
        }
    }
}