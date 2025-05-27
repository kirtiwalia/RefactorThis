using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Domain
{
	/// <summary>Aggregate-root representing an accounts-receivable invoice.</summary>
	public class Invoice
	{
		public Invoice()
		{
			Payments = new List<Payment>();
		}

		// Properties
		public decimal Amount { get; set; }
		public decimal AmountPaid { get; private set; }
		public decimal TaxAmount { get; private set; }
		public List<Payment> Payments { get; }

		public InvoiceType Type { get; set; }

		/// <summary>
		/// Applies <paramref name="payment"/> and returns a rich result describing what happened.
		/// </summary>
		public PaymentResult ApplyPayment(Payment payment)
		{
			if (Amount == 0)
			{
				// No money owed => no payment required.
				if (Payments.Count == 0)
				{
					return PaymentResult.Failure(InvoiceMessages.NoPaymentNeeded);
				}

				throw new InvalidOperationException(InvoiceMessages.InvalidZeroAmountWithPayments);
			}

			if (AmountPaid == Amount)
			{
				return PaymentResult.Failure(InvoiceMessages.InvoiceAlreadyFullyPaid);
			}

			decimal remaining = Amount - AmountPaid;

			// Too much?
			if (payment.Amount > remaining)
			{
				if (Payments.Any())
				{
					return PaymentResult.Failure(InvoiceMessages.PaymentExceedsRemainingBalance);
				}

				return PaymentResult.Failure(InvoiceMessages.PaymentExceedsInvoiceAmount);
			}

			// Valid payment
			AmountPaid += payment.Amount;
			Payments.Add(payment);

			if (Type == InvoiceType.Commercial)
			{
				// Commercial invoices accrue tax on every payment.
				TaxAmount += payment.Amount * 0.14m;
			}
			else if (Type == InvoiceType.Standard && Payments.Count > 1)
			{
				// Standard invoices accrue tax only on the *first* payment,
				// mirroring original behaviour.
				TaxAmount += payment.Amount * 0.14m;
			}

			// Determine result message
			bool fullyPaid = AmountPaid == Amount;
			bool wasPartialBefore = Payments.Count > 1;

			if (fullyPaid)
			{
				if (wasPartialBefore)
				{
					return PaymentResult.Success(InvoiceMessages.FinalPartialPaymentComplete);
				}
				else
				{
					return PaymentResult.Success(InvoiceMessages.InvoiceNowFullyPaid);
				}
			}
			else
			{
				if (wasPartialBefore)
				{
					return PaymentResult.Success(InvoiceMessages.AdditionalPartialPayment);
				}
				else
				{
					return PaymentResult.Success(InvoiceMessages.InitialPartialPayment);
				}
			}
		}
	}
}
