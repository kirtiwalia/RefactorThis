using RefactorThis.Persistence;
using System.Linq;


namespace RefactorThis.Domain
{
  public static class InvoiceExtensions
  {
    public static bool IsFullyPaid(this Invoice invoice)
    {
      return invoice.Payments.Sum(x => x.Amount) == invoice.Amount;
    }

    public static decimal GetRemainingAmount(this Invoice invoice)
    {
      return invoice.Amount - invoice.AmountPaid;
    }

    public static bool HasExistingPayments(this Invoice invoice)
    {
      return invoice.Payments.Any();
    }

    public static void ApplyPayment(this Invoice invoice, Payment payment)
    {
      invoice.AmountPaid += payment.Amount;

      if (invoice.Type == InvoiceType.Commercial)
      {
        invoice.TaxAmount += payment.Amount * 0.14m;
      }

      invoice.Payments.Add(payment);
    }
  }
}
