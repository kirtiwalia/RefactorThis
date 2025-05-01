using System;

namespace RefactorThis.Domain.Exceptions
{
    public class InvoiceValidationException : Exception
    {
        public InvoiceValidationException(string message) 
            : base(message)
        {}

        public InvoiceValidationException(string message, Exception innerException)
            : base(message, innerException)
        {}
    }
}