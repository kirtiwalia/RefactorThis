using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using RefactorThis.Persistence;
using RefactorThis.Persistence.Contracts;
using RefactorThis.Persistence.Enums;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        private IInvoiceRepository _mockRepo;
        private InvoiceService _paymentProcessor;

        [SetUp]
        public void Setup()
        {
            _mockRepo = Substitute.For<IInvoiceRepository>();
            _paymentProcessor = new InvoiceService(_mockRepo);
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
        {
            _mockRepo.GetInvoice(Arg.Any<string>()).Returns((Invoice)null);

            var payment = new Payment();
            var failureMessage = "";

            try
            {
                var result = _paymentProcessor.ProcessPayment(payment);
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
            var invoice = new Invoice(_mockRepo)
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null
            };

            _mockRepo.GetInvoice(Arg.Any<string>()).Returns(invoice);

            var payment = new Payment();

            var result = _paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual(PaymentStatus.NoPaymentNeeded, result.Status);
            Assert.AreEqual("No payment needed", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            var invoice = new Invoice(_mockRepo)
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 10
                    }
                }
            };

            _mockRepo.GetInvoice(Arg.Any<string>()).Returns(invoice);

            var payment = new Payment();

            var result = _paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual(PaymentStatus.AlreadyPaid, result.Status);
            Assert.AreEqual("Invoice was already fully paid", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            var invoice = new Invoice(_mockRepo)
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

            _mockRepo.GetInvoice(Arg.Any<string>()).Returns(invoice);

            var payment = new Payment()
            {
                Amount = 6
            };

            var result = _paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual(PaymentStatus.Overpaid, result.Status);
            Assert.AreEqual("The payment is greater than the partial amount remaining", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            var invoice = new Invoice(_mockRepo)
            {
                Amount = 5,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };

            _mockRepo.GetInvoice(Arg.Any<string>()).Returns(invoice);

            var payment = new Payment()
            {
                Amount = 6
            };

            var result = _paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual(PaymentStatus.Overpaid, result.Status);
            Assert.AreEqual("The payment is greater than the invoice amount", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            var invoice = new Invoice(_mockRepo)
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

            _mockRepo.GetInvoice(Arg.Any<string>()).Returns(invoice);

            var payment = new Payment()
            {
                Amount = 5
            };

            var result = _paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual(PaymentStatus.FullyPaid, result.Status);
            Assert.AreEqual("Final partial payment received, invoice is now fully paid", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            var invoice = new Invoice(_mockRepo)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };

            _mockRepo.GetInvoice(Arg.Any<string>()).Returns(invoice);

            var payment = new Payment()
            {
                Amount = 10
            };

            var result = _paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual(PaymentStatus.FullyPaid, result.Status);
            Assert.AreEqual("Invoice is now fully paid", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            var invoice = new Invoice(_mockRepo)
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

            _mockRepo.GetInvoice(Arg.Any<string>()).Returns(invoice);

            var payment = new Payment()
            {
                Amount = 1
            };

            var result = _paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual(PaymentStatus.PartiallyPaid, result.Status);
            Assert.AreEqual("Another partial payment received, still not fully paid", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            var invoice = new Invoice(_mockRepo)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };

            _mockRepo.GetInvoice(Arg.Any<string>()).Returns(invoice);

            var payment = new Payment()
            {
                Amount = 1
            };

            var result = _paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual(PaymentStatus.PartiallyPaid, result.Status);
            Assert.AreEqual("Invoice is now partially paid", result.Message);
        }
    }
}