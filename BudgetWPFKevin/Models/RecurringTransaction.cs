namespace BudgetWPFKevin.Models
{

    public enum RecurrenceType
    {
        Monthly,
        Yearly
    }

    public class RecurringTransaction
    {
        public int Id { get; set; }

        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public bool IsRecurring { get; set; }

        public RecurrenceType RecurrenceType { get; set; }
        public int? Month { get; set; }

        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        public bool IsSystemGenerated { get; set; }
    }
}
