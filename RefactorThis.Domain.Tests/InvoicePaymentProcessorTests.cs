using Moq;
using NUnit.Framework;
using RefactorThis.Persistence;
using System;
using System.Collections.Generic;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        const decimal TaxPercentage = 0.14m;

        Mock<IInvoiceRepository> _invoiceRepositoryMock;

        [SetUp]
        public void Setup()
        {
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
        {
            var paymentProcessor = new InvoiceService(_invoiceRepositoryMock.Object);

            var payment = new Payment();
            var failureMessage = "";

            try
            {
                var result = paymentProcessor.ProcessPayment(payment);
            }
            catch (InvalidOperationException e)
            {
                failureMessage = e.Message;
            }

            Assert.That(failureMessage, Is.EqualTo("There is no invoice matching this payment"));

            _invoiceRepositoryMock.Verify(r => r.SaveInvoice(It.IsAny<Invoice>()), Times.Never);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            var invoice = new Invoice(_invoiceRepositoryMock.Object)
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null
            };

            _invoiceRepositoryMock.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var paymentProcessor = new InvoiceService(_invoiceRepositoryMock.Object);

            var payment = new Payment();

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo("no payment needed"));

            _invoiceRepositoryMock.Verify(r => r.SaveInvoice(It.IsAny<Invoice>()), Times.Never);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {

            var invoice = new Invoice(_invoiceRepositoryMock.Object)
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

            _invoiceRepositoryMock.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var paymentProcessor = new InvoiceService(_invoiceRepositoryMock.Object);

            var payment = new Payment();

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo("invoice was already fully paid"));

            _invoiceRepositoryMock.Verify(r => r.SaveInvoice(It.IsAny<Invoice>()), Times.Never);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            var invoice = new Invoice(_invoiceRepositoryMock.Object)
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
            _invoiceRepositoryMock.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var paymentProcessor = new InvoiceService(_invoiceRepositoryMock.Object);

            var payment = new Payment()
            {
                Amount = 6
            };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo("the payment is greater than the partial amount remaining"));

            _invoiceRepositoryMock.Verify(r => r.SaveInvoice(It.IsAny<Invoice>()), Times.Never);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            var invoice = new Invoice(_invoiceRepositoryMock.Object)
            {
                Amount = 5,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            _invoiceRepositoryMock.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var paymentProcessor = new InvoiceService(_invoiceRepositoryMock.Object);

            var payment = new Payment()
            {
                Amount = 6
            };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo("the payment is greater than the invoice amount"));

            _invoiceRepositoryMock.Verify(r => r.SaveInvoice(It.IsAny<Invoice>()), Times.Never);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            var invoice = new Invoice(_invoiceRepositoryMock.Object)
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
            _invoiceRepositoryMock.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var paymentProcessor = new InvoiceService(_invoiceRepositoryMock.Object);

            var payment = new Payment()
            {
                Amount = 5
            };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo("final partial payment received, invoice is now fully paid"));
            _invoiceRepositoryMock.Verify(r => r.SaveInvoice(It.Is<Invoice>(i => VerifyInvoice(i, 10, 10, 2))), Times.Once);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            var invoice = new Invoice(_invoiceRepositoryMock.Object)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>() { new Payment() { Amount = 10 } }
            };
            _invoiceRepositoryMock.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var paymentProcessor = new InvoiceService(_invoiceRepositoryMock.Object);

            var payment = new Payment()
            {
                Amount = 10
            };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo("invoice was already fully paid"));
            _invoiceRepositoryMock.Verify(r => r.SaveInvoice(It.IsAny<Invoice>()), Times.Never);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            var invoice = new Invoice(_invoiceRepositoryMock.Object)
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
            _invoiceRepositoryMock.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var paymentProcessor = new InvoiceService(_invoiceRepositoryMock.Object);

            var payment = new Payment()
            {
                Amount = 1
            };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo("another partial payment received, still not fully paid"));
            _invoiceRepositoryMock.Verify(r => r.SaveInvoice(It.Is<Invoice>(i => VerifyInvoice(i, 10, 6, 2))), Times.Once);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            var invoice = new Invoice(_invoiceRepositoryMock.Object)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            _invoiceRepositoryMock.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var paymentProcessor = new InvoiceService(_invoiceRepositoryMock.Object);

            var payment = new Payment()
            {
                Amount = 1
            };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo("invoice is now partially paid"));
            _invoiceRepositoryMock.Verify(r => r.SaveInvoice(It.Is<Invoice>(i => VerifyInvoice(i, 10, 1, 1))), Times.Once);
        }

        [Test]
        public void ProcessPayment_CommercialInvoice_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            var invoice = new Invoice(_invoiceRepositoryMock.Object)
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 5
                    }
                },
                Type = InvoiceType.Commercial
            };
            _invoiceRepositoryMock.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var paymentProcessor = new InvoiceService(_invoiceRepositoryMock.Object);

            var payment = new Payment()
            {
                Amount = 1
            };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo("another partial payment received, still not fully paid"));
            _invoiceRepositoryMock.Verify(r => r.SaveInvoice(It.Is<Invoice>(i => VerifyInvoice(i, 10, 6, 2))), Times.Once);
        }

        [Test]
        public void ProcessPayment_CommercialInvoice_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            var invoice = new Invoice(_invoiceRepositoryMock.Object)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>(),
                Type = InvoiceType.Commercial
            };
            _invoiceRepositoryMock.Setup(r => r.GetInvoice(It.IsAny<string>())).Returns(invoice);

            var paymentProcessor = new InvoiceService(_invoiceRepositoryMock.Object);

            var payment = new Payment()
            {
                Amount = 1
            };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.That(result, Is.EqualTo("invoice is now partially paid"));
            _invoiceRepositoryMock.Verify(r => r.SaveInvoice(It.Is<Invoice>(i => VerifyInvoice(i, 10, 1, 1))), Times.Once);
        }

        private bool VerifyInvoice(Invoice inv, decimal expectedAmount, decimal expectedAmountPaid, int expectedNumPayments)
        {
            Assert.That(inv, Is.Not.Null);
            Assert.That(inv.Amount, Is.EqualTo(expectedAmount));
            Assert.That(inv.AmountPaid, Is.EqualTo(expectedAmountPaid));
            Assert.That(inv.Payments, Has.Count.EqualTo(expectedNumPayments));
            if (inv.Type == InvoiceType.Commercial)
            {
                Assert.That(inv.TaxAmount, Is.EqualTo(inv.Amount * TaxPercentage));
            }
            return true;
        }
    }
}