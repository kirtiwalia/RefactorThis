

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


---------------------------------

## Summary
* In this code refactoring, I hope to follow the principles of Clean Architecture to make the code clearer, more maintainable, and easier to extend. However, since the project does not allow the addition of new files, I can only define the interface (IInvoiceRepository) inside InvoiceRepository instead of creating a new file separately.

## Problems
* InvoiceService has complex logic：
    * The ProcessPayment method is too long and has too many nested if-else statements.
    * InvoiceService depends directly on InvoiceRepository and does not use interfaces, making it difficult to test.
    * The mutual dependency problem between Invoice and InvoiceRepository.

* InvoiceRepository has a design flaw:：
    * Can only store one Invoice, not suitable for real business scenarios.
    * The GetInvoice method cannot correctly return the Invoice corresponding to the Reference..etc

* Invoice has no unique identifier (Reference)：
    * In the business logic, Payment relies on Reference to find Invoice, but Invoice itself does not have a    Reference attribute, resulting in confusion in data access.

* Test code is unstable：
    * Direct dependency on InvoiceRepository causes the test to not run independently.



## Solution
* Split the ProcessPayment method into different small methods to avoid one method taking on too many    responsibilities.
* Dependency inversion is performed through IInvoiceRepository, allowing different data storage methods to be replaced
* InvoiceRepository inherits IInvoiceRepository to ensure that FakeInvoiceRepository can replace the real database for testing.
* IInvoiceRepository only contains GetInvoice, SaveInvoice and Add to avoid unnecessary coupling.
* InvoiceService depends on IInvoiceRepository instead of InvoiceRepository to improve testability.

* Clean Code
    * Split the ProcessPayment method:
        * Extract the GetPaymentStatus method: avoid multiple if-else nesting.
        * Extract ApplyPayment method: focus on payment logic and improve readability.
        * Define PaymentStatus enumeration to make the logic clearer.
    * Simplifying the InvoiceRepository：
        * Use Dictionary<string, Invoice> with Reference as the key to ensure efficient query.

