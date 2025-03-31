using System;
using System.Collections.Generic;

namespace RefactorThis.Persistence
{
	public interface IPaymentRepository
	{
		IReadOnlyCollection<Payment> GetPaymentsByInvoiceId(Guid InvoiceId);
		void SavePayment(Payment payment);
	}
}