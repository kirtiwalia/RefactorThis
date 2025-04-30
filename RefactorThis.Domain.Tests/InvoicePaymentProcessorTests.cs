using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Domain.Services;
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
            var service = new InvoiceService(repo);
            var payment = new Payment { Reference = "unknown" };

            var ex = Assert.Throws<InvalidOperationException>(() => service.ProcessPayment(payment));
            Assert.AreEqual("There is no invoice matching this payment", ex.Message);

		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded( )
		{
            var repo = new InvoiceRepository();
            var invoice = new Invoice
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null,
                Type = InvoiceType.Standard
            };

            repo.Add( invoice );

            var service = new InvoiceService(repo);
            var result = service.ProcessPayment(new Payment());


           Assert.AreEqual("no payment needed", result);
        }

        [Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid( )
		{
			var repo = new InvoiceRepository( );

            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment> { new Payment { Amount = 10 } },
                Type = InvoiceType.Standard
            };
            repo.Add( invoice );

            var service = new InvoiceService(repo);
            var result = service.ProcessPayment(new Payment());

            Assert.AreEqual( "invoice was already fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue( )
		{
			var repo = new InvoiceRepository( );
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment> { new Payment { Amount = 5 } },
                Type = InvoiceType.Standard
            };
            repo.Add( invoice );

            var service = new InvoiceService(repo);
            var result = service.ProcessPayment(new Payment { Amount = 6 });

			Assert.AreEqual( "the payment is greater than the partial amount remaining", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount( )
		{
			var repo = new InvoiceRepository( );
			var invoice = new Invoice
			{
				Amount = 5,
				AmountPaid = 0,
				Payments = new List<Payment>( ),
                Type = InvoiceType.Standard
            };
			repo.Add( invoice );

            var service = new InvoiceService(repo);
            var result = service.ProcessPayment(new Payment { Amount = 6 });

			Assert.AreEqual( "the payment is greater than the invoice amount", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue( )
		{
			var repo = new InvoiceRepository( );
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
			repo.Add( invoice );

            var service = new InvoiceService(repo);
            var result = service.ProcessPayment(new Payment { Amount = 5 });

            Assert.AreEqual( "final partial payment received, invoice is now fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount( )
		{
			var repo = new InvoiceRepository( );
			var invoice = new Invoice
			{
				Amount = 10,
				AmountPaid = 0,
                Payments = new List<Payment>(),
                Type = InvoiceType.Standard
            };
			repo.Add( invoice );

            var service = new InvoiceService(repo);
            var result = service.ProcessPayment(new Payment { Amount = 10 });

            Assert.AreEqual("invoice is now fully paid", result);
        }

        [Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue( )
		{
			var repo = new InvoiceRepository( );
			var invoice = new Invoice
			{
				Amount = 10,
				AmountPaid = 5,
                Payments = new List<Payment> { new Payment { Amount = 5 } },
                Type = InvoiceType.Standard
            };
			repo.Add( invoice );

            var service = new InvoiceService(repo);
            var result = service.ProcessPayment(new Payment { Amount = 2 });

            Assert.AreEqual( "another partial payment received, still not fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount( )
		{
			var repo = new InvoiceRepository( );
			var invoice = new Invoice
			{
				Amount = 10,
				AmountPaid = 0,
                Payments = new List<Payment>(),
                Type = InvoiceType.Standard
            };
			repo.Add( invoice );

            var service = new InvoiceService(repo);
            var result = service.ProcessPayment(new Payment { Amount = 2 });


            Assert.AreEqual( "invoice is now partially paid", result );
		}

        [Test]
        public void ProcessPayment_Should_CalculateTax_When_CommercialInvoice()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice
            {
                Amount = 100,
                AmountPaid = 0,
                Payments = new List<Payment>(),
                Type = InvoiceType.Commercial
            };
            repo.Add(invoice);

            var service = new InvoiceService(repo);
            var result = service.ProcessPayment(new Payment { Amount = 100 });

            Assert.AreEqual("invoice is now fully paid", result);
            Assert.AreEqual(14, invoice.TaxAmount); // 14% of 100
        }
    }
}