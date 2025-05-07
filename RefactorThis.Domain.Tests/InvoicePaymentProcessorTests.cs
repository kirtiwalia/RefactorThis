using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using RefactorThis.Domain.enums;
using RefactorThis.Domain.messaging;
using RefactorThis.Persistence.messaging;
using RefactorThis.Persistence.models;
using RefactorThis.Persistence.repositories;

namespace RefactorThis.Domain.Tests
{
	[TestFixture]
	public class InvoicePaymentProcessorTests
	{
		private static Guid _invoiceId = Guid.NewGuid();
		private Mock<IInvoiceRepository> _mockRepo;

		[SetUp]
		public void Setup()
		{
			_mockRepo = new Mock<IInvoiceRepository>();
		}

		[Test]
		public void ProcessPayment_Should_ThrowException_When_NoInvoiceFound( )
		{
			_mockRepo.Setup(repo => repo.GetInvoice(_invoiceId)).Throws(new InvalidOperationException(InvoiceRepositoryMessages.NoInvoiceFound));

			var paymentProcessor = new InvoiceService( _mockRepo.Object );
			var payment = new Payment();

			var ex = Assert.Throws<InvalidOperationException>(() => paymentProcessor.ProcessPayment(payment, _invoiceId));

			Assert.That(ex.Message, Is.EqualTo(InvoiceRepositoryMessages.NoInvoiceFound));
		}

        [Test]
        public void ProcessPayment_Should_ThrowException_When_InvoiceWithPaymentAlreadyExists()
        {
			var payment = new Payment {
				Reference = "Reference"
			};
            var invoice = new Invoice
            {
                Id = _invoiceId,
                Amount = 10,
                Payments = null
            };
            _mockRepo.Setup(repo => repo.GetInvoice(_invoiceId)).Returns(invoice);
            _mockRepo.Setup(repo => repo.FindInvoiceWithPayment(payment.Reference)).Returns(new Invoice());

            var paymentProcessor = new InvoiceService(_mockRepo.Object);

            var ex = Assert.Throws<InvalidOperationException>(() => paymentProcessor.ProcessPayment(payment, _invoiceId));

            Assert.That(ex.Message, Is.EqualTo(ProcessPaymentErrorMessages.InvoiceWithPaymentAlreadyExists));
        }

		public static IEnumerable<TestCaseData> InvalidPaymentScenarios =>
			new[]
			{
				new TestCaseData(new Invoice { Id = _invoiceId, Amount = 0, Payments = null}, new Payment(), ProcessPaymentErrorMessages.NoPaymentNecessary),
                new TestCaseData(new Invoice { Id = _invoiceId, Amount = 0, Payments = new List<Payment>()}, new Payment(), ProcessPaymentErrorMessages.NoPaymentNecessary),
                new TestCaseData(new Invoice { Id = _invoiceId, Amount = 10, Payments = new List<Payment>{ new Payment {Amount = 10} } }, new Payment(), ProcessPaymentErrorMessages.InvoiceAlreadyPaid),
                new TestCaseData(new Invoice { Id = _invoiceId, Amount = 10, Payments = new List<Payment>{ new Payment {Amount = 5} } }, new Payment { Amount = 6 }, ProcessPaymentErrorMessages.PaymentTooGreat),
                new TestCaseData(new Invoice { Id = _invoiceId, Amount = 5, Payments = new List<Payment>() }, new Payment { Amount = 6 }, ProcessPaymentErrorMessages.PaymentTooGreat)
            };
        
		[Test, TestCaseSource(nameof(InvalidPaymentScenarios))]
		public void ProcessPayment_Should_ThrowException_When_InvalidPayments(Invoice invoice, Payment payment, string errorMessage)
		{
            _mockRepo.Setup(repo => repo.GetInvoice(_invoiceId)).Returns(invoice);

			var paymentProcessor = new InvoiceService( _mockRepo.Object );

            var ex = Assert.Throws<InvalidOperationException>(() => paymentProcessor.ProcessPayment(payment, _invoiceId));

            Assert.That(ex.Message, Is.EqualTo(errorMessage) );
		}

		public static IEnumerable<TestCaseData> ValidPaymentScenarios =>
			new[]
			{
				new TestCaseData(new Invoice{Id = _invoiceId, Amount = 10, Payments = new List<Payment>{ new Payment {  Amount = 5} }}, new Payment { Amount = 5}, InvoicePaymentStatus.Paid),
                new TestCaseData(new Invoice{Id = _invoiceId, Amount = 10, Payments = new List<Payment>() }, new Payment { Amount = 10}, InvoicePaymentStatus.Paid),
                new TestCaseData(new Invoice{Id = _invoiceId, Amount = 10, Payments = new List<Payment>{ new Payment {  Amount = 5} }}, new Payment { Amount = 1}, InvoicePaymentStatus.PartiallyPaid),
                new TestCaseData(new Invoice{Id = _invoiceId, Amount = 10, Payments = new List<Payment>() }, new Payment { Amount = 1}, InvoicePaymentStatus.PartiallyPaid)
            };

        [Test, TestCaseSource(nameof(ValidPaymentScenarios))]
		public void ProcessPayment_Should_ReturnCorrectStatus_When_ValidPaymentMade(Invoice invoice, Payment payment, InvoicePaymentStatus status )
		{
			_mockRepo.Setup(repo => repo.GetInvoice(_invoiceId)).Returns( invoice );

			var paymentProcessor = new InvoiceService( _mockRepo.Object );

			var result = paymentProcessor.ProcessPayment( payment, _invoiceId );

			Assert.That(result, Is.EqualTo(status));
		}
	}
}