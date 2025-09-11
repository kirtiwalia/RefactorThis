using Moq;
using NUnit.Framework;
using RefactorThis.Domain.Interfaces;
using RefactorThis.Domain.Models;
using RefactorThis.Domain.Services;
using System;
using System.Collections.Generic;

namespace RefactorThis.Domain.Tests
{
	[TestFixture]
	public class InvoicePaymentProcessorTests
	{
        private Mock<IInvoiceRepository> _mockRepository;
        private InvoiceService _invoiceService;
        
		[SetUp]
		public void Setup()
		{
            _mockRepository = new Mock<IInvoiceRepository>();
            _invoiceService = new InvoiceService(_mockRepository.Object);
        }

		[Test]
		public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference( )
		{
			var paymentProcessor = _invoiceService;
			var payment = new Payment( );
			var failureMessage = "";

			try
			{
				var result = paymentProcessor.ProcessPayment(payment);
			}
			catch ( InvalidOperationException e )
			{
				failureMessage = e.Message;
			}

			Assert.AreEqual( "There is no invoice matching this payment", failureMessage );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded( )
		{
			var invoice = new Invoice
			{
				Amount = 0,
				AmountPaid = 0,
				Payments = null
			};

            _mockRepository.Setup(x => x.GetInvoice(It.IsAny<string>())).Returns(invoice);

			var payment = new Payment();

			var result = _invoiceService.ProcessPayment( payment );

			Assert.AreEqual( "no payment needed", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid( )
		{
			var invoice = new Invoice
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

            _mockRepository.Setup(x => x.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment( );

			var result = _invoiceService.ProcessPayment( payment );

			Assert.AreEqual( "invoice was already fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue( )
		{
			var invoice = new Invoice
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
            _mockRepository.Setup(x => x.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment( )
			{
				Amount = 6
			};

			var result = _invoiceService.ProcessPayment( payment );

			Assert.AreEqual( "the payment is greater than the partial amount remaining", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount( )
		{
			var invoice = new Invoice
			{
				Amount = 5,
				AmountPaid = 0,
				Payments = new List<Payment>( )
			};
            _mockRepository.Setup(x => x.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment( )
			{
				Amount = 6
			};

			var result = _invoiceService.ProcessPayment( payment );

			Assert.AreEqual( "the payment is greater than the invoice amount", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue( )
		{
			var invoice = new Invoice
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
            _mockRepository.Setup(x => x.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment( )
			{
				Amount = 5
			};

			var result = _invoiceService.ProcessPayment( payment );

			Assert.AreEqual( "final partial payment received, invoice is now fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount( )
		{
			var invoice = new Invoice
			{
				Amount = 10,
				AmountPaid = 0,
				Payments = new List<Payment>( ) { new Payment( ) { Amount = 10 } }
			};
			_mockRepository.Setup(x => x.GetInvoice(It.IsAny<string>())).Returns( invoice );

			var payment = new Payment( )
			{
				Amount = 10
			};

			var result = _invoiceService.ProcessPayment( payment );

			Assert.AreEqual( "invoice was already fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue( )
		{
			var invoice = new Invoice
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
            _mockRepository.Setup(x => x.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment( )
			{
				Amount = 1
			};

			var result = _invoiceService.ProcessPayment( payment );

			Assert.AreEqual( "another partial payment received, still not fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount( )
		{
			var invoice = new Invoice
			{
				Amount = 10,
				AmountPaid = 0,
				Payments = new List<Payment>( )
			};
            _mockRepository.Setup(x => x.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var payment = new Payment( )
			{
				Amount = 1
			};

			var result = _invoiceService.ProcessPayment( payment );

			Assert.AreEqual( "invoice is now partially paid", result );
		}
	}
}