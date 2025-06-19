using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Domain.Entities;
using RefactorThis.Persistence.Repository;
using RefactorThis.Application.Commands;
namespace RefactorThis.Domain.Tests
{
	[TestFixture]
	public class InvoicePaymentProcessorTests
	{
		[Test]
		public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
		{
			var repo = new InvoiceRepository();

			Invoice invoice = null;
			var paymentProcessor = new ProcessPaymentCommandHandler(repo);

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

			Assert.AreEqual("There is no invoice matching this payment", failureMessage);
		}

		
	}
}