using System;
using RefactorThis.Domain;
using RefactorThis.Persistence;
namespace Refactor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            InvoiceService invoiceService = new InvoiceService();    
            Invoice invoice = new Invoice(new InvoiceRepository())
            {
                Amount = 100,
                AmountPaid = 100,
                Type = InvoiceType.Standard
            };
            Payment payment = new Payment()
            {
                Amount = 100,
                Reference = "Refactor"
            };  
            Console.WriteLine("Released Refactored");
        }
    }
}
