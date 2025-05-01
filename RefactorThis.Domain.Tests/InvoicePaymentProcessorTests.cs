using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Domain.Loggers;
using RefactorThis.Domain.Models;
using RefactorThis.Domain.Services;
using RefactorThis.Domain.Tests.Mocks;
using RefactorThis.Domain.Validators;
using RefactorThis.Persistence.Entities;
using RefactorThis.Persistence.Repositories;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        private IInvoiceService _invoiceService;
        private IInvoiceRepository _invoiceRepository;
        private IInvoiceValidator _invoiceValidator;
        private IAppLogger _logger;

        [SetUp]
        public void BeforeEachTest()
        {
            _invoiceRepository = new MockInvoiceRepository();
            _invoiceValidator = new InvoiceValidator();
            _logger = new AppLogger<InvoicePaymentProcessorTests>();
            _invoiceService = new InvoiceService(_invoiceRepository, _invoiceValidator, _logger);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailure_When_NoInvoiceFoundForPaymentReference()
        {
            var payment = new Payment();
            var result = _invoiceService.ProcessPayment(payment);
            
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ResultCode.InvoiceNotFound, result.Code);
            Assert.AreEqual("There is no invoice matching this payment", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnSuccess_When_NoPaymentNeeded()
        {
            var invoice = new Invoice { Amount = 0, Payments = null };
            _invoiceRepository.Add(invoice);
            
            var payment = new Payment();
            var result = _invoiceService.ProcessPayment(payment);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(ResultCode.NoPaymentNeeded, result.Code);
            Assert.AreEqual("no payment needed", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnSuccess_When_InvoiceAlreadyFullyPaid()
        {
            var invoice = new Invoice
            {
                Amount = 10,
                Payments = new List<Payment> { new Payment { Amount = 10 } }
            };
            _invoiceRepository.Add(invoice);
            
            var payment = new Payment();
            
            var result = _invoiceService.ProcessPayment(payment);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(ResultCode.AlreadyFullyPaid, result.Code);
            Assert.AreEqual("invoice was already fully paid", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailure_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 10,
                Payments = new List<Payment> { new Payment { Amount = 5 } }
            };
            _invoiceRepository.Add(invoice);
            
            var payment = new Payment { Amount = 6 };

            var result = _invoiceService.ProcessPayment(payment);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ResultCode.PaymentExceedsRemainingAmount, result.Code);
            Assert.AreEqual("the payment is greater than the partial amount remaining", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailure_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            // Arrange
            var invoice = new Invoice { Amount = 5, Payments = new List<Payment>() };
            _invoiceRepository.Add(invoice);
            var payment = new Payment { Amount = 6 };

            // Act
            var result = _invoiceService.ProcessPayment(payment);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ResultCode.PaymentExceedsInvoiceAmount, result.Code);
            Assert.AreEqual("the payment is greater than the invoice amount", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnSuccess_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 10,
                Payments = new List<Payment> { new Payment { Amount = 5 } }
            };
            _invoiceRepository.Add(invoice);
            var payment = new Payment { Amount = 5 };

            // Act
            var result = _invoiceService.ProcessPayment(payment);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(ResultCode.FinalPaymentComplete, result.Code);
            Assert.AreEqual("final partial payment received, invoice is now fully paid", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnSuccess_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            // Arrange
            var invoice = new Invoice
            {
                Amount = 10,
                Payments = new List<Payment> { new Payment { Amount = 5 } }
            };
            _invoiceRepository.Add(invoice);
            var payment = new Payment { Amount = 1 };

            // Act
            var result = _invoiceService.ProcessPayment(payment);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(ResultCode.PartialPaymentComplete, result.Code);
            Assert.AreEqual("partial payment received, invoice is still not fully paid", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnSuccess_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            // Arrange
            var invoice = new Invoice { Amount = 10, Payments = new List<Payment>() };
            _invoiceRepository.Add(invoice);
            var payment = new Payment { Amount = 1 };

            // Act
            var result = _invoiceService.ProcessPayment(payment);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(ResultCode.PartialPaymentComplete, result.Code);
            Assert.AreEqual("invoice is now partially paid", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_CalculateTax_When_CommercialInvoice()
        {
            // Arrange
            var invoice = new Invoice 
            { 
                Amount = 10, 
                Payments = new List<Payment>(),
                Type = InvoiceType.Commercial
            };
            _invoiceRepository.Add(invoice);
            var payment = new Payment { Amount = 10 };

            // Act
            var result = _invoiceService.ProcessPayment(payment);

            // Assert
            Assert.AreEqual(1.4m, invoice.TaxAmount);
            Assert.IsTrue(result.IsSuccess);
        }
    }
}