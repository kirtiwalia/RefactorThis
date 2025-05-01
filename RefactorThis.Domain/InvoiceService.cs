using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
  public class InvoiceService
  {
    private readonly InvoiceRepository _invoiceRepository;

    public InvoiceService(InvoiceRepository invoiceRepository)
    {
      _invoiceRepository = invoiceRepository;
    }

    public string ProcessPayment(Payment payment)
    {
      if (payment == null)
      {
        throw new ArgumentNullException(nameof(payment));
      }

      if (string.IsNullOrEmpty(payment.Reference))
      {
        throw new ArgumentException("Payment reference is required", nameof(payment.Reference));
      }

      var invoice = _invoiceRepository.GetInvoice(payment.Reference);
      if (invoice == null)
      {
        throw new InvalidOperationException("There is no invoice matching this payment");
      }

      ValidateInvoiceState(invoice);

      var responseMessage = ProcessPaymentBasedOnState(invoice, payment);

      _invoiceRepository.SaveInvoice(invoice);

      return responseMessage;
    }

    private void ValidateInvoiceState(Invoice invoice)
    {
      if (invoice.Amount == 0 && invoice.Payments.Any())
      {
        throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
      }
    }

    private string ProcessPaymentBasedOnState(Invoice invoice, Payment payment)
    {
      if (invoice.Amount == 0)
      {
        return "no payment needed";
      }

      if (invoice.IsFullyPaid())
      {
        return "invoice was already fully paid";
      }

      if (payment.Amount > invoice.GetRemainingAmount())
      {
        return "the payment is greater than the partial amount remaining";
      }

      return invoice.HasExistingPayments()
          ? ProcessPartialPayment(invoice, payment)
          : ProcessFirstPayment(invoice, payment);
    }

    private string ProcessPartialPayment(Invoice invoice, Payment payment)
    {
      invoice.ApplyPayment(payment);

      return invoice.IsFullyPaid()
          ? "final partial payment received, invoice is now fully paid"
          : "another partial payment received, still not fully paid";
    }

    private string ProcessFirstPayment(Invoice invoice, Payment payment)
    {
      if (payment.Amount > invoice.Amount)
      {
        return "the payment is greater than the invoice amount";
      }

      invoice.ApplyPayment(payment);

      return invoice.IsFullyPaid()
          ? "invoice is now fully paid"
          : "invoice is now partially paid";
    }
  }
}