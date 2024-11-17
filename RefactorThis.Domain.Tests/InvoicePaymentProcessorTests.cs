using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using RefactorThis.Persistence;
using RefactorThis.Persistence.Entity;
using RefactorThis.Persistence.Enum;
using RefactorThis.Persistence.Implementation;
using RefactorThis.Persistence.Interface;

namespace RefactorThis.Domain.Tests
{
	[TestFixture]
	public class InvoicePaymentProcessorTests
	{
		private Mock<IInvoiceRepository> _mockInvoiceRepository;
		private InvoiceService _invoiceService;

		[SetUp]
		public void SetUp()
		{
			_mockInvoiceRepository = new Mock<IInvoiceRepository>();
			_invoiceService = new InvoiceService(_mockInvoiceRepository.Object);
		}

		[Test]
		public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
		{
			var payment = new Payment { Reference = "REF01" };
			_mockInvoiceRepository.Setup(repo => repo.GetInvoice(payment.Reference)).Returns((Invoice)null);

			Assert.Multiple(() =>
			{
				var ex = Assert.Throws<InvalidOperationException>(() => _invoiceService.ProcessPayment(payment));
				Assert.That(ex.Message, Is.EqualTo("There is no invoice matching this payment"));
			});
		}

		[Test]
		public void ProcessPayment_Should_ThrowException_When_NoInvoiceIsInvalid()
		{
			var invoice = CreateInvoice(0, 0, new List<Payment> { new Payment { Amount = 10 } }, InvoiceType.Standard);
			var payment = new Payment { Reference = "REF01", Amount = 10 };
			_mockInvoiceRepository.Setup(repo => repo.GetInvoice(payment.Reference)).Returns(invoice);

			Assert.Multiple(() =>
			{
				var ex = Assert.Throws<InvalidOperationException>(() => _invoiceService.ProcessPayment(payment));
				Assert.That(ex.Message, Is.EqualTo("The invoice is in an invalid state."));
			});
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
		{
			var invoice = CreateInvoice(0, 0, null, InvoiceType.Standard);
			var payment = new Payment { Reference = "REF01", Amount = 50 };
			_mockInvoiceRepository.Setup(repo => repo.GetInvoice(payment.Reference)).Returns(invoice);

			var result = _invoiceService.ProcessPayment(payment);

			Assert.That(result, Is.EqualTo("no payment needed"));
		}

		[Test]
		public void ProcessPayment_UpdatesInvoiceAndReturnsStatusMessage_ForValidPayment()
		{
			var invoice = CreateInvoice(20, 0, new List<Payment>(), InvoiceType.Standard);
			var payment = new Payment { Reference = "REF01", Amount = 10 };
			_mockInvoiceRepository.Setup(repo => repo.GetInvoice(payment.Reference)).Returns(invoice);

			var result = _invoiceService.ProcessPayment(payment);


			Assert.That(result, Is.EqualTo("invoice is now partially paid"));
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
		{
			var invoice = CreateInvoice(10, 10, new List<Payment>(){
					new Payment
					{
						Amount = 10,
						Reference = "REF01"
					}
				}, InvoiceType.Standard);
			var payment = new Payment { Reference = "REF02", Amount = 10 };
			_mockInvoiceRepository.Setup(repo => repo.GetInvoice(payment.Reference)).Returns(invoice);

			var result = _invoiceService.ProcessPayment(payment);

			Assert.That(result, Is.EqualTo("invoice was already fully paid"));
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
		{
			var invoice = CreateInvoice(10, 5, new List<Payment>(){
					new Payment
					{
						Amount = 5,
						Reference = "REF01"
					}
				}, InvoiceType.Standard);
			var payment = new Payment { Reference = "REF02", Amount = 10 };
			_mockInvoiceRepository.Setup(repo => repo.GetInvoice(payment.Reference)).Returns(invoice);

			var result = _invoiceService.ProcessPayment(payment);

			Assert.That(result, Is.EqualTo("the payment is greater than the partial amount remaining"));
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
		{
			var invoice = CreateInvoice(10, 0, new List<Payment>(), InvoiceType.Standard);
			var payment = new Payment { Reference = "REF01", Amount = 11 };
			_mockInvoiceRepository.Setup(repo => repo.GetInvoice(payment.Reference)).Returns(invoice);

			var result = _invoiceService.ProcessPayment(payment);

			Assert.That(result, Is.EqualTo("the payment is greater than the invoice amount"));
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
		{
			var invoice = CreateInvoice(10, 5, new List<Payment>(){
					new Payment
					{
						Amount = 5,
						Reference = "REF01"
					}
				}, InvoiceType.Standard);
			var payment = new Payment { Reference = "REF02", Amount = 5 };
			_mockInvoiceRepository.Setup(repo => repo.GetInvoice(payment.Reference)).Returns(invoice);

			var result = _invoiceService.ProcessPayment(payment);

			Assert.That(result, Is.EqualTo("final partial payment received, invoice is now fully paid"));
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
		{
			var invoice = CreateInvoice(10, 10, new List<Payment>(){
					new Payment
					{
						Amount = 10,
						Reference = "REF01"
					}
				}, InvoiceType.Standard);
			var payment = new Payment { Reference = "REF02", Amount = 10 };
			_mockInvoiceRepository.Setup(repo => repo.GetInvoice(payment.Reference)).Returns(invoice);

			var result = _invoiceService.ProcessPayment(payment);

			Assert.That(result, Is.EqualTo("invoice was already fully paid"));
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
		{
			var invoice = CreateInvoice(10, 5, new List<Payment>(){
					new Payment
					{
						Amount = 5,
						Reference = "REF01"
					}
				}, InvoiceType.Standard);
			var payment = new Payment { Reference = "REF02", Amount = 2 };
			_mockInvoiceRepository.Setup(repo => repo.GetInvoice(payment.Reference)).Returns(invoice);

			var result = _invoiceService.ProcessPayment(payment);

			Assert.That(result, Is.EqualTo("another partial payment received, still not fully paid"));
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
		{
			var invoice = CreateInvoice(10, 0, new List<Payment>(), InvoiceType.Standard);
			var payment = new Payment { Reference = "REF01", Amount = 2 };
			_mockInvoiceRepository.Setup(repo => repo.GetInvoice(payment.Reference)).Returns(invoice);

			var result = _invoiceService.ProcessPayment(payment);

			Assert.That(result, Is.EqualTo("invoice is now partially paid"));
		}

		#region Helper Methods
		private Invoice CreateInvoice(decimal amount, decimal amountPaid, List<Payment> payments, InvoiceType type)
		{
			var invoice = new Invoice(_mockInvoiceRepository.Object)
			{
				Amount = amount,
				AmountPaid = amountPaid,
				Payments = payments,
				Type = type
			};
			return invoice;
		}
		#endregion
	}
}