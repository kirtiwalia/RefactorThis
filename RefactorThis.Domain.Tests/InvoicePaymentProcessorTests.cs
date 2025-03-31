using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Tests
{
	[TestFixture]
	public class InvoicePaymentProcessorTests
	{
		[Test]
		public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference( )
		{
			var repo = new InvoiceRepository( );

			Invoice invoice = null;
			var paymentProcessor = new InvoiceService( repo );

			var payment = new Payment( );

			var result = paymentProcessor.ProcessPayment( payment );
			

			Assert.AreEqual(State.NoInvoiceFound, result.PaymentState);
			Assert.False(result.Success);
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded( )
		{
			var repo = new InvoiceRepository( );

			var invoice = new Invoice()
			{
				Amount = 0,
				AmountPaid = 0,
			};

            repo.SaveInvoice( invoice );

			var paymentProcessor = new InvoiceService( repo );

			var payment = new Payment() { InvoiceId = invoice.Id };

			var result = paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual(State.NoPaymentRequred, result.PaymentState);
            Assert.False(result.Success);
        }

        [Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid( )
		{
			var repo = new InvoiceRepository( );

			var invoice = new Invoice( )
			{
				Amount = 10,
				AmountPaid = 10
			};
            invoice.Payments.Add(new Payment { Amount = 10, InvoiceId = invoice.Id });

            repo.SaveInvoice( invoice );

			var paymentProcessor = new InvoiceService( repo );

			var payment = new Payment() { InvoiceId = invoice.Id };

			var result = paymentProcessor.ProcessPayment( payment );

            Assert.AreEqual(State.NoPaymentRequred, result.PaymentState);
            Assert.False(result.Success);
        }

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue( )
		{
			var repo = new InvoiceRepository( );
			var invoice = new Invoice( )
			{
				Amount = 10,
				AmountPaid = 5,
			};
			invoice.Payments.Add(new Payment() { Amount = 5, InvoiceId = invoice.Id });
			repo.SaveInvoice( invoice );

			var paymentProcessor = new InvoiceService( repo );

			var payment = new Payment( )
			{
				Amount = 6,
                InvoiceId = invoice.Id
            };

			var result = paymentProcessor.ProcessPayment( payment );

            Assert.AreEqual(State.GreaterThanRemainder, result.PaymentState);
            Assert.False(result.Success);
        }

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount( )
		{
			var repo = new InvoiceRepository( );
			var invoice = new Invoice( )
			{
				Amount = 5,
				AmountPaid = 0
			};
			repo.SaveInvoice( invoice );

			var paymentProcessor = new InvoiceService( repo );

			var payment = new Payment( )
			{
				Amount = 6,
				InvoiceId = invoice.Id
			};

			var result = paymentProcessor.ProcessPayment( payment );

            Assert.AreEqual(State.GreaterThanRemainder, result.PaymentState);
            Assert.False(result.Success);
        }

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue( )
		{
			var repo = new InvoiceRepository( );
			var invoice = new Invoice( )
			{
				Amount = 10,
				AmountPaid = 5
			};
            invoice.Payments.Add(new Payment() { Amount = 5 });

            repo.SaveInvoice( invoice );

			var paymentProcessor = new InvoiceService( repo );

			var payment = new Payment( )
			{
				Amount = 5,
                InvoiceId = invoice.Id
            };

			var result = paymentProcessor.ProcessPayment( payment );

            Assert.AreEqual(State.FullyPaid, result.PaymentState);
            Assert.True(result.Success);
        }

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount( )
		{
			var repo = new InvoiceRepository( );
			var invoice = new Invoice( )
			{
				Amount = 10,
				AmountPaid = 0
			};
            invoice.Payments.Add(new Payment() { Amount = 10, InvoiceId = invoice.Id});

            repo.SaveInvoice( invoice );

			var paymentProcessor = new InvoiceService( repo );

			var payment = new Payment( )
			{
				Amount = 10,
                InvoiceId = invoice.Id
            };

			var result = paymentProcessor.ProcessPayment( payment );

            Assert.AreEqual(State.NoPaymentRequred, result.PaymentState);
            Assert.False(result.Success);
        }

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue( )
		{
			var repo = new InvoiceRepository( );
			var invoice = new Invoice( )
			{
				Amount = 10,
				AmountPaid = 5
			};
            invoice.Payments.Add(new Payment() { Amount = 5 });

            repo.SaveInvoice( invoice );

			var paymentProcessor = new InvoiceService( repo );

			var payment = new Payment( )
			{
				Amount = 1,
                InvoiceId = invoice.Id
            };

			var result = paymentProcessor.ProcessPayment( payment );

            Assert.AreEqual(State.PartialPaid, result.PaymentState);
            Assert.True(result.Success);
        }

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount( )
		{
			var repo = new InvoiceRepository( );
			var invoice = new Invoice( )
			{
				Amount = 10,
				AmountPaid = 0
			};
			repo.SaveInvoice( invoice );

			var paymentProcessor = new InvoiceService( repo );

			var payment = new Payment( )
			{
				Amount = 1,
                InvoiceId = invoice.Id
            };

			var result = paymentProcessor.ProcessPayment( payment );

            Assert.AreEqual(State.PartialPaid, result.PaymentState);
            Assert.True(result.Success);
        }
	}
}