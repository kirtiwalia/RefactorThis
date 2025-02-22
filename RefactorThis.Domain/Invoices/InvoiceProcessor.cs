using RefactorThis.Domain.Payments;

namespace RefactorThis.Domain.Invoices
{
    public class InvoiceProcessor
    {
        public string Process(Invoice invoice, Payment payment)
        {
            payment.IsInitialPayment = !invoice.HasPayments();

            if (invoice.TotalAmountPaid > 0)
            {
                if (invoice.FullyPaid)
                    return InvoiceError.InvoiceAlreadyFullyPaid;

                if (payment.Amount > invoice.Balance)
                    return InvoiceError.PaymentIsGreaterThanRemainingBalance;
            }
            else if (payment.Amount > invoice.Amount)
            {
                return InvoiceError.PaymentIsGreaterThanInvoiceAmount;
            }

            payment.PaymentType = DeterminePaymentType(invoice, payment);
            return ApplyPayment(invoice, payment);
        }

        private PaymentTypeEnum DeterminePaymentType(Invoice invoice, Payment payment)
        {
            return invoice.Balance == payment.Amount ?
                PaymentTypeEnum.Final : PaymentTypeEnum.Partial;
        }

        private string ApplyPayment(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid += payment.Amount;
            invoice.TaxAmount +=
                payment.IsInitialPayment ? payment.Amount * 0.14m
                    : invoice.Type.CalculateTax(payment.Amount);
            invoice.Payments.Add(payment);

            if (invoice.FullyPaid)
            {
                return payment.PaymentType == PaymentTypeEnum.Final
                    ? InvoicePaymentStatus.FinalPaymentFullyPaid
                    : InvoicePaymentStatus.FullyPaid;
            }

            return payment.IsInitialPayment
                ? InvoicePaymentStatus.PartiallyPaid
                : InvoicePaymentStatus.PartialPaymentReceived;
        }
    }
}
