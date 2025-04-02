using NUnit.Framework;
using RefactorThis.Persistence;
using System;
using System.Collections.Generic;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
        {
            IInvoiceRepository repo = new InvoiceRepository();
            IInvoiceService paymentProcessor = new InvoiceService(repo);
            var payment = new Payment();
            var failureMessage = "";

            try
            {
                paymentProcessor.ProcessPayment(payment);
            }
            catch (InvalidOperationException e)
            {
                failureMessage = e.Message;
            }

            Assert.AreEqual("There is no invoice matching this payment", failureMessage);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            IInvoiceRepository repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null
            };
            repo.Add(invoice);
            IInvoiceService paymentProcessor = new InvoiceService(repo);
            var payment = new Payment();
            var result = paymentProcessor.ProcessPayment(payment);
            Assert.AreEqual("no payment needed", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            IInvoiceRepository repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment>
                {
                    new Payment { Amount = 10 }
                }
            };
            repo.Add(invoice);
            IInvoiceService paymentProcessor = new InvoiceService(repo);
            var payment = new Payment();
            var result = paymentProcessor.ProcessPayment(payment);
            Assert.AreEqual("invoice was already fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            IInvoiceRepository repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment { Amount = 5 }
                }
            };
            repo.Add(invoice);
            IInvoiceService paymentProcessor = new InvoiceService(repo);
            var payment = new Payment { Amount = 6 };
            var result = paymentProcessor.ProcessPayment(payment);
            Assert.AreEqual("the payment is greater than the partial amount remaining", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            IInvoiceRepository repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 5,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            repo.Add(invoice);
            IInvoiceService paymentProcessor = new InvoiceService(repo);
            var payment = new Payment { Amount = 6 };
            var result = paymentProcessor.ProcessPayment(payment);
            Assert.AreEqual("the payment is greater than the invoice amount", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            IInvoiceRepository repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment { Amount = 5 }
                }
            };
            repo.Add(invoice);
            IInvoiceService paymentProcessor = new InvoiceService(repo);
            var payment = new Payment { Amount = 5 };
            var result = paymentProcessor.ProcessPayment(payment);
            Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            IInvoiceRepository repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment> { new Payment { Amount = 10 } }
            };
            repo.Add(invoice);
            IInvoiceService paymentProcessor = new InvoiceService(repo);
            var payment = new Payment { Amount = 10 };
            var result = paymentProcessor.ProcessPayment(payment);
            Assert.AreEqual("invoice was already fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            IInvoiceRepository repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment { Amount = 5 }
                }
            };
            repo.Add(invoice);
            IInvoiceService paymentProcessor = new InvoiceService(repo);
            var payment = new Payment { Amount = 1 };
            var result = paymentProcessor.ProcessPayment(payment);
            Assert.AreEqual("another partial payment received, still not fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            IInvoiceRepository repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            repo.Add(invoice);
            IInvoiceService paymentProcessor = new InvoiceService(repo);
            var payment = new Payment { Amount = 1 };
            var result = paymentProcessor.ProcessPayment(payment);
            Assert.AreEqual("invoice is now partially paid", result);
        }
    }
}
