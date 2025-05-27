using System;

namespace RefactorThis.Domain
{
	/// <summary>
	/// Coordinates invoice look-up, applies a payment, and persists changes.
	/// </summary>
	public class InvoiceService
	{
		private readonly IInvoiceRepository _repository;

		public InvoiceService(IInvoiceRepository repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}

		/// <summary>
		/// Applies <paramref name="payment"/> to the referenced invoice.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// Thrown when no invoice matches the supplied reference.
		/// </exception>
		public string ProcessPayment(Payment payment)
		{
			var invoice = _repository.GetByReference(payment.Reference) ?? throw new InvalidOperationException(InvoiceMessages.NoInvoiceMatchingPayment);

            PaymentResult result = invoice.ApplyPayment(payment);

			_repository.Save(invoice);

			return result.Message;
		}
	}
}
