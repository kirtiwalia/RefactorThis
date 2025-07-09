using Moq;
using RefactorThis.Core.entities;
using RefactorThis.Core.Interfaces;
using System;
using System.Collections.Generic;
using Xunit;


namespace RefactorThis.Domain.Tests
{

    public class InvoicePaymentProcessorTests
    {
        private IInvoiceService _invoiceService;
        private void SetUpRepository(Invoice invoice)
        {

            var mockInvoiceRepo = new Mock<IInvoiceRepository>();
            mockInvoiceRepo.Setup(x => x.GetInvoice(It.IsAny<string>())).Returns(invoice);
            _invoiceService = new InvoiceService(mockInvoiceRepo.Object);          
        }

        [Fact]
        public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
        {
            var payment = new Payment();
            SetUpRepository(null);

            Assert.Throws(typeof(ArgumentNullException), () => _invoiceService.ProcessPayment(payment));

        }
        [Fact]
        public void ProcessPayment_Should_ThrowException_When_PaymentReferenceIsNull()
        {
            var payment = new Payment
            {
                Amount = 1,
                Reference = null
            };
            SetUpRepository(null);
            Assert.Throws(typeof(ArgumentNullException), () => _invoiceService.ProcessPayment(payment));

        }
        [Fact]
        public void ProcessPayment_Should_ThrowException_When_PaymentAmountIsLessThanOrEqualsToZero()
        {

            var payment = new Payment
            {
                Amount = 0,
                Reference = "TestRef"
            };
            SetUpRepository(null);

            Assert.Throws(typeof(InvalidOperationException), () => _invoiceService.ProcessPayment(payment));

        }

        [Fact]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {

            var invoice = new Invoice()
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null
            };
            var payment = new Payment
            {
                Amount = 100,
                Reference = "TestRef"
            };

            SetUpRepository(invoice);

            var result = _invoiceService.ProcessPayment(payment);

            Assert.Equal("no payment needed", result);
        }

        [Fact]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            var invoice = new Invoice()
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

            var payment = new Payment
            {
                Amount = 100,
                Reference = "TestRef"
            };

            SetUpRepository(invoice);
            var result = _invoiceService.ProcessPayment(payment);

            Assert.Equal("invoice was already fully paid", result);
        }

        [Fact]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {

            var invoice = new Invoice()
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

            var payment = new Payment
            {
                Amount = 6,
                Reference = "TestRef"
            };

            SetUpRepository(invoice);
            var result = _invoiceService.ProcessPayment(payment);

            Assert.Equal("the payment is greater than the partial amount remaining", result);
        }

        [Fact]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {

            var invoice = new Invoice()
            {
                Amount = 5,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            var payment = new Payment
            {
                Amount = 6,
                Reference = "TestRef"
            };

            SetUpRepository(invoice);
            var result = _invoiceService.ProcessPayment(payment);

            Assert.Equal("the payment is greater than the invoice amount", result);
        }

        [Fact]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {

            var invoice = new Invoice()
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
            var payment = new Payment()
            {
                Amount = 5,
                Reference = "TestRef"
            };

            SetUpRepository(invoice);
            var result = _invoiceService.ProcessPayment(payment);

            Assert.Equal("final partial payment received, invoice is now fully paid", result);
        }

        [Fact]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {

            var invoice = new Invoice()
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>() { new Payment() { Amount = 10 } }
            };
            var payment = new Payment()
            {
                Amount = 10,
                Reference = "TestRef"
            };

            SetUpRepository(invoice);
            var result = _invoiceService.ProcessPayment(payment);

            Assert.Equal("invoice was already fully paid", result);
        }

        [Fact]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {

            var invoice = new Invoice()
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
            var payment = new Payment()
            {
                Amount = 1,
                Reference = "TestRef"
            };

            SetUpRepository(invoice);
            var result = _invoiceService.ProcessPayment(payment);

            Assert.Equal("another partial payment received, still not fully paid", result);
        }

        [Fact]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {

            var invoice = new Invoice()
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };

            var payment = new Payment()
            {
                Amount = 1,
                Reference = "TestRef"
            };

            SetUpRepository(invoice);
            var result = _invoiceService.ProcessPayment(payment);

            Assert.Equal("invoice is now partially paid", result);
        }
    }
}