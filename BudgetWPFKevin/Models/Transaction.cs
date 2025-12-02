using System.ComponentModel;

namespace BudgetWPFKevin.Models
{
    public enum TransactionType
    {
        [Description("Inkomst")]
        Income,

        [Description("Utgift")]
        Expense
    }

    public class Transaction
    {
        public int Id { get; set; }

        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;
        public DateTimeOffset Date { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int? RecurringTransactionId { get; set; }
        public RecurringTransaction? RecurringTransaction { get; set; }


    }
}
