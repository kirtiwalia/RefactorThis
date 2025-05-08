using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Persistence;
using RefactorThis.Domain;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        private IInvoiceRepository _repository;
        private InvoiceService _paymentProcessor;
        private const string TEST_REFERENCE = "TEST-123";

        [SetUp]
        public void Setup()
        {
            _repository = new InvoiceRepository();
            _paymentProcessor = new InvoiceService(_repository);
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
        {
            // Arrange
            var payment = new Payment { Reference = TEST_REFERENCE };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                _paymentProcessor.ProcessPayment(payment));
                
            Assert.AreEqual("There is no invoice matching this payment", exception.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            // Arrange
            var invoice = new Invoice(_repository)
            {
                Reference = TEST_REFERENCE,
                Amount = 0
            };
            _repository.Add(invoice);

            var payment = new Payment { Reference = TEST_REFERENCE };

            // Act
            var result = _paymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual("no payment needed", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            // Arrange
            var invoice = new Invoice(_repository)
            {
                Reference = TEST_REFERENCE,
                Amount = 10
            };
            
            invoice.SetAmountPaid(10);
            invoice.SetPayments(new List<Payment> { new Payment { Amount = 10 } });
            
            _repository.Add(invoice);

            var payment = new Payment { Reference = TEST_REFERENCE };

            // Act
            var result = _paymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual("invoice was already fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            // Arrange
            var invoice = new Invoice(_repository)
            {
                Reference = TEST_REFERENCE,
                Amount = 10
            };
            
            invoice.SetAmountPaid(5);
            invoice.SetPayments(new List<Payment> { new Payment { Amount = 5 } });
            
            _repository.Add(invoice);

            var payment = new Payment
            {
                Reference = TEST_REFERENCE,
                Amount = 6
            };

            // Act
            var result = _paymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual("the payment is greater than the partial amount remaining", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            // Arrange
            var invoice = new Invoice(_repository)
            {
                Reference = TEST_REFERENCE,
                Amount = 5
            };
            
            _repository.Add(invoice);

            var payment = new Payment
            {
                Reference = TEST_REFERENCE,
                Amount = 6
            };

            // Act
            var result = _paymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual("the payment is greater than the invoice amount", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            // Arrange
            var invoice = new Invoice(_repository)
            {
                Reference = TEST_REFERENCE,
                Amount = 10
            };
            
            invoice.SetAmountPaid(5);
            invoice.SetPayments(new List<Payment> { new Payment { Amount = 5 } });
            
            _repository.Add(invoice);

            var payment = new Payment
            {
                Reference = TEST_REFERENCE,
                Amount = 5
            };

            // Act
            var result = _paymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            // Arrange
            var invoice = new Invoice(_repository)
            {
                Reference = TEST_REFERENCE,
                Amount = 10
            };
            
            _repository.Add(invoice);

            var payment = new Payment
            {
                Reference = TEST_REFERENCE,
                Amount = 10
            };

            // Act
            var result = _paymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual("invoice is now fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            // Arrange
            var invoice = new Invoice(_repository)
            {
                Reference = TEST_REFERENCE,
                Amount = 10
            };
            
            invoice.SetAmountPaid(5);
            invoice.SetPayments(new List<Payment> { new Payment { Amount = 5 } });
            
            _repository.Add(invoice);

            var payment = new Payment
            {
                Reference = TEST_REFERENCE,
                Amount = 1
            };

            // Act
            var result = _paymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual("another partial payment received, still not fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            // Arrange
            var invoice = new Invoice(_repository)
            {
                Reference = TEST_REFERENCE,
                Amount = 10
            };
            
            _repository.Add(invoice);

            var payment = new Payment
            {
                Reference = TEST_REFERENCE,
                Amount = 1
            };

            // Act
            var result = _paymentProcessor.ProcessPayment(payment);

            // Assert
            Assert.AreEqual("invoice is now partially paid", result);
        }
        
        [Test]
        public void ProcessPayment_Should_UpdateTaxAmount_When_CommercialInvoicePayment()
        {
            // Arrange
            var invoice = new Invoice(_repository)
            {
                Reference = TEST_REFERENCE,
                Amount = 100,
                Type = InvoiceType.Commercial
            };
            
            _repository.Add(invoice);

            var payment = new Payment
            {
                Reference = TEST_REFERENCE,
                Amount = 50
            };

            // Act
            _paymentProcessor.ProcessPayment(payment);

            // Assert
            var updatedInvoice = _repository.GetInvoice(TEST_REFERENCE);
            Assert.AreEqual(50, updatedInvoice.AmountPaid);
            Assert.AreEqual(7, updatedInvoice.TaxAmount); // 50 * 0.14
        }
    }
}