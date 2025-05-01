

## Instructions
This repository is comprised of badly written code and some unit tests. 

The overall objective is to refactor the code and keep the tests passing.  There is no 'correct' solution to this task.  We are interested in how you would approach this problem, what clean maintainable code looks like to you and your ability to explain your changes.

* Changing the current tests is ok.
* Approach this as if it were the beginning of a project that was likely to become a very large and long-lived solution. 
* Structure dependencies and organise the solution as you see fit. 
* Refactor the "ProcessPayment" method, and give it your best shot at making it readable and maintainable.
* Complete the refactoring in .NET 472 or if upgrading **do not** delete all the existing files and add new ones. (This makes it difficult to review.)

## Expectations
* We expect this will take you between 1 and 2 hours.  We are not expecting you to spend significantly longer on this task.
* Do enough to show us what your approach will be and how you would organise things.
* We will likely ask you to explain/justify your solution in one of the interview rounds.

## Submission
* Clone this repository and push it to a private repository under your own account. (You are welcome to fork the repository, but that will be publically visible.)
* Branch, make your changes, and **create a pull request**.
* Invite *re-leased-hiring* as a reviewer to the PR.
* Let your hiring rep know your refactoring task is ready for review.



### Code refactoring improvements
* Validation Separation: Moved validation logic to dedicated InvoiceValidator
* Process Decomposition: Split payment processing into discrete, focused methods
* Interface Contracts: Established clear interfaces for all dependencies
* Method Simplification: Broke down monolithic ProcessPayment into single-responsibility methods
* Control Flow: Eliminated nested conditionals for linear readability
* Error Handling: Implemented structured exception handling with code safety
* Added logging for issues investigation

* The string return type in ProcessPayment is problematic as it forces client to parse unstructured messages; 
* a strongly-typed response object would provide clearer contracts and better programmability.


* Client side, before:

// Returns unstructured string messages

        string result = _invoiceService.ProcessPayment(payment);

// Client must parse strings to determine outcome. How to do that efficiently?

* Client side, after:

      PaymentResult result = _invoiceService.ProcessPayment(payment);

      if (result.IsSuccess)
      {
          switch (result.Code)
          {
              case ResultCode.FinalPaymentComplete:
              _receiptService.Generate(invoice, result.AmountPaid);
              _auditService.LogPaymentComplete(result.TransactionId);
              break;
              
              case ResultCode.PartialPaymentComplete:
                  _paymentTracker.ScheduleFollowUp(
                      result.RemainingAmount,
                      DateTime.Now.AddDays(7));
              break;
      
              case ResultCode.NoPaymentNeeded:
              case ResultCode.AlreadyFullyPaid:
                  // Handle these cases as needed
                  break;
          }
      }
      else
      {
          // Handle failure scenarios
          switch (result.Code)
          {
              case ResultCode.InvoiceNotFound:
              _alertService.TriggerSupportAlert(result);
              break;
              
              case ResultCode.PaymentExceedsRemainingAmount:
              case ResultCode.PaymentExceedsInvoiceAmount:
                  // Handle these cases as needed
              break;
      
              case ResultCode.ProcessingError:
              case ResultCode.InvalidInvoiceState:
                  // Handle these cases as needed
              break;
      
              default:
                  _alertService.TriggerSupportAlert(result);
              break;
          }
      }

*** Architectural improvements
* Proper layering with domain entities, services, and persistence
* Dependency injection for better testability
* Clear boundaries between components
* Each class has a single responsibility, easier to modify or extend behavior. Clear separation makes it easier to add new features.


*** Suggested Business Object Enhancements (not implemented):


* Invoice/Payment Data Enrichment
* Currently missing critical transaction details: Payer/recipient identification (names, contact info)
* Banking/payment details
* Transaction references





