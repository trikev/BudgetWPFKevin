using BudgetWPFKevin.Models;

namespace BudgetWPFKevin.Data.Interface
{
    public interface IRecurringTransaction
    {
        Task AddRecurringTransactionAsync(RecurringTransaction recurringTransaction);
        Task UpdateRecurringTransactionAsync(RecurringTransaction recurringTransaction);
        Task DeleteRecurringTransactionAsync(int recurringTransactionId);
        Task<List<RecurringTransaction>> GetAllRecurringTransactionsAsync();
        Task<RecurringTransaction> GetRecurringTransactionByIdAsync(int recurringTransactionId);
        Task<List<RecurringTransaction>> GetByMonthAsync(DateTimeOffset month);
    }
}
