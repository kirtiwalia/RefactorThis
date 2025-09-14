using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    // Result class to encapsulate success status and message
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public static PaymentResult SuccessResult(string message) => new PaymentResult { Success = true, Message = message };
        public static PaymentResult ErrorResult(string message) => new PaymentResult { Success = false, Message = message };
    }

    public class InvoiceService
    {
        private readonly InvoiceRepository _invoiceRepository;

        public InvoiceService(InvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        private bool HasPayments(Invoice invoice) => invoice.Payments != null && invoice.Payments.Any();

        public PaymentResult ProcessPayment(Payment payment)
        {
            try
            {
                var invoice = _invoiceRepository.GetInvoice(payment.Reference);
                if (invoice == null)
                    return PaymentResult.ErrorResult("There is no invoice matching this payment");

                // Validate invoice amount and payments
                if (invoice.Amount == 0)
                {
                    if (invoice.Payments == null || invoice.Payments.Count == 0)
                        return PaymentResult.SuccessResult("No payment needed");
                    return PaymentResult.ErrorResult("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
                }

                // Cache values to avoid repeated enumeration
                var payments = invoice.Payments;

                decimal totalPaid = 0m;
                if (HasPayments(invoice))
                {
                    // Use for loop for better performance on large lists
                    for (int i = 0; i < payments.Count; i++)
                        totalPaid += payments[i].Amount;
                }
                decimal remaining = invoice.Amount - invoice.AmountPaid;

                // Process payment based on existing payments made
                if (HasPayments(invoice))
                    return ProcessInvoiceWithPayments(totalPaid, remaining, invoice, payment);

                return ProcessInvoiceWithoutPayments(invoice, payment);


            }
            catch (Exception ex)
            {
                return PaymentResult.ErrorResult($"An error occurred while processing the payment: {ex.Message}");
            }
        }

        // Apply payment methods to encapsulate payment logic
        private PaymentResult ProcessInvoiceWithPayments(decimal totalPaid, decimal remaining, Invoice invoice, Payment payment)
        {
            if (totalPaid != 0 && invoice.Amount == totalPaid)
                return PaymentResult.SuccessResult("Invoice was already fully paid");

            if (totalPaid != 0 && payment.Amount > remaining)
                return PaymentResult.ErrorResult("The payment is greater than the partial amount remaining");

            if (remaining == payment.Amount)
            {
                CalculatePaymentForInvoice(invoice, payment, true);
                return PaymentResult.SuccessResult("Final partial payment received, invoice is now fully paid");
            }
            else
            {
                CalculatePaymentForInvoice(invoice, payment, false);
                return PaymentResult.SuccessResult("Another partial payment received, still not fully paid");
            }
        }

        // Apply full payment to the invoice
        private PaymentResult ProcessInvoiceWithoutPayments(Invoice invoice, Payment payment)
        {
            if (payment.Amount > invoice.Amount)
                return PaymentResult.ErrorResult("The payment is greater than the invoice amount");

            if (invoice.Amount == payment.Amount)
            {
                CalculatePaymentForInvoice(invoice, payment, true, true);
                return PaymentResult.SuccessResult("Invoice is now fully paid");
            }
            else
            {
                CalculatePaymentForInvoice(invoice, payment, false, true);
                return PaymentResult.SuccessResult("Invoice is now partially paid");
            }
        }

        //Calculate the tax amount and update the invoice with the payment based on the invoice type.
        private void CalculatePaymentForInvoice(Invoice invoice, Payment payment, bool isFinal, bool isFirst = false)
        {
            if (invoice.Payments == null)
                invoice.Payments = new System.Collections.Generic.List<Payment>();

            switch (invoice.Type)
            {
                case InvoiceType.Standard:
                    ProcessStandardInvoicePayment(invoice, payment, isFirst);
                    break;
                case InvoiceType.Commercial:
                    ProcessCommercialInvoicePayment(invoice, payment, isFirst);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            invoice.Payments.Add(payment);
            invoice.Save();
        }

        // Separate methods for processing different invoice types

        private void ProcessStandardInvoicePayment(Invoice invoice, Payment payment, bool isFirst)
        {
            if (isFirst)
                invoice.AmountPaid = payment.Amount;
            else
                invoice.AmountPaid += payment.Amount;
            invoice.TaxAmount = payment.Amount * 0.14m;
        }

        private void ProcessCommercialInvoicePayment(Invoice invoice, Payment payment, bool isFirst)
        {
            if (isFirst)
                invoice.AmountPaid = payment.Amount;
            else
                invoice.AmountPaid += payment.Amount;
            invoice.TaxAmount += payment.Amount * 0.14m;
        }
    }
}