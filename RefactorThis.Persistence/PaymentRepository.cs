using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence
{
	public class PaymentRepository : IPaymentRepository
	{
		private readonly IList<Payment> _payments;

		public PaymentRepository()
		{
			_payments = new List<Payment>();
		}

		public IReadOnlyCollection<Payment> GetPaymentsByInvoiceId(Guid InvoiceId)
		{
			return _payments.Where(x=> x.InvoiceId == InvoiceId).ToList();
		}

		public void SavePayment(Payment payment)
		{
			_payments.Add(payment);
		}
	}
}