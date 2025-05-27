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
		public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
		{
			// Arrange
			var repo = new InvoiceRepository();
			var service = new InvoiceService(repo);

			// Create a payment with a reference that has no matching invoice
			var payment = new Payment
			{
				Reference = "INV-NOT-FOUND",
				Amount = 5
			};

			// Act + Assert
			var ex = Assert.Throws<InvalidOperationException>(() => service.ProcessPayment(payment));

			// Check exception message for clarity
			Assert.AreEqual(InvoiceMessages.NoInvoiceMatchingPayment, ex.Message);
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
		{
			// Arrange
			var repo = new InvoiceRepository();

			// Create an invoice that has nothing owing.
			var invoice = new Invoice(InvoiceType.Standard) // any type is fine here
			{
				Amount = 0, // zero-value invoice
			};

			// Seed the repository with a reference the payment will use.
			const string reference = "INV-1";
			repo.Add(reference, invoice);

			// Act
			var service = new InvoiceService(repo);

			var payment = new Payment // any amount / reference is OK
			{
				Reference = reference,
				Amount = 0
			};

			var result = service.ProcessPayment(payment);

			// Assert
			Assert.AreEqual(InvoiceMessages.NoPaymentNeeded, result);
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
		{
			// Arrange
			var repo = new InvoiceRepository();

			// Create an invoice with full payment already applied.
			var invoice = new Invoice(InvoiceType.Standard)
			{
				Amount = 10,
			};

			// Simulate full payment
			invoice.ApplyPayment(new Payment
			{
				Amount = 10,
				Reference = "INV-2"
			});

			// Seed the invoice using its reference
			const string reference = "INV-2";
			repo.Add(reference, invoice);

			// Act
			var service = new InvoiceService(repo);

			// Create a new payment attempt for the same invoice
			var payment = new Payment
			{
				Reference = reference,
				Amount = 5
			};

			var result = service.ProcessPayment(payment);

			// Assert
			Assert.AreEqual(InvoiceMessages.InvoiceAlreadyFullyPaid, result);
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
		{
			// Arrange
			var repo = new InvoiceRepository();

			// Create an invoice with a partial payment already applied
			var invoice = new Invoice(InvoiceType.Standard)
			{
				Amount = 10,
			};

			// Apply an initial payment of 5
			invoice.ApplyPayment(new Payment
			{
				Reference = "INV-3",
				Amount = 5
			});

			// Add the invoice to the repository using its reference
			const string reference = "INV-3";
			repo.Add(reference, invoice);

			// Act
			var service = new InvoiceService(repo);

			// Create a new payment that exceeds the remaining balance (10 - 5 = 5 remaining, but trying to pay 6)
			var payment = new Payment
			{
				Reference = reference,
				Amount = 6
			};

			var result = service.ProcessPayment(payment);

			// Assert
			Assert.AreEqual(InvoiceMessages.PaymentExceedsRemainingBalance, result);
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
		{
			// Arrange
			var repo = new InvoiceRepository();

			// Create an invoice with no payments yet, and a total amount of 5
			var invoice = new Invoice(InvoiceType.Standard)
			{
				Amount = 5,
			};

			// Add the invoice to the repository using a known reference
			const string reference = "INV-4";
			repo.Add(reference, invoice);

			// Act
			var service = new InvoiceService(repo);

			// Attempt to pay more than the invoice amount (invoice = 5, payment = 6)
			var payment = new Payment
			{
				Reference = reference,
				Amount = 6
			};

			var result = service.ProcessPayment(payment);

			// Assert
			Assert.AreEqual(InvoiceMessages.PaymentExceedsInvoiceAmount, result);
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
		{
			// Arrange
			var repo = new InvoiceRepository();

			// Create an invoice with partial payment already applied (5 out of 10)
			var invoice = new Invoice(InvoiceType.Standard)
			{
				Amount = 10,
			};

			// Apply a partial payment of 5
			invoice.ApplyPayment(new Payment
			{
				Reference = "INV-5",
				Amount = 5
			});

			// Add the invoice to the repository using a matching reference
			const string reference = "INV-5";
			repo.Add(reference, invoice);

			// Act
			var service = new InvoiceService(repo);

			// Apply a second payment of 5, which completes the invoice
			var payment = new Payment
			{
				Reference = reference,
				Amount = 5
			};

			var result = service.ProcessPayment(payment);

			// Assert
			Assert.AreEqual(InvoiceMessages.FinalPartialPaymentComplete, result);
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
		{
			var repo = new InvoiceRepository();

			// Create an invoice and apply a full single payment immediately
			var invoice = new Invoice(InvoiceType.Standard)
			{
				Amount = 10,
			};

			// Simulate full payment
			invoice.ApplyPayment(new Payment
			{
				Reference = "INV-6",
				Amount = 10
			});

			// Add the invoice to the repository using a matching reference
			const string reference = "INV-6";
			repo.Add(reference, invoice);

			// Act
			var service = new InvoiceService(repo);

			// Attempt to apply another full payment (should be rejected as already fully paid)
			var payment = new Payment
			{
				Reference = reference,
				Amount = 10
			};

			var result = service.ProcessPayment(payment);

			// Assert
			Assert.AreEqual(InvoiceMessages.InvoiceAlreadyFullyPaid, result);
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
		{
			// Arrange
			var repo = new InvoiceRepository();

			// Create an invoice with a partial payment of 5 already applied
			var invoice = new Invoice(InvoiceType.Standard)
			{
				Amount = 10,
			};

			invoice.ApplyPayment(new Payment
			{
				Reference = "INV-7",
				Amount = 5
			});

			// Add invoice to repository using matching reference
			const string reference = "INV-7";
			repo.Add(reference, invoice);

			// Act
			var service = new InvoiceService(repo);

			// Apply a second payment of 1 (still leaves balance unpaid)
			var payment = new Payment
			{
				Reference = reference,
				Amount = 1
			};

			var result = service.ProcessPayment(payment);

			// Assert
			Assert.AreEqual(InvoiceMessages.AdditionalPartialPayment, result);
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
		{
			// Arrange
			var repo = new InvoiceRepository();

			// Create a new invoice with no payments yet and a total of 10 due
			var invoice = new Invoice(InvoiceType.Standard)
			{
				Amount = 10,
			};

			// Add the invoice to the repository using a known reference
			const string reference = "INV-8";
			repo.Add(reference, invoice);

			// Act
			var service = new InvoiceService(repo);

			// Apply a first payment of 1 (not enough to fully pay the invoice)
			var payment = new Payment
			{
				Reference = reference,
				Amount = 1
			};

			var result = service.ProcessPayment(payment);

			// Assert
			Assert.AreEqual(InvoiceMessages.InitialPartialPayment, result);
		}
	}
}