using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Tests
{


	[TestFixture]
	public class InvoicePaymentProcessorTests
	{

		private InvoiceService _invoiceService;
		private FakeInvoiceRepository _fakeRepo;

		[SetUp]
		public void Setup()
		{
			_fakeRepo = new FakeInvoiceRepository();
			_invoiceService = new InvoiceService(_fakeRepo);
		}

		[Test]
		public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference( )
		{
			var payment = new Payment { Reference = "InvalidRef", Amount = 10 };

			var ex = Assert.Throws<InvalidOperationException>(() => _invoiceService.ProcessPayment(payment));

			Assert.AreEqual( "There is no invoice matching this payment", ex.Message );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded( )
		{

			var invoice = new Invoice
			{
				Reference = "INV-001",
				Amount = 0,
				AmountPaid = 0,
				Payments = null
			};

			_fakeRepo.Add( invoice );

			var payment = new Payment
			{
				Reference = "INV-001",
				Amount = 10
			};

			var result = _invoiceService.ProcessPayment( payment );

			Assert.AreEqual( "no payment needed", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid( )
		{

			var invoice = new Invoice
			{
				Reference = "INV-002",
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

			_fakeRepo.Add( invoice );

			var payment = new Payment
			{
				Reference = "INV-002",
				Amount = 10,
			};

			var result = _invoiceService.ProcessPayment( payment );

			Assert.AreEqual( "invoice was already fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue( )
		{
			var invoice = new Invoice
			{
				Reference = "INV-003",
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
			_fakeRepo.Add( invoice );

			var payment = new Payment
			{
				Reference = "INV-003",
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
				Reference =	"INV-004",
				Amount = 5,
				AmountPaid = 0,
				Payments = new List<Payment>( )
			};
			_fakeRepo.Add( invoice );

			var payment = new Payment( )
			{
				Reference = "INV-004",
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
				Reference = "INV-005",
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
			_fakeRepo.Add( invoice );

			var payment = new Payment( )
			{
				Reference = "INV-005",
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
				Reference = "INV-006",
				Amount = 10,
				AmountPaid = 0,
				Payments = new List<Payment> { new Payment { Amount = 10 } }
			};
			_fakeRepo.Add(invoice);

			var payment = new Payment
			{
				Reference = "INV-006",
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
				Reference = "INV-007",
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
			_fakeRepo.Add( invoice );


			var payment = new Payment( )
			{
				Reference = "INV-007",
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
				Reference = "INV-008",
				Amount = 10,
				AmountPaid = 0,
				Payments = new List<Payment>( )
			};
			_fakeRepo.Add(invoice);

			var payment = new Payment( )
			{
				Reference = "INV-008",
				Amount = 1
			};

			var result = _invoiceService.ProcessPayment( payment );

			Assert.AreEqual( "invoice is now partially paid", result );
		}
	}


	/// <summary>
	///  Fake Invoice Repository for testing without using a real database
	/// </summary>
	public class FakeInvoiceRepository : IInvoiceRepository
	{
		private readonly Dictionary<string, Invoice> _invoices = new Dictionary<string, Invoice>();
		public Invoice GetInvoice(string reference)
		{
			return _invoices.TryGetValue(reference, out var invoice) ? invoice : null;
		}

		public void SaveInvoice(Invoice invoice)
		{
			_invoices[invoice.Reference] = invoice;
		}

		public void Add(Invoice invoice)
		{
			_invoices[invoice.Reference] = invoice;
		}

	}
}