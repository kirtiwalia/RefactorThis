using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Domain;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        private IRepository _repo;
        private IInvoiceService _invoiceService;

        [SetUp]
        public void SetUp()
        {
            _repo = new InvoiceRepository();
            _invoiceService = new InvoiceService(_repo);
        }

        [Test]
        public void Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
        {
            var payment = new Payment { Reference = "NOT_EXISTING" };

            var ex = Assert.Throws<InvalidOperationException>(() =>
                _invoiceService.ProcessPayment(payment)
            );

            Assert.That(ex.Message, Is.EqualTo("There is no invoice matching this payment"));
        }

        [Test]
        public void Should_ReturnNoPaymentNeeded_When_InvoiceAmountIsZeroAndNoPayments()
        {
            AddInvoice(amount: 0, amountPaid: 0, payments: null);

            var result = _invoiceService.ProcessPayment(new Payment { Reference = "" });
            Assert.That(result, Is.EqualTo("no payment needed"));
        }

        [Test]
        public void Should_ReturnAlreadyFullyPaid_When_InvoiceIsAlreadyPaid()
        {
            AddInvoice(amount: 10, amountPaid: 10, payments: new List<Payment> { new Payment { Amount = 10 } });

            var result = _invoiceService.ProcessPayment(new Payment { Reference = "" });
            Assert.That(result, Is.EqualTo("invoice was already fully paid"));
        }

        [Test]
        public void Should_ReturnPaymentExceedsPartialAmount_When_Overpaying()
        {
            AddInvoice(amount: 10, amountPaid: 5, payments: new List<Payment> { new Payment { Amount = 5 } });

            var result = _invoiceService.ProcessPayment(new Payment { Amount = 6, Reference = "" });
            Assert.That(result, Is.EqualTo("the payment is greater than the partial amount remaining"));
        }

        [Test]
        public void Should_ReturnPaymentExceedsInvoiceAmount_When_NoPreviousPayments()
        {
            AddInvoice(amount: 5, amountPaid: 0, payments: new List<Payment>());

            var result = _invoiceService.ProcessPayment(new Payment { Amount = 6, Reference = "" });
            Assert.That(result, Is.EqualTo("the payment is greater than the invoice amount"));
        }

        [Test]
        public void Should_ReturnFullyPaidMessage_When_FinalPartialPaymentReceived()
        {
            AddInvoice(amount: 10, amountPaid: 5, payments: new List<Payment> { new Payment { Amount = 5 } });

            var result = _invoiceService.ProcessPayment(new Payment { Amount = 5, Reference = "" });
            Assert.That(result, Is.EqualTo("final partial payment received, invoice is now fully paid"));
        }

        [Test]
        public void Should_ReturnAlreadyFullyPaid_When_NoPartialPaymentExistsAndAmountMatchesInvoice()
        {
            AddInvoice(amount: 10, amountPaid: 0, payments: new List<Payment> { new Payment { Amount = 10 } });

            var result = _invoiceService.ProcessPayment(new Payment { Amount = 10, Reference = "" });
            Assert.That(result, Is.EqualTo("invoice was already fully paid"));
        }

        [Test]
        public void Should_ReturnPartiallyPaid_When_AnotherPartialPaymentIsMade()
        {
            AddInvoice(amount: 10, amountPaid: 5, payments: new List<Payment> { new Payment { Amount = 5 } });

            var result = _invoiceService.ProcessPayment(new Payment { Amount = 1, Reference = "" });
            Assert.That(result, Is.EqualTo("another partial payment received, still not fully paid"));
        }

        [Test]
        public void Should_ReturnPartiallyPaid_When_FirstPartialPaymentIsMade()
        {
            AddInvoice(amount: 10, amountPaid: 0, payments: new List<Payment>());

            var result = _invoiceService.ProcessPayment(new Payment { Amount = 1, Reference = "" });
            Assert.That(result, Is.EqualTo("invoice is now partially paid"));
        }

        private void AddInvoice(decimal amount, decimal amountPaid, List<Payment> payments)
        {
            var invoice = new Invoice(_repo)
            {
                Amount = amount,
                AmountPaid = amountPaid,
                Payments = payments ?? new List<Payment>(),
                Type = InvoiceType.Standard
            };

            _repo.Add(invoice);
        }
    }
}
