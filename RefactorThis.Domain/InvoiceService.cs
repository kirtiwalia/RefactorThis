using RefactorThis.Persistence;
using System.Collections.Generic;
using System;
using System.Linq;

public class InvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;

    public InvoiceService(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    /// <summary>
    /// Processes a payment for a given invoice.
    /// </summary>
    /// <param name="payment">The payment details.</param>
    /// <returns>A message indicating the payment status.</returns>
    public string ProcessPayment(Payment payment)
    {
        var inv = _invoiceRepository.GetInvoice(payment.Reference);

        if (inv == null)
            throw new InvalidOperationException("There is no invoice matching this payment");

        string responseMessage;

        // Handle invoices with zero amount separately
        if (inv.Amount == 0)
        {
            responseMessage = HandleZeroAmountInvoice(inv);
        }
        else
        {
            responseMessage = ProcessNonZeroAmountInvoice(inv, payment);
        }

        // Save the updated invoice state
        _invoiceRepository.SaveInvoice(inv);
        return responseMessage;
    }

    /// <summary>
    /// Handles payment processing for invoices with a zero balance.
    /// </summary>
    /// <param name="inv">The invoice to process.</param>
    /// <returns>A message indicating the payment status.</returns>
    private string HandleZeroAmountInvoice(Invoice inv)
    {
        if (inv.Payments == null || !inv.Payments.Any())
            return "no payment needed";

        throw new InvalidOperationException("The invoice is in an invalid state.");
    }

    /// <summary>
    /// Handles payment processing for invoices with a non-zero balance.
    /// </summary>
    /// <param name="inv">The invoice to process.</param>
    /// <param name="payment">The payment details.</param>
    /// <returns>A message indicating the payment status.</returns>
    private string ProcessNonZeroAmountInvoice(Invoice inv, Payment payment)
    {
        decimal totalPaid = inv.Payments?.Sum(p => p.Amount) ?? 0m;
        decimal remainingAmount = inv.Amount - totalPaid;
        bool isFirstPayment = totalPaid == 0m;

        // Check if the invoice has already been fully paid
        if (totalPaid >= inv.Amount)
            return "invoice was already fully paid";

        // If it's the first payment and exceeds the invoice amount, return an error
        if (isFirstPayment && payment.Amount > inv.Amount)
            return "the payment is greater than the invoice amount";

        // If it's a subsequent payment that exceeds the remaining amount, return an error
        if (!isFirstPayment && payment.Amount > remainingAmount)
            return "the payment is greater than the partial amount remaining";

        return ProcessValidPayment(inv, payment, remainingAmount, isFirstPayment);
    }

    /// <summary>
    /// Processes a valid payment and updates the invoice.
    /// </summary>
    /// <param name="inv">The invoice being updated.</param>
    /// <param name="payment">The payment details.</param>
    /// <param name="remainingAmount">The remaining balance before this payment.</param>
    /// <param name="isFirstPayment">Indicates if this is the first payment.</param>
    /// <returns>A message indicating the payment status.</returns>
    private string ProcessValidPayment(Invoice inv, Payment payment, decimal remainingAmount, bool isFirstPayment)
    {
        // Ensure the payments list is initialized and add the new payment
        (inv.Payments ?? (inv.Payments = new List<Payment>())).Add(payment);

        // Apply tax if it's a commercial invoice
        decimal tax = inv.Type == InvoiceType.Commercial ? payment.Amount * 0.14m : 0m;
        inv.AmountPaid += payment.Amount;
        inv.TaxAmount += tax;

        return DetermineResponseMessage(inv.Amount, inv.AmountPaid, remainingAmount, payment.Amount, isFirstPayment);
    }

    /// <summary>
    /// Determines the appropriate response message based on the invoice and payment details.
    /// </summary>
    /// <param name="invoiceAmount">The total invoice amount.</param>
    /// <param name="amountPaid">The total amount paid so far.</param>
    /// <param name="remainingAmount">The remaining balance before this payment.</param>
    /// <param name="paymentAmount">The amount of the current payment.</param>
    /// <param name="isFirstPayment">Indicates if this is the first payment.</param>
    /// <returns>A message indicating the current payment status.</returns>
    private string DetermineResponseMessage(decimal invoiceAmount, decimal amountPaid,
                                          decimal remainingAmount, decimal paymentAmount, bool isFirstPayment)
    {
        if (paymentAmount == remainingAmount)
            return "final partial payment received, invoice is now fully paid";

        if (amountPaid == invoiceAmount)
            return "invoice is now fully paid";

        if (isFirstPayment && amountPaid < invoiceAmount)
            return "invoice is now partially paid";

        return "another partial payment received, still not fully paid";
    }
}