using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Persistence;
using Moq;

namespace RefactorThis.Domain.Tests
{
    /// <summary>
    /// Used Moq to mock the IInvoiceRepository dependency for testing the InvoiceService & to achive Test Coverage
    /// </summary>
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        private Mock<IInvoiceRepository> mockInvoiceRepository;
        private InvoiceService paymentProcessor;

        [SetUp]
        public void SetUp()
        {
            mockInvoiceRepository = new Mock<IInvoiceRepository>();
            paymentProcessor = new InvoiceService(mockInvoiceRepository.Object);
        }

        private static Payment PaymentWithRef(string reference, decimal amount = 0)
            => new Payment { Reference = reference, Amount = amount };

        private void SetupInvoice(string reference, Invoice invoice)
        {
            mockInvoiceRepository.Setup(r => r.GetInvoice(reference)).Returns(invoice);
        }

        [Test]
        public void ProcessPayment_Should_ThrowArgumentNullException_When_PaymentIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                paymentProcessor.ProcessPayment(null));
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
        {
            var payment = PaymentWithRef("notfound");
            SetupInvoice("notfound", null);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                paymentProcessor.ProcessPayment(payment));
            Assert.That(ex.Message, Is.EqualTo(ResponseMessages.NoInvoiceMatchingPayment));
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_ZeroAmountInvoiceHasPayments()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = new List<Payment> { new Payment { Amount = 1 } }
            };
            SetupInvoice("ref1", invoice);

            var payment = PaymentWithRef("ref1");

            var ex = Assert.Throws<InvalidOperationException>(() =>
                paymentProcessor.ProcessPayment(payment));
            Assert.That(ex.Message, Is.EqualTo(ResponseMessages.InvoiceInvalidZeroAmountWithPayments));
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null
            };
            SetupInvoice("ref2", invoice);

            var payment = PaymentWithRef("ref2");

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo(ResponseMessages.NoPaymentNeeded));
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment> { new Payment { Amount = 10 } }
            };
            SetupInvoice("ref3", invoice);

            var payment = PaymentWithRef("ref3");

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo(ResponseMessages.InvoiceAlreadyFullyPaid));
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment> { new Payment { Amount = 5 } }
            };
            SetupInvoice("ref4", invoice);

            var payment = PaymentWithRef("ref4", 6);

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo(ResponseMessages.PaymentGreaterPartialAmountRemaining));
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 5,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            SetupInvoice("ref5", invoice);

            var payment = PaymentWithRef("ref5", 6);

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo(ResponseMessages.PaymentGreaterInvoiceAmount));
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment> { new Payment { Amount = 5 } }
            };
            SetupInvoice("ref6", invoice);

            var payment = PaymentWithRef("ref6", 5);

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo(ResponseMessages.FinalPartialPaymentReceivedInvoiceFullyPaid));
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            SetupInvoice("ref7", invoice);

            var payment = PaymentWithRef("ref7", 10);

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo(ResponseMessages.FinalPartialPaymentReceivedInvoiceFullyPaid));
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment> { new Payment { Amount = 5 } }
            };
            SetupInvoice("ref8", invoice);

            var payment = PaymentWithRef("ref8", 1);

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo(ResponseMessages.AnotherPartialPaymentReceivedInvoiceNotFullyPaid));
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            SetupInvoice("ref9", invoice);

            var payment = PaymentWithRef("ref9", 1);

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo(ResponseMessages.InvoiceNowPartiallyPaid));
        }

        [Test]
        public void ProcessPayment_Should_Handle_NullPaymentsList()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = null
            };
            SetupInvoice("ref10", invoice);

            var payment = PaymentWithRef("ref10", 5);

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo(ResponseMessages.InvoiceNowPartiallyPaid));
        }

        [Test]
        public void ProcessPayment_Should_Handle_ZeroPaymentAmount()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            SetupInvoice("ref11", invoice);

            var payment = PaymentWithRef("ref11", 0);

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo(ResponseMessages.InvoiceNowPartiallyPaid));
        }

        [Test]
        public void ProcessPayment_Should_Handle_NegativePaymentAmount()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            SetupInvoice("ref12", invoice);

            var payment = PaymentWithRef("ref12", -1);

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo(ResponseMessages.InvoiceNowPartiallyPaid));
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_InvoiceIsPaidInMultipleSteps()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            SetupInvoice("ref13", invoice);

            var payment1 = PaymentWithRef("ref13", 4);
            var payment2 = PaymentWithRef("ref13", 6);

            var result1 = paymentProcessor.ProcessPayment(payment1);
            var result2 = paymentProcessor.ProcessPayment(payment2);

            Assert.That(result1, Is.EqualTo(ResponseMessages.InvoiceNowPartiallyPaid));
            Assert.That(result2, Is.EqualTo(ResponseMessages.FinalPartialPaymentReceivedInvoiceFullyPaid));
        }

        [Test]
        public void ProcessPayment_Should_ReturnAlreadyFullyPaid_When_ExtraPaymentAfterFullyPaid()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment> { new Payment { Amount = 10 } }
            };
            SetupInvoice("ref14", invoice);

            var payment = PaymentWithRef("ref14", 1);

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo(ResponseMessages.InvoiceAlreadyFullyPaid));
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_PaymentReferenceIsNull()
        {
            var payment = new Payment { Reference = null, Amount = 5 };
            Assert.Throws<InvalidOperationException>(() => paymentProcessor.ProcessPayment(payment));
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_PaymentReferenceIsEmpty()
        {
            var payment = new Payment { Reference = "", Amount = 5 };
            Assert.Throws<InvalidOperationException>(() => paymentProcessor.ProcessPayment(payment));
        }

        [Test]
        public void ProcessPayment_Should_Handle_LargePaymentAmount()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = decimal.MaxValue,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            SetupInvoice("large", invoice);

            var payment = PaymentWithRef("large", decimal.MaxValue);

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo(ResponseMessages.FinalPartialPaymentReceivedInvoiceFullyPaid));
        }

        [Test]
        public void ProcessPayment_Should_Handle_MultiplePayments_ExceedingInvoiceAmount()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 10,
                AmountPaid = 8,
                Payments = new List<Payment> { new Payment { Amount = 8 } }
            };
            SetupInvoice("multi", invoice);

            var payment = PaymentWithRef("multi", 5);

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo(ResponseMessages.PaymentGreaterPartialAmountRemaining));
        }

        [Test]
        public void ProcessPayment_Should_Handle_NegativeInvoiceAmount()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = -10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            SetupInvoice("neg", invoice);

            var payment = PaymentWithRef("neg", -5);

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo(ResponseMessages.NoPaymentNeeded));
        }

        [Test]
        public void ProcessPayment_Should_Handle_PaymentWithNullReference()
        {
            var payment = new Payment { Reference = null, Amount = 10 };
            Assert.Throws<InvalidOperationException>(() => paymentProcessor.ProcessPayment(payment));
        }

        [Test]
        public void ProcessPayment_Should_Handle_PaymentWithWhitespaceReference()
        {
            var invoice = new Invoice(mockInvoiceRepository.Object)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            SetupInvoice("   ", invoice);

            var payment = PaymentWithRef("   ", 5);

            Assert.Throws<InvalidOperationException>(() => paymentProcessor.ProcessPayment(payment));
        }
    }
}