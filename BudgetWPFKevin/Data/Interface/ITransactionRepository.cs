using BudgetWPFKevin.Models;

namespace BudgetWPFKevin.Data.Interface
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task DeleteAsync(int id);
        Task<List<Transaction>> GetByMonthAsync(DateTimeOffset month);
        Task<List<Transaction>> GetAllAsync();
        Task<List<Transaction>> GetAllIncomes();
        Task<List<Transaction>> GetAllExpenses();
        Task<Transaction> GetByIdAsync(int transactionId);
    }
}
