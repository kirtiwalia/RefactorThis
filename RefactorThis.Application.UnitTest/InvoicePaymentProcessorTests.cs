using NUnit.Framework;
using RefactorThis.Application.Common;
using RefactorThis.Application.Interfaces;
using RefactorThis.Domain;
using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Enums;
using RefactorThis.Domain.PaymentStrategies;
using RefactorThis.UnitTests;
using System;
using System.Collections.Generic;

namespace RefactorThis.Application.UnitTests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        private IInvoiceRepository _repo;
        private IInvoicePaymentStrategyFactory _strategyFactory;
        private InvoiceService _paymentProcessor;


        [SetUp]
        public void Setup()
        {
            _repo = new FakeInvoiceRepository();
            _strategyFactory = new InvoicePaymentStrategyFactory();
            _paymentProcessor = new InvoiceService(_repo, _strategyFactory);
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
        {
            var payment = new Payment();
            var ex = Assert.Throws<InvalidOperationException>(() => _paymentProcessor.ProcessPayment(payment));
            Assert.AreEqual("There is no invoice matching this payment", ex.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            var invoice = CreateInvoice(0, 0, 0);
            var payment = new Payment();

            var result = ProcessInvoicePayment(invoice, payment);

            Assert.AreEqual(PaymentResultMessages.ToMessage(PaymentResultCode.NoPaymentNeeded, invoice), result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {

            var invoice = CreateInvoice(10, 10, 10);
            var payment = new Payment();

            var result = ProcessInvoicePayment(invoice, payment);
            Assert.AreEqual(PaymentResultMessages.ToMessage(PaymentResultCode.InvoiceAlreadyPaid, invoice), result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            var invoice = CreateInvoice(10, 5, 5);
            var payment = new Payment() { Amount = 6 };

            var result = ProcessInvoicePayment(invoice, payment);
            Assert.AreEqual(PaymentResultMessages.ToMessage(PaymentResultCode.PaymentGreaterThanRemaining, invoice), result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            var invoice = CreateInvoice(5, 0, 0);
            var payment = new Payment() { Amount = 6 };

            var result = ProcessInvoicePayment(invoice, payment);

            Assert.AreEqual(PaymentResultMessages.ToMessage(PaymentResultCode.PaymentGreaterThanInvoiceAmount, invoice), result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            var invoice = CreateInvoice(10, 5, 5);
            var payment = new Payment() { Amount = 5 };

            var result = ProcessInvoicePayment(invoice, payment);

            Assert.AreEqual(PaymentResultMessages.ToMessage(PaymentResultCode.FullyPaid, invoice), result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        { 
            var invoice = CreateInvoice(10, 0, 10);
            var payment = new Payment() { Amount = 10 };

            var result = ProcessInvoicePayment(invoice, payment);

            Assert.AreEqual(PaymentResultMessages.ToMessage(PaymentResultCode.InvoiceAlreadyPaid, invoice), result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            var invoice = CreateInvoice(10, 5, 5);
            var payment = new Payment() { Amount = 1 };

            var result = ProcessInvoicePayment(invoice, payment);
            Assert.AreEqual(PaymentResultMessages.ToMessage(PaymentResultCode.PartiallyPaid, invoice), result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_WhenFirstPartialPayment()
        {
            var invoice = CreateInvoice(10, 0, 0);
            var payment = new Payment() { Amount = 1 };

            var result = ProcessInvoicePayment(invoice, payment);
            Assert.AreEqual(PaymentResultMessages.ToMessage(PaymentResultCode.PartiallyPaid, invoice), result);
        }

        private Invoice CreateInvoice(decimal amount, decimal amountPaid = 0, params decimal[] payments)
        {
            var invoice = new Invoice
            {
                Amount = amount,
                AmountPaid = amountPaid,
                Payments = new List<Payment>()
            };

            foreach (var p in payments)
            {
                invoice.Payments.Add(new Payment { Amount = p });
            }

            return invoice;
        }

        private string ProcessInvoicePayment(Invoice invoice, Payment payment)
        {
            _repo.Add(invoice);
            return _paymentProcessor.ProcessPayment(payment);
        }
    }
}