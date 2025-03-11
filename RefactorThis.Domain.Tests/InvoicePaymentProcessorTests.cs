using System;
using Moq;
using RefactorThis.Persistence;
using static RefactorThis.Domain.InvoiceService;

namespace RefactorThis.Domain.Tests
{
	[TestFixture]
	public class InvoicePaymentProcessorTests
	{
        private Mock<IInvoiceRepository> _invoiceRepository;
        private InvoiceService _invoiceService;

        [SetUp]
        public void Setup()
        {
            _invoiceRepository = new Mock<IInvoiceRepository>();
            _invoiceService = new InvoiceService(_invoiceRepository.Object);
        }

		[Test]
		public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
		{
            // Arrange
			var payment = new Payment();
			var failureMessage = "";

            _invoiceRepository.Setup(repo => repo.GetInvoice("123")).Throws<InvalidOperationException>();
            
            // Act
			try
			{
				var result = _invoiceService.ProcessPayment(payment);
			}   
			catch (InvalidOperationException e)
			{
				failureMessage = e.Message;
			}

            // Assert
            _invoiceRepository.Verify(repo => repo.SaveInvoice(It.IsAny<Invoice>()), Times.Never);
            Assert.That(failureMessage, Is.EqualTo("There is no invoice matching this payment"));
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
		{
            // Arrange
			var invoice = new Invoice()
			{
				Amount = 0,
				AmountPaid = 0,
				Payments = null
			};
            var payment = new Payment();

            // Act
            _invoiceRepository.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);
			var result = _invoiceService.ProcessPayment(payment);

            // Assert
            _invoiceRepository.Verify(repo => repo.SaveInvoice(It.IsAny<Invoice>()), Times.Once);
            Assert.That(result, Is.EqualTo(PaymentMessages.NoPaymentNeeded));
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
		{
            // Arrange
            var payment = new Payment();
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
			
            // Act
            _invoiceRepository.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);
			var result = _invoiceService.ProcessPayment(payment);
			
            // Assert
            _invoiceRepository.Verify(repo => repo.SaveInvoice(It.IsAny<Invoice>()), Times.Once);
            Assert.That(result, Is.EqualTo(PaymentMessages.InvoiceAlreadyFullyPaid));
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
		{
            // Arrange
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

			var payment = new Payment()
			{
				Amount = 6
			};
			
            // Act
            _invoiceRepository.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);
			var result = _invoiceService.ProcessPayment(payment);
			
            // Assert
            _invoiceRepository.Verify(repo => repo.SaveInvoice(It.IsAny<Invoice>()), Times.Once);
            Assert.That(result, Is.EqualTo(PaymentMessages.PaymentGreaterThanRemaining));
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
		{
            // Arrange
			var invoice = new Invoice()
			{
				Amount = 5,
				AmountPaid = 0,
				Payments = new List<Payment>()
			};

			var payment = new Payment()
			{
				Amount = 6
			};
            
            // Act
            _invoiceRepository.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);
			var result = _invoiceService.ProcessPayment(payment);
			
            // Assert
            _invoiceRepository.Verify(repo => repo.SaveInvoice(It.IsAny<Invoice>()), Times.Once);
            Assert.That(result, Is.EqualTo(PaymentMessages.PaymentGreaterThanInvoice));
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
		{
            // Arrange
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

			var payment = new Payment()
			{
				Amount = 5
			};

            // Act
            _invoiceRepository.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);
			var result = _invoiceService.ProcessPayment(payment);
			
            // Assert
            _invoiceRepository.Verify(repo => repo.SaveInvoice(It.IsAny<Invoice>()), Times.Once);
            Assert.That(result, Is.EqualTo(PaymentMessages.FinalPartialPaymentReceived));
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
		{
            // Arrange
			var invoice = new Invoice()
			{
				Amount = 10,
				AmountPaid = 0,
				Payments = new List<Payment>() { new Payment() { Amount = 10 } }
			};

			var payment = new Payment()
			{
				Amount = 10
			};
            // Act
            _invoiceRepository.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);
			var result = _invoiceService.ProcessPayment(payment);
			
            // Assert
            _invoiceRepository.Verify(repo => repo.SaveInvoice(It.IsAny<Invoice>()), Times.Once);
            Assert.That(result, Is.EqualTo(PaymentMessages.InvoiceAlreadyFullyPaid));
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
		{
            // Arrange
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

			var payment = new Payment()
			{
				Amount = 1
			};

            // Act
            _invoiceRepository.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);
			var result = _invoiceService.ProcessPayment(payment);
			
            // Assert
            _invoiceRepository.Verify(repo => repo.SaveInvoice(It.IsAny<Invoice>()), Times.Once);
            Assert.That(result, Is.EqualTo(PaymentMessages.AnotherPartialPaymentReceived));
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
		{
            // Arrange
			var invoice = new Invoice()
			{
				Amount = 10,
				AmountPaid = 0,
				Payments = new List<Payment>()
			};

			var payment = new Payment()
			{
				Amount = 1
			};

            // Act
            _invoiceRepository.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);
			var result = _invoiceService.ProcessPayment(payment);
			
            // Assert
            _invoiceRepository.Verify(repo => repo.SaveInvoice(It.IsAny<Invoice>()), Times.Once);
            Assert.That(result, Is.EqualTo(PaymentMessages.InvoicePartiallyPaid));
		}

        [Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PaymentExistsAndAmountPaidIsEqualToInvoiceAmount()
		{
            // Arrange
			var invoice = new Invoice()
			{
				Amount = 10,
				AmountPaid = 0,
				Payments = null
			};

			var payment = new Payment()
			{
				Amount = 10
			};
            
            // Act
            _invoiceRepository.Setup(repo => repo.GetInvoice(It.IsAny<string>())).Returns(invoice);
			var result = _invoiceService.ProcessPayment(payment);
			
            // Assert
            _invoiceRepository.Verify(repo => repo.SaveInvoice(It.IsAny<Invoice>()), Times.Once);
            Assert.That(result, Is.EqualTo(PaymentMessages.InvoiceNowFullyPaid));
		}
	}
}