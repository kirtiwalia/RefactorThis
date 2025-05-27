using System;
using System.Collections.Generic;
using RefactorThis.Domain.TaxStrategy;

namespace RefactorThis.Domain
{
	/// <summary>
	/// Aggregate root representing an accounts-receivable invoice.
	/// Encapsulates business rules for applying payments and tracking tax.
	/// </summary>
	public class Invoice
	{
		private readonly IInvoiceTaxStrategy _taxStrategy;

		/// <summary>
		/// Creates a new invoice of the specified type.
		/// </summary>
		/// <param name="type">The invoice type (e.g. Standard or Commercial).</param>
		public Invoice(InvoiceType type)
		{
			Type = type;
			Payments = new List<Payment>();
			_taxStrategy = InvoiceTaxStrategyFactory.GetStrategy(type);
		}

		/// <summary>
		/// The total invoice amount due.
		/// </summary>
		public decimal Amount { get; set; }

		/// <summary>
		/// The total amount paid against this invoice so far.
		/// </summary>
		public decimal AmountPaid { get; private set; }

		/// <summary>
		/// The total tax amount calculated from payments.
		/// </summary>
		public decimal TaxAmount { get; private set; }

		/// <summary>
		/// The list of payments that have been applied to this invoice.
		/// </summary>
		public List<Payment> Payments { get; }

		/// <summary>
		/// The classification of this invoice (e.g., Standard or Commercial).
		/// </summary>
		public InvoiceType Type { get; }

		/// <summary>
		/// Applies a payment to this invoice and returns a result indicating the outcome.
		/// Business rules around overpayment, full payment, and tax calculation are enforced.
		/// </summary>
		/// <param name="payment">The payment to apply.</param>
		/// <returns>A <see cref="PaymentResult"/> indicating success or failure with a message.</returns>
		public PaymentResult ApplyPayment(Payment payment)
		{
			var validationResult = ValidateInvoiceState();
			if (validationResult != null)
			{
				return validationResult;
			}

			if (IsAlreadyFullyPaid())
			{
				return PaymentResult.Failure(InvoiceMessages.InvoiceAlreadyFullyPaid);
			}

			if (IsOverPayment(payment))
			{
				var message = Payments.Count > 0
					? InvoiceMessages.PaymentExceedsRemainingBalance
					: InvoiceMessages.PaymentExceedsInvoiceAmount;

				return PaymentResult.Failure(message);
			}

			ApplyPaymentInternal(payment);

			return PaymentResult.Success(GenerateResultMessage());
		}

		/// <summary>
		/// Validates the current state of the invoice and returns a failure result if needed.
		/// </summary>
		/// <returns>A <see cref="PaymentResult"/> if invalid, otherwise null.</returns>
		private PaymentResult /*nullable*/ ValidateInvoiceState()
		{
			if (Amount == 0)
			{
				if (Payments.Count == 0)
				{
					return PaymentResult.Failure(InvoiceMessages.NoPaymentNeeded);
				}

				throw new InvalidOperationException(InvoiceMessages.InvalidZeroAmountWithPayments);
			}

			return null;
		}

		/// <summary>
		/// Determines whether the invoice has already been fully paid.
		/// </summary>
		private bool IsAlreadyFullyPaid() => AmountPaid == Amount;

		/// <summary>
		/// Determines whether the given payment amount exceeds the remaining balance.
		/// </summary>
		private bool IsOverPayment(Payment payment) => payment.Amount > (Amount - AmountPaid);

		/// <summary>
		/// Applies a valid payment, updating the amount paid, tax, and payment history.
		/// </summary>
		private void ApplyPaymentInternal(Payment payment)
		{
			AmountPaid += payment.Amount;
			Payments.Add(payment);
			TaxAmount += _taxStrategy.CalculateTax(payment.Amount, Payments.Count);
		}

		/// <summary>
		/// Generates a result message describing the new payment state.
		/// </summary>
		private string GenerateResultMessage()
		{
			bool fullyPaid = AmountPaid == Amount;
			bool wasPartialBefore = Payments.Count > 1;

			if (fullyPaid)
			{
				return wasPartialBefore
					? InvoiceMessages.FinalPartialPaymentComplete
					: InvoiceMessages.InvoiceNowFullyPaid;
			}

			return wasPartialBefore
				? InvoiceMessages.AdditionalPartialPayment
				: InvoiceMessages.InitialPartialPayment;
		}
	}
}