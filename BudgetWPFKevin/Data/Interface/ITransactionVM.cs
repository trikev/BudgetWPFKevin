using BudgetWPFKevin.Models;

namespace BudgetWPFKevin.Data.Interface
{
    public interface ITransactionVM
    {
        int Id { get; }
        string Description { get; set; }
        string CategoryName { get; }
        int CategoryId { get; set; }
        decimal Amount { get; set; }
        DateTime Date { get; set; }
        TransactionType Type { get; set; }
        bool IsIncome { get; }
        bool IsExpense { get; }
        
    }
}