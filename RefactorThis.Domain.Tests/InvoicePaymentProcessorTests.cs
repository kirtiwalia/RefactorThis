using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Tests
{
  [TestFixture]
  public class InvoicePaymentProcessorTests
  {
    [Test]
    public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
    {
      var repo = new InvoiceRepository();
      var paymentProcessor = new InvoiceService(repo);

      var payment = new Payment { Reference = "NON_EXISTENT_REFERENCE" };

      var ex = Assert.Throws<InvalidOperationException>(() => paymentProcessor.ProcessPayment(payment));

      Assert.AreEqual("There is no invoice matching this payment", ex.Message);
    }

    [Test]
    public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
    {
      var repo = new InvoiceRepository();

      var invoice = new Invoice(repo)
      {
        Amount = 0,
        AmountPaid = 0,
        Payments = null,
      };

      repo.Add(invoice);

      var paymentProcessor = new InvoiceService(repo);

      var payment = new Payment
      {
        Reference = null,
        Amount = 0
      };

      var result = paymentProcessor.ProcessPayment(payment);

      Assert.AreEqual("no payment needed", result);
    }

    [Test]
    public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
    {
      var repo = new InvoiceRepository();

      var invoice = new Invoice(repo)
      {
        Amount = 10,
        AmountPaid = 10,
        Payments = new List<Payment>
        {
          new Payment
          {
            Amount = 10,
            Reference = "INV-123"
          },
        },
        Type = InvoiceType.Standard
      };
      repo.Add(invoice);

      var paymentProcessor = new InvoiceService(repo);

      var payment = new Payment
      {
        Amount = 1m,
        Reference = "INV-123"
      };

      var result = paymentProcessor.ProcessPayment(payment);

      Assert.AreEqual("invoice was already fully paid", result);
    }

    [Test]
    public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
    {
      var repo = new InvoiceRepository();
      var invoice = new Invoice(repo)
      {
        Amount = 10,
        AmountPaid = 5,
        Payments = new List<Payment>
        {
          new Payment
          {
            Amount = 5,
            Reference = "INV-001"
          }
        },
        Type = InvoiceType.Standard
      };
      repo.Add(invoice);

      var paymentProcessor = new InvoiceService(repo);

      var payment = new Payment()
      {
        Amount = 6,
        Reference = "INV-001"
      };

      var result = paymentProcessor.ProcessPayment(payment);

      Assert.AreEqual("the payment is greater than the partial amount remaining", result);
    }

    [Test]
    public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
    {
      var repo = new InvoiceRepository();
      var invoice = new Invoice(repo)
      {
        Amount = 5,
        AmountPaid = 0,
        Payments = new List<Payment>(),
        Type = InvoiceType.Standard
      };
      repo.Add(invoice);

      var paymentProcessor = new InvoiceService(repo);

      var payment = new Payment()
      {
        Amount = 6,
        Reference = "INV-002"
      };

      var result = paymentProcessor.ProcessPayment(payment);

      Assert.AreEqual("the payment is greater than the invoice amount", result);
    }

    [Test]
    public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
    {
      var repo = new InvoiceRepository();
      var invoice = new Invoice(repo)
      {
        Amount = 10,
        AmountPaid = 5,
        Payments = new List<Payment>
        {
          new Payment
          {
            Amount = 5
          }
        }
      };
      repo.Add(invoice);

      var paymentProcessor = new InvoiceService(repo);

      var payment = new Payment()
      {
        Amount = 5
      };

      var result = paymentProcessor.ProcessPayment(payment);

      Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
    }

    [Test]
    public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
    {
      var repo = new InvoiceRepository();
      var invoice = new Invoice(repo)
      {
        Amount = 10m,
        AmountPaid = 5m,
        Payments = new List<Payment>
        {
            new Payment { Amount = 5m }
        },
        Type = InvoiceType.Standard
      };
      repo.Add(invoice);

      var paymentProcessor = new InvoiceService(repo);

      var payment = new Payment
      {
        Amount = 5m,
        Reference = "INV-003"
      };

      var result = paymentProcessor.ProcessPayment(payment);

      Assert.AreEqual("invoice was already fully paid", result);
    }

    [Test]
    public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
    {
      var repo = new InvoiceRepository();
      var invoice = new Invoice(repo)
      {
        Amount = 10,
        AmountPaid = 5,
        Payments = new List<Payment>
        {
          new Payment
          {
            Amount = 5
          }
        }
      };
      repo.Add(invoice);

      var paymentProcessor = new InvoiceService(repo);

      var payment = new Payment
      {
        Amount = 1,
        Reference = "INV-004"
      };

      var result = paymentProcessor.ProcessPayment(payment);

      Assert.AreEqual("another partial payment received, still not fully paid", result);
    }

    [Test]
    public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
    {
      var repo = new InvoiceRepository();
      var invoice = new Invoice(repo)
      {
        Amount = 10,
        AmountPaid = 0,
        Payments = new List<Payment>(),
        Type = InvoiceType.Standard
      };
      repo.Add(invoice);

      var paymentProcessor = new InvoiceService(repo);

      var payment = new Payment()
      {
        Amount = 1,
        Reference = "INV-005"
      };

      var result = paymentProcessor.ProcessPayment(payment);

      Assert.AreEqual("invoice is now partially paid", result);
    }
  }
}