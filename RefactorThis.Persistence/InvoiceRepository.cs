using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence
{
    // Interface for repository operations
    public interface IInvoiceRepository
    {
        Invoice GetInvoice(string reference);
        void SaveInvoice(Invoice invoice);
        void Add(Invoice invoice);
        bool Exists(string reference);
        IEnumerable<Invoice> GetAll();
    }

    // Implementation that maintains backward compatibility with existing code
    public class InvoiceRepository : IInvoiceRepository
    {
        // For backward compatibility, maintain the single invoice field
        // but add a dictionary for proper multi-invoice support
        private readonly Dictionary<string, Invoice> _invoices = new Dictionary<string, Invoice>(StringComparer.OrdinalIgnoreCase);
        private Invoice _lastInvoice; // For legacy code support

        public Invoice GetInvoice(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return _lastInvoice; // Legacy behavior for tests
            }

            return _invoices.TryGetValue(reference, out var invoice) ? invoice : null;
        }

        public void SaveInvoice(Invoice invoice)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            // For legacy code without reference
            if (string.IsNullOrWhiteSpace(invoice.Reference))
            {
                // Just update the last invoice for backward compatibility
                _lastInvoice = invoice;
                return;
            }

            // Store in dictionary for modern code
            _invoices[invoice.Reference] = invoice;
            _lastInvoice = invoice; // Keep reference for legacy code
        }

        public void Add(Invoice invoice)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            // For legacy code without reference
            if (string.IsNullOrWhiteSpace(invoice.Reference))
            {
                _lastInvoice = invoice;
                return;
            }

            // Check for duplicates in modern implementation
            if (_invoices.ContainsKey(invoice.Reference))
            {
                throw new InvalidOperationException($"An invoice with reference '{invoice.Reference}' already exists");
            }

            _invoices[invoice.Reference] = invoice;
            _lastInvoice = invoice; // Keep reference for legacy code
        }

        public bool Exists(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return _lastInvoice != null; // Legacy behavior
            }

            return _invoices.ContainsKey(reference);
        }

        public IEnumerable<Invoice> GetAll()
        {
            // Include the legacy invoice if it has no reference
            var allInvoices = _invoices.Values.ToList();

            if (_lastInvoice != null && string.IsNullOrWhiteSpace(_lastInvoice.Reference) &&
                !allInvoices.Contains(_lastInvoice))
            {
                allInvoices.Add(_lastInvoice);
            }

            return allInvoices;
        }
    }

    // A modern implementation for new code
    public class ModernInvoiceRepository : IInvoiceRepository
    {
        private readonly Dictionary<string, Invoice> _invoices = new Dictionary<string, Invoice>(StringComparer.OrdinalIgnoreCase);

        // Database context would be injected here in real implementation

        public Invoice GetInvoice(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                throw new ArgumentException("Invoice reference cannot be null or empty", nameof(reference));
            }

            return _invoices.TryGetValue(reference, out var invoice) ? invoice : null;
        }

        public void SaveInvoice(Invoice invoice)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            if (string.IsNullOrWhiteSpace(invoice.Reference))
            {
                throw new ArgumentException("Invoice must have a valid reference", nameof(invoice));
            }

            _invoices[invoice.Reference] = invoice;
        }

        public void Add(Invoice invoice)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            if (string.IsNullOrWhiteSpace(invoice.Reference))
            {
                throw new ArgumentException("Invoice must have a valid reference", nameof(invoice));
            }

            if (_invoices.ContainsKey(invoice.Reference))
            {
                throw new InvalidOperationException($"An invoice with reference '{invoice.Reference}' already exists");
            }

            _invoices[invoice.Reference] = invoice;
        }

        public bool Exists(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                throw new ArgumentException("Invoice reference cannot be null or empty", nameof(reference));
            }

            return _invoices.ContainsKey(reference);
        }

        public IEnumerable<Invoice> GetAll()
        {
            return _invoices.Values.ToList();
        }
    }
}