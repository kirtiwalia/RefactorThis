namespace RefactorThis.Domain.Models
{
    public class PaymentResult
    {
        public bool IsSuccess { get; }
        public ResultCode? Code { get; }
        public string Message { get; }

        public static PaymentResult SuccessResult(ResultCode? code = null, string message = null) 
            => new PaymentResult(true, code, message);
            
        public static PaymentResult FailureResult(ResultCode? code = null, string message = null) 
            => new PaymentResult(false, code, message);
        
        private PaymentResult(bool isSuccess, ResultCode? code = null, string message = null)
        {
            IsSuccess = isSuccess;
            Code = code ?? ResultCode.Unknown;
            Message = string.IsNullOrEmpty(message) ? "No comments" : message;
        }
    }

    public enum ResultCode
    {
        Unknown,
        // Success
        NoPaymentNeeded,
        AlreadyFullyPaid,
        FinalPaymentComplete,
        PartialPaymentComplete,
        
        // Failure
        InvoiceNotFound,
        InvalidInvoiceState,
        PaymentExceedsRemainingAmount,
        PaymentExceedsInvoiceAmount,
        ProcessingError
    }
}