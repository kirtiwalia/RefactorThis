using RefactorThis.Core.entities;
using RefactorThis.Core.enums;
using RefactorThis.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Domain
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public string ProcessPayment(Payment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException("Payment should not be null");
            }
            if (payment.Reference == null)
            {
                throw new ArgumentNullException("Payment reference should not be null");
            }
            if(payment.Amount < 0)
            {
                throw new InvalidOperationException("Payment amount should be a positive value");
            }

            var inv = _invoiceRepository.GetInvoice(payment.Reference);

            if (inv == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }
            if (inv.Amount == 0)
            {
                if (IsPaymentListEmpty(inv.Payments))
                {
                   return "no payment needed";
                }
                else
                {
                    throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
                }
            }

            return HasExistingPayments(inv) ? ProcessExistingPayments(inv, payment) : ProcessInitialPayment(inv, payment);


     
        }

        private string ProcessExistingPayments(Invoice inv, Payment payment)
        {
            decimal totalPayments = inv.Payments.Sum(p => p.Amount);
            decimal remainingBalance = inv.Amount - inv.AmountPaid;
         

            if (inv.Amount == totalPayments)

                return "invoice was already fully paid";

            if (payment.Amount > remainingBalance)

                return "the payment is greater than the partial amount remaining";

            ApplyPayment(inv, payment,false);

            return remainingBalance == payment.Amount ? "final partial payment received, invoice is now fully paid"
            : "another partial payment received, still not fully paid";

    
        }

        private string ProcessInitialPayment(Invoice inv, Payment payment)
        {
            if (payment.Amount > inv.Amount)
                return "the payment is greater than the invoice amount";
            
            ApplyPayment(inv, payment,true);
            return  inv.Amount == payment.Amount ? "invoice is now fully paid" : "invoice is now partially paid";

        }

        private void ApplyPayment(Invoice inv, Payment payment,bool isInitial)
        {
            if(inv.Payments == null)
            {
                inv.Payments = new List<Payment>(); 
            }

            // Logic of orginal code
            // if "HasExistingPayments = true" and a payment has been made 
            // for standard tax amount will not increment/change
            // for commercial tax amount will increment by 14% of the payment amount
            // if Initial Payment has been made
            // given that initial value of amountpain and taxamount is defaulted to 0 
            // we can use the += operator for both amount paid and taxamount(on commercial type) to set/increment the values accordingly


            switch (inv.Type)
            {
                case InvoiceType.Standard:
                    inv.AmountPaid += payment.Amount;
                    inv.TaxAmount = isInitial ? payment.Amount * 0.14m : inv.TaxAmount;
                    inv.Payments.Add(payment);
                    break;
                case InvoiceType.Commercial:
                    inv.AmountPaid += payment.Amount;
                    inv.TaxAmount += payment.Amount * 0.14m;
                    inv.Payments.Add(payment);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _invoiceRepository.SaveInvoice(inv);
        }

        private bool HasExistingPayments(Invoice inv)
        {
            return inv.Payments != null && inv.Payments.Any();
        }
        private bool IsPaymentListEmpty(List<Payment> payments) =>
          payments == null || !payments.Any();
    }

}