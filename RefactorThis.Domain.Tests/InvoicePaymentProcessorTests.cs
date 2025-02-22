using Moq;
using NUnit.Framework;
using RefactorThis.Application;
using RefactorThis.Domain.Invoices;
using RefactorThis.Domain.Invoices.Interfaces;
using RefactorThis.Domain.InvoiceTypes;
using RefactorThis.Domain.Payments;
using System;
using System.Collections.Generic;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        private Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private InvoiceService _invoiceService;

        [SetUp]
        public void Setup()
        {
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _invoiceService = new InvoiceService(_invoiceRepositoryMock.Object);
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
        {
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns((Invoice)null);

            var payment = new Payment();

            Assert.Throws<InvalidOperationException>(() => _invoiceService.ProcessPayment(payment), InvoiceError.NoInvoiceFound);
        }


        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            var invoice =
                new Invoice
                {
                    Amount = 0,
                    Payments = new List<Payment>()
                };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment { Amount = 50 };

            var result = _invoiceService.ProcessPayment(payment);

            Assert.AreEqual(InvoiceError.NoPaymentNeeded, result);
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_ZeroBalanceButWithPayment()
        {
            var invoice =
                new Invoice
                {
                    Amount = 0,
                    Payments = new List<Payment>
                    {
                        new Payment
                        {
                            Amount = 50
                        }
                    }
                };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment { Amount = 50 };

            Assert.Throws<InvalidOperationException>(() => _invoiceService.ProcessPayment(payment), "The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            var invoice =
                new Invoice
                {
                    Amount = 10,
                    AmountPaid = 10,
                    Payments = new List<Payment>
                    {
                        new Payment
                        {
                            Amount = 10
                        }
                    },
                    Type = new StandardInvoice()
                };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment { Amount = 10 };

            var result = _invoiceService.ProcessPayment(payment);

            Assert.AreEqual(InvoiceError.InvoiceAlreadyFullyPaid, result);
            Assert.AreEqual(10, invoice.AmountPaid);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            var invoice =
                new Invoice
                {
                    Amount = 10,
                    AmountPaid = 5,
                    Payments = new List<Payment>
                    {
                        new Payment
                        {
                            Amount = 5
                        }
                    },
                    Type = new StandardInvoice()
                };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment { Amount = 6 };

            var result = _invoiceService.ProcessPayment(payment);

            Assert.AreEqual(InvoiceError.PaymentIsGreaterThanRemainingBalance, result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            var invoice =
                new Invoice
                {
                    Amount = 5,
                    AmountPaid = 0,
                    Payments = new List<Payment>(),
                    Type = new StandardInvoice()
                };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment { Amount = 6 };

            var result = _invoiceService.ProcessPayment(payment);

            Assert.AreEqual(InvoiceError.PaymentIsGreaterThanInvoiceAmount, result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            var invoice =
                new Invoice
                {
                    Amount = 10,
                    AmountPaid = 5,
                    Payments = new List<Payment>
                    {
                        new Payment
                        {
                            Amount = 5
                        }
                    },
                    Type = new StandardInvoice()
                };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment { Amount = 5 };

            var result = _invoiceService.ProcessPayment(payment);

            Assert.AreEqual(InvoicePaymentStatus.FinalPaymentFullyPaid, result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            var invoice =
               new Invoice
               {
                   Amount = 10,
                   AmountPaid = 0,
                   Payments = new List<Payment>
                   {
                        new Payment
                        {
                            Amount = 10
                        }
                   },
                   Type = new StandardInvoice()
               };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment { Amount = 10 };

            var result = _invoiceService.ProcessPayment(payment);

            Assert.AreEqual(InvoiceError.InvoiceAlreadyFullyPaid, result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            var invoice =
               new Invoice
               {
                   Amount = 10,
                   AmountPaid = 5,
                   Payments = new List<Payment>
                   {
                        new Payment
                        {
                            Amount = 5
                        }
                   },
                   Type = new StandardInvoice()
               };

            _invoiceRepositoryMock.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment { Amount = 1 };

            var result = _invoiceService.ProcessPayment(payment);

            Assert.AreEqual(InvoicePaymentStatus.PartialPaymentReceived, result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            var invoice =
               new Invoice
               {
                   Amount = 10,
                   AmountPaid = 0,
                   Payments = new List<Payment>(),
                   Type = new StandardInvoice()
               };

            _invoiceRepositoryMock.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment { Amount = 1 };

            var result = _invoiceService.ProcessPayment(payment);

            Assert.AreEqual(InvoicePaymentStatus.PartiallyPaid, result);
        }
    }
}