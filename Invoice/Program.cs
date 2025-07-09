using System;
using System.Collections.Generic;
using RefactorThis.Domain;
using RefactorThis.Persistence;

namespace RefactorThis.ConsoleApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            IRepository repo = new InvoiceRepository();
            IInvoiceService invoiceService = new InvoiceService(repo);

            // Setup a test invoice
            var invoice = new Invoice(repo)
            {
                Amount = 100,
                AmountPaid = 0,
                Payments = new List<Payment>(),
                Type = InvoiceType.Commercial
            };
            repo.Add(invoice);

            // Process a payment
            var payment = new Payment
            {
                Amount = 100,
                Reference = "" // match what's in the repo
            };

            Console.WriteLine("Debugging ProcessPayment method for PR testing."); // ✅ forceful, visible change


            string result = invoiceService.ProcessPayment(payment);
            Console.WriteLine(result);
        }
    }
}
