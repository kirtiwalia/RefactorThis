using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        private Mock<IInvoiceRepository> _mockRepo;
        private InvoiceService _service;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IInvoiceRepository>();
            _service = new InvoiceService(_mockRepo.Object);
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
        {
            _mockRepo.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns((Invoice)null);

            var payment = new Payment { Reference = "unknown-ref" };

            var ex = Assert.Throws<InvalidOperationException>(() => _service.ProcessPayment(payment));
            Assert.AreEqual("There is no invoice matching this payment", ex.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            var invoice = new Invoice
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null
            };

            _mockRepo.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment();
            var result = _service.ProcessPayment(payment);

            Assert.AreEqual("no payment needed", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment>
                {
                    new Payment { Amount = 10 }
                }
            };

            _mockRepo.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment();
            var result = _service.ProcessPayment(payment);

            Assert.AreEqual("invoice was already fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment { Amount = 5 }
                }
            };

            _mockRepo.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment { Amount = 6 };
            var result = _service.ProcessPayment(payment);

            Assert.AreEqual("the payment is greater than the partial amount remaining", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            var invoice = new Invoice
            {
                Amount = 5,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };

            _mockRepo.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment { Amount = 6 };
            var result = _service.ProcessPayment(payment);

            Assert.AreEqual("the payment is greater than the invoice amount", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment { Amount = 5 }
                }
            };

            _mockRepo.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment { Amount = 5 };
            var result = _service.ProcessPayment(payment);

            Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment> { new Payment { Amount = 10 } }
            };

            _mockRepo.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment { Amount = 10 };
            var result = _service.ProcessPayment(payment);

            Assert.AreEqual("invoice was already fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment { Amount = 5 }
                }
            };

            _mockRepo.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment { Amount = 1 };
            var result = _service.ProcessPayment(payment);

            Assert.AreEqual("another partial payment received, still not fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };

            _mockRepo.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment { Amount = 1 };
            var result = _service.ProcessPayment(payment);

            Assert.AreEqual("invoice is now partially paid", result);
        }
    }
}
