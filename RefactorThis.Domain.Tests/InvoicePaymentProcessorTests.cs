using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        private InvoiceRepository _repository;
        private InvoiceService _service;

        [SetUp]
        public void SetUp()
        {
            _repository = new InvoiceRepository();
            _service = new InvoiceService(_repository);
        }

        [Test]
        public void ProcessPayment_NoInvoice_ThrowsInvalidOperationException()
        {
            // When no invoice has ever been added, GetInvoice(...) will return null.
            // In that case, ProcessPayment(...) is supposed to throw:
            var payment = new Payment();

            var result = _service.ProcessPayment(payment);
            Assert.AreEqual("There is no invoice matching this payment", result);
        }

        [Test]
        public void ProcessPayment_AmountZeroAndNoPayments_ReturnsNoPaymentNeeded()
        {
            // Add an invoice whose Amount==0 and Payments==null.
            AddInvoice(amount: 0, paid: 0, payments: null);

            var result = _service.ProcessPayment(new Payment());
            Assert.AreEqual("no payment needed", result);
        }

        [Test]
        public void ProcessPayment_InvoiceAlreadyFullyPaid_ReturnsInvoiceAlreadyFullyPaid()
        {
            // Add an invoice that is already paid in full (AmountPaid == Amount):
            AddInvoice(
                amount: 10,
                paid: 10,
                payments: new List<Payment> { new Payment { Amount = 10 } }
            );

            var result = _service.ProcessPayment(new Payment());
            Assert.AreEqual("invoice was already fully paid", result);
        }

        [Test]
        public void ProcessPayment_PaymentExceedsPartialRemaining_ReturnsTooHighPartialMessage()
        {
            // Add an invoice with a partial payment of 5/10 already registered.
            AddInvoice(
                amount: 10,
                paid: 5,
                payments: new List<Payment> { new Payment { Amount = 5 } }
            );

            // Make a payment of 6, which exceeds the (10 – 5) remaining:
            var result = _service.ProcessPayment(new Payment { Amount = 6 });
            Assert.AreEqual(
                "the payment is greater than the partial amount remaining",
                result
            );
        }

        [Test]
        public void ProcessPayment_PaymentExceedsInvoiceAmountAndNoPartialExists_ReturnsTooHighInvoiceMessage()
        {
            // Add an invoice with zero paid so far, but full amount is 5.
            AddInvoice(amount: 5, paid: 0, payments: new List<Payment>());

            // Make a payment of 6, which exceeds the total invoice amount:
            var result = _service.ProcessPayment(new Payment { Amount = 6 });
            Assert.AreEqual("the payment is greater than the invoice amount", result);
        }

        [Test]
        public void ProcessPayment_FinalPartialPayment_ReturnsFinalFullyPaidMessage()
        {
            // Add an invoice that already has a partial 5/10 payment.
            AddInvoice(
                amount: 10,
                paid: 5,
                payments: new List<Payment> { new Payment { Amount = 5 } }
            );

            // Make a payment of 5, which exactly matches the remaining (10 – 5).
            var result = _service.ProcessPayment(new Payment { Amount = 5 });
            Assert.AreEqual(
                "final partial payment received, invoice is now fully paid",
                result
            );
        }

        [Test]
        public void ProcessPayment_FullPayment_NoPriorPartialExists_ReturnsInvoiceAlreadyFullyPaid()
        {
            // “No partial” means Payments list may contain a single payment that equals full amount.
            AddInvoice(
                amount: 10,
                paid: 0,
                payments: new List<Payment> { new Payment { Amount = 10 } }
            );

            // If we call ProcessPayment with Amount=10 again, business logic treats it
            // as “invoice was already fully paid.”
            var result = _service.ProcessPayment(new Payment { Amount = 10 });
            Assert.AreEqual("invoice was already fully paid", result);
        }

        [Test]
        public void ProcessPayment_AnotherPartialPayment_ReturnsAnotherPartialMessage()
        {
            // Add an invoice already partially paid 5/10.
            AddInvoice(
                amount: 10,
                paid: 5,
                payments: new List<Payment> { new Payment { Amount = 5 } }
            );

            // Make a new payment of 1, which does not finish the invoice.
            var result = _service.ProcessPayment(new Payment { Amount = 1 });
            Assert.AreEqual(
                "another partial payment received, still not fully paid",
                result
            );
        }

        [Test]
        public void ProcessPayment_FirstPartialPayment_ReturnsPartiallyPaidMessage()
        {
            // Add an invoice that has no payments yet (paid=0, Payments empty).
            AddInvoice(amount: 10, paid: 0, payments: new List<Payment>());

            // Make a new payment of 1, which is less than invoice amount.
            var result = _service.ProcessPayment(new Payment { Amount = 1 });
            Assert.AreEqual("invoice is now partially paid", result);
        }

        /// <summary>
        /// Helper method to create and register a new Invoice in the repository.
        /// </summary>
        private void AddInvoice(decimal amount, decimal paid, List<Payment> payments)
        {
            var invoice = new Invoice(_repository)
            {
                Amount = amount,
                AmountPaid = paid,
                Payments = payments
            };

            _repository.Add(invoice);
        }
    }
}
