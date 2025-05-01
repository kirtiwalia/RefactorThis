using System;
using System.Linq;
using RefactorThis.Domain.Loggers;
using RefactorThis.Domain.Models;
using RefactorThis.Domain.Validators;
using RefactorThis.Persistence.Entities;
using RefactorThis.Persistence.Repositories;

namespace RefactorThis.Domain.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IInvoiceValidator _invoiceValidator;
        private readonly IAppLogger _logger;

        public InvoiceService(
            IInvoiceRepository invoiceRepository, 
            IInvoiceValidator invoiceValidator,
            IAppLogger logger)
        {
            _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
            _invoiceValidator = invoiceValidator ?? throw new ArgumentNullException(nameof(invoiceValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public PaymentResult ProcessPayment(Payment payment)
        {
            try
            {
                var invoice = _invoiceRepository.GetInvoice(payment.Reference);
                var errorResult = _invoiceValidator.ValidatePayment(invoice, payment);
                
                if (errorResult != null)
                {
                    _logger.LogWarning("Payment validation failed: {0} - {1}", 
                        errorResult.Code, errorResult.Message);
                    return errorResult;
                }
                
                return ProcessValidPayment(invoice, payment);
            }
            catch (Exception ex)
            {
                var logMessage = $"Unexpected error processing payment for reference: {payment?.Reference ?? "null"}. " +
                                 $"Exception: {ex.GetType().Name} - {ex.Message}. " +
                                 $"Stack Trace: {ex.StackTrace ?? "Not available"}";
                
                var userMessage = "An unexpected error occurred while processing payment" +
                                  (payment?.Reference != null ? $" (Reference: {payment.Reference})" : "");

                _logger?.LogError(ex, logMessage);
    
                return PaymentResult.FailureResult(
                    ResultCode.ProcessingError,
                    userMessage);
            }
        }

        private PaymentResult ProcessValidPayment(Invoice invoice, Payment payment)
        {
            try
            {
                var totalPaid = invoice.Payments.Sum(p => p.Amount);
                var remainingAmount = invoice.Amount - totalPaid;
                var isFinalPayment = remainingAmount == payment.Amount;
                var hasExistingPayments = invoice.Payments.Any();

                _logger.LogInformation("Processing payment - Amount: {0}, Remaining: {1}, Final: {2}", 
                    payment.Amount, remainingAmount, isFinalPayment);

                UpdateInvoiceWithPayment(invoice, payment);
                _invoiceRepository.SaveInvoice(invoice);

                _logger.LogInformation("Successfully updated invoice {0} with payment {1}", 
                    payment.Reference, payment.Amount);

                if (isFinalPayment)
                {
                    _logger.LogInformation("Final payment completed for invoice {0}", payment.Reference);
                    return PaymentResult.SuccessResult(
                        ResultCode.FinalPaymentComplete,
                        "final partial payment received, invoice is now fully paid");
                }

                _logger.LogInformation("Partial payment processed for invoice {0}", payment.Reference);
                return PaymentResult.SuccessResult(
                    ResultCode.PartialPaymentComplete,
                    hasExistingPayments 
                        ? "partial payment received, invoice is still not fully paid" 
                        : "invoice is now partially paid");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing valid payment for reference: {0}", payment.Reference);
                throw;
            }
        }

        private void UpdateInvoiceWithPayment(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid += payment.Amount;
            invoice.Payments.Add(payment);

            if (invoice.Type != InvoiceType.Commercial) return;
            
            var tax = payment.Amount * 0.14m;
            invoice.TaxAmount += tax;
            _logger.LogInformation("Added commercial tax {0} for payment {1}", 
                tax, payment.Amount);
        }
    }
}