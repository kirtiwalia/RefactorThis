using NUnit.Framework;
using System;
using System.Collections.Generic;
using RefactorThis.Domain.Services.Payments;
using RefactorThis.Persistence.Models;

namespace RefactorThis.Domain.Tests.InvoicePaymentProcessorTests
{

    [TestFixture]
    public class PaymentServiceTests
    {
        private PaymentService _paymentService;
        private Invoice _invoice;
        private Payment _payment;

        [SetUp]
        public void SetUp()
        {
            _paymentService = new PaymentService();
            _invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                Amount = 100,
                AmountPaid = 0,
                TaxAmount = 0,
                Type = InvoiceType.Standard,
                Payments = new List<Payment>()
            };

            _payment = new Payment
            {
                InvoiceId = _invoice.Id,
                Amount = 50
            };
        }

        [Test]
        public void ProcessPayment_ShouldReturnNoPaymentRequired_WhenInvoiceAmountIsZeroAndNoPayments()
        {
            _invoice.Amount = 0;
            _invoice.Payments = new List<Payment>();

            var result = _paymentService.ProcessPayment(_invoice, _payment);

            AssertAmountPaidMatchesExpected(_invoice, 0);
            AssertTaxAmountMatchesExpected(_invoice, 0);
            Assert.That(result.ResponseMessage, Is.EqualTo(Constants.NoPaymentRequiredMessage));
        }

        [Test]
        public void ProcessPayment_ShouldReturnAlreadyFullyPaid_WhenInvoiceIsAlreadyFullyPaid()
        {
            _invoice.AmountPaid = 100;
            _invoice.TaxAmount = 14;
            _invoice.Payments.Add(new Payment { Amount = 100 });

            var result = _paymentService.ProcessPayment(_invoice, _payment);

            AssertAmountPaidMatchesExpected(_invoice, 100);
            AssertTaxAmountMatchesExpected(_invoice, 100 * Constants.TaxRate);
            Assert.That(result.ResponseMessage, Is.EqualTo(Constants.InvoiceAlreadyFullyPaidMessage));
        }

        [Test]
        public void ProcessPayment_ShouldReturnInitialOverpaymentMessage_WhenOverpayingWithNoPriorPayments()
        {
            _payment.Amount = 150;

            var result = _paymentService.ProcessPayment(_invoice, _payment);

            AssertAmountPaidMatchesExpected(_invoice, 0);
            AssertTaxAmountMatchesExpected(_invoice, 0);
            Assert.That(result.ResponseMessage, Is.EqualTo(Constants.InitialOverpaymentMessage));
        }

        [Test]
        public void ProcessPayment_ShouldReturnPartialOverpaymentMessage_WhenOverpayingWithExistingPayments()
        {
            _invoice.AmountPaid = 50;
            _invoice.TaxAmount = 7;
            _invoice.Payments.Add(new Payment { Amount = 50 });
            _payment.Amount = 100;

            var result = _paymentService.ProcessPayment(_invoice, _payment);

            AssertAmountPaidMatchesExpected(_invoice, 50);
            AssertTaxAmountMatchesExpected(_invoice, 50 * Constants.TaxRate);
            Assert.That(result.ResponseMessage, Is.EqualTo(Constants.PartialOverpaymentMessage));
        }

        [Test]
        public void ProcessPayment_ShouldReturnPartialInitialPaymentMessage_WhenFirstPartialPayment()
        {
            _payment.Amount = 40;

            var result = _paymentService.ProcessPayment(_invoice, _payment);

            AssertAmountPaidMatchesExpected(_invoice, 40);
            AssertTaxAmountMatchesExpected(_invoice, 40 * Constants.TaxRate);
            Assert.That(result.ResponseMessage, Is.EqualTo(Constants.PartialInitialPaymentMessage));
        }

        [Test]
        public void ProcessPayment_ShouldReturnFinalInitialPaymentMessage_WhenFirstPaymentIsFullAmount()
        {
            _payment.Amount = 100;

            var result = _paymentService.ProcessPayment(_invoice, _payment);

            AssertAmountPaidMatchesExpected(_invoice, 100);
            AssertTaxAmountMatchesExpected(_invoice, 100 * Constants.TaxRate);
            Assert.That(result.ResponseMessage, Is.EqualTo(Constants.FinalInitialPaymentMessage));
        }

        [Test]
        public void ProcessPayment_ShouldReturnPartialPaymentMessage_WhenSubsequentPartialPayment()
        {
            _invoice.AmountPaid = 40;
            _invoice.TaxAmount = 5.6m;
            _invoice.Payments.Add(new Payment { Amount = 40 });

            _payment.Amount = 30;

            var result = _paymentService.ProcessPayment(_invoice, _payment);

            AssertAmountPaidMatchesExpected(_invoice, 70);
            AssertTaxAmountMatchesExpected(_invoice, 40 * Constants.TaxRate);
            Assert.That(result.ResponseMessage, Is.EqualTo(Constants.PartialPaymentMessage));
        }

        [Test]
        public void ProcessPayment_ShouldReturnFinalPartialPaymentMessage_WhenSubsequentFinalPayment()
        {
            _invoice.AmountPaid = 70;
            _invoice.TaxAmount = 9.8m;
            _invoice.Payments.Add(new Payment { Amount = 70 });

            _payment.Amount = 30;

            var result = _paymentService.ProcessPayment(_invoice, _payment);

            AssertAmountPaidMatchesExpected(_invoice, 100);
            AssertTaxAmountMatchesExpected(_invoice, 70 * Constants.TaxRate);
            Assert.That(result.ResponseMessage, Is.EqualTo(Constants.FinalPartialPaymentMessage));
        }

        [Test]
        public void ProcessPayment_ShouldApplyTax_ForFirstPaymentOnStandardInvoice()
        {
            _payment.Amount = 50;

            _paymentService.ProcessPayment(_invoice, _payment);

            AssertTaxAmountMatchesExpected(_invoice, 50 * Constants.TaxRate);
        }

        [Test]
        public void ProcessPayment_ShouldApplyTax_ForAnyPaymentOnCommercialInvoice()
        {
            _invoice.Type = InvoiceType.Commercial;
            _invoice.AmountPaid = 50;
            _invoice.Payments.Add(new Payment { Amount = 50 });

            _payment.Amount = 30;

            _paymentService.ProcessPayment(_invoice, _payment);

            AssertTaxAmountMatchesExpected(_invoice, 30 * Constants.TaxRate);
        }

        [Test]
        public void ProcessPayment_ShouldNotApplyTax_ForSubsequentStandardInvoicePayments()
        {
            _invoice.Type = InvoiceType.Standard;
            _invoice.AmountPaid = 50;
            _invoice.Payments.Add(new Payment { Amount = 50 });

            _payment.Amount = 30;

            _paymentService.ProcessPayment(_invoice, _payment);

            AssertTaxAmountMatchesExpected(_invoice, 0 * Constants.TaxRate);
        }

        private static void AssertAmountPaidMatchesExpected(Invoice invoice, decimal expectedAmount)
        {
            AssertActualDecimalValueMatchesExpected(invoice.AmountPaid, expectedAmount,"AmountPaid does not match expected value.");
        }
        
        private static void AssertTaxAmountMatchesExpected(Invoice invoice, decimal expectedAmount)
        {
            AssertActualDecimalValueMatchesExpected(invoice.TaxAmount, expectedAmount,"TaxAmount does not match expected value.");
        }

        private static void AssertActualDecimalValueMatchesExpected(decimal actual, decimal expected, string errorMessage)
        {
            Assert.That(actual, Is.EqualTo(expected), errorMessage);
        }
    }
}