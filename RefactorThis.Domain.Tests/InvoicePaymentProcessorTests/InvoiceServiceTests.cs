using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using RefactorThis.Domain.Models;
using RefactorThis.Domain.Services.Invoices;
using RefactorThis.Domain.Services.Payments;
using RefactorThis.Persistence.Models;
using RefactorThis.Persistence.Repositories;

namespace RefactorThis.Domain.Tests.InvoicePaymentProcessorTests
{
	[TestFixture]
	public class InvoiceServiceTests
	{
		private readonly IInvoiceRepository _mockInvoiceRepository = Substitute.For<IInvoiceRepository>();
		private readonly IPaymentService _mockPaymentService = Substitute.For<IPaymentService>();
		private InvoiceService _invoiceService;

		public InvoiceServiceTests()
		{
			SetupInvoiceService();
		}
		
		private void SetupInvoiceService()
		{
			_invoiceService = new InvoiceService(_mockInvoiceRepository, _mockPaymentService);
		}

		[Test]
		public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPayment( )
		{
			var payment = new Payment();
			
			var ex = Assert.Throws<InvalidOperationException>(() => _invoiceService.ProcessPayment(payment));
			Assert.That(ex.Message, Is.EqualTo("There is no invoice matching this payment"));
		}
		
		[Test]
		public void ProcessPayment_Should_ThrowException_When_AmountIsZero_And_HasPayments()
		{
			var invoice = new Invoice
			{
				Amount = 0,
				Payments = new List<Payment> { new Payment { Amount = 10 } }
			};

			var payment = new Payment { InvoiceId = Guid.NewGuid() };
			_mockInvoiceRepository.GetById(payment.InvoiceId).Returns(invoice);

			var ex = Assert.Throws<InvalidOperationException>(() => _invoiceService.ProcessPayment(payment));
			Assert.That(ex.Message, Is.EqualTo("The invoice is in an invalid state, it has an amount of 0 and it has payments."));
		}
		
		[Test]
		public void ProcessPayment_Should_ThrowException_When_AmountPaidIsNonZero_ButNoPayments()
		{
			var invoice = new Invoice
			{
				Amount = 100,
				AmountPaid = 50,
				Payments = new List<Payment>() // no payments
			};

			var payment = new Payment { InvoiceId = Guid.NewGuid() };
			_mockInvoiceRepository.GetById(payment.InvoiceId).Returns(invoice);

			var ex = Assert.Throws<InvalidOperationException>(() => _invoiceService.ProcessPayment(payment));
			Assert.That(ex.Message, Is.EqualTo("The invoice is in an invalid state, it has an amount paid, but no payments."));
		}
		
		[Test]
		public void ProcessPayment_Should_ThrowException_When_InvoiceType_IsInvalid()
		{
			var invoice = new Invoice
			{
				Amount = 100,
				Type = (InvoiceType)99, // invalid
				Payments = new List<Payment>()
			};

			var payment = new Payment { InvoiceId = Guid.NewGuid() };
			_mockInvoiceRepository.GetById(payment.InvoiceId).Returns(invoice);

			var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _invoiceService.ProcessPayment(payment));
			Assert.That(ex.ParamName, Is.EqualTo("Type"));
			Assert.That(ex.Message, Does.Contain("Unsupported invoice type"));
		}
		
		[Test]
		public void ProcessPayment_Should_Process_And_SaveInvoice_When_Invoice_Valid()
		{
			var invoiceId = Guid.NewGuid();
			var invoice = new Invoice
			{
				Id = invoiceId,
				Amount = 100,
				Type = InvoiceType.Standard,
				Payments = new List<Payment>()
			};

			var payment = new Payment { InvoiceId = invoiceId, Amount = 100 };
			const string expectedResponse = Constants.FinalInitialPaymentMessage;
			var expectedPaymentResult = new PaymentResult(invoice, expectedResponse);
			
			_mockPaymentService.ProcessPayment(invoice, payment).Returns(expectedPaymentResult);

			_mockInvoiceRepository.GetById(payment.InvoiceId).Returns(invoice);

			var result = _invoiceService.ProcessPayment(payment);

			_mockInvoiceRepository.Received(1).Save(invoice);
			Assert.That(result, Is.EqualTo(expectedResponse));
		}
	}
}