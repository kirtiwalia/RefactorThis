using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Persistence.Enums
{
    public enum PaymentStatus
    {
        NoPaymentNeeded,
        FullyPaid,
        PartiallyPaid,
        Overpaid,
        AlreadyPaid,
        InvalidState,
        Error
    }
}

