namespace RefactorThis.Domain.common.enums
{
    public enum ProcessPaymentStatus
    {
        // "No payment needed";
        NoPaymentNeeded,
        // "Invoice was already fully paid";
        InvoiceAlreadyFullyPaid,
        // "The payment is greater than the partial amount remaining";
        PartialPaymentExistsAndAmountPaidExceedsAmountDue,
        // "The payment is greater than the invoice amount";
        NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount,
        // "Another partial payment received, still not fully paid";
        PartialPaymentExistsAndAmountPaidIsLessThanAmountDue,
        // "Final partial payment received, invoice is now fully paid";
        PartialPaymentExistsAndAmountPaidEqualsAmountDue
    }
}