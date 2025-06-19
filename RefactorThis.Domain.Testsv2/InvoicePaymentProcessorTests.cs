using RefactorThis.Application.Commands;
using RefactorThis.Domain.Entities;
using RefactorThis.Persistence.Repository;

namespace RefactorThis.Domain.Testsv2
{
    public class InvoicePaymentProcessorTests
    {
        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
        {
            // Arrange
            var repo = new InvoiceRepository(); 
            var handler = new ProcessPaymentCommandHandler(repo);

            var payment = new Payment
            {
                Reference = "INV-404"
            };

            var failureMessage = "";

            // Act
            try
            {
                var result = handler.Handle(new ProcessPaymentCommand { Payment = payment });
            }
            catch (Exception ex)
            {
                failureMessage = ex.Message;
            }

            // Assert
            Assert.AreEqual("There is no invoice matching this payment", failureMessage);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            var repo = new InvoiceRepository();

            var invoice = new Invoice()
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null
            };

            repo.Add(invoice);

            var paymentProcessor = new ProcessPaymentCommandHandler(repo);

            var payment = new Payment()
            {
                Reference = null
            };

            var result = paymentProcessor.Handle(new ProcessPaymentCommand { Payment = payment });

            Assert.AreEqual("no payment needed", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            var repo = new InvoiceRepository();

            var invoice = new Invoice()
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
            repo.Add(invoice);

            var paymentProcessor = new ProcessPaymentCommandHandler(repo);

            var payment = new Payment();

            var result = paymentProcessor.Handle(new ProcessPaymentCommand { Payment = payment });

            Assert.AreEqual("invoice was already fully paid", result);
        }



        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice()
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
            repo.Add(invoice);

            var paymentProcessor = new ProcessPaymentCommandHandler(repo);

            var payment = new Payment()
            {
                Amount = 6
            };

            var result = paymentProcessor.Handle(new ProcessPaymentCommand { Payment = payment });

            Assert.AreEqual("the payment is greater than the partial amount remaining", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice()
            {
                Amount = 5,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            repo.Add(invoice);

            var paymentProcessor = new ProcessPaymentCommandHandler(repo);

            var payment = new Payment()
            {
                Amount = 6
            };

            var result = paymentProcessor.Handle(new ProcessPaymentCommand { Payment = payment });

            Assert.AreEqual("the payment is greater than the invoice amount", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice()
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
            repo.Add(invoice);

            var paymentProcessor = new ProcessPaymentCommandHandler(repo);

            var payment = new Payment()
            {
                Amount = 5
            };

            var result = paymentProcessor.Handle(new ProcessPaymentCommand { Payment = payment });

            Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice()
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>() { new Payment() { Amount = 10 } }
            };
            repo.Add(invoice);

            var paymentProcessor = new ProcessPaymentCommandHandler(repo);

            var payment = new Payment()
            {
                Amount = 10
            };

            var result = paymentProcessor.Handle(new ProcessPaymentCommand { Payment = payment });

            Assert.AreEqual("invoice was already fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice()
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
            repo.Add(invoice);

            var paymentProcessor = new ProcessPaymentCommandHandler(repo);

            var payment = new Payment()
            {
                Amount = 1
            };

            var result = paymentProcessor.Handle(new ProcessPaymentCommand { Payment = payment });

            Assert.AreEqual("another partial payment received, still not fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice()
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            repo.Add(invoice);

            var paymentProcessor = new ProcessPaymentCommandHandler(repo);

            var payment = new Payment()
            {
                Amount = 1
            };

            var result = paymentProcessor.Handle(new ProcessPaymentCommand { Payment = payment });

            Assert.AreEqual("invoice is now partially paid", result);
        }
    }
}