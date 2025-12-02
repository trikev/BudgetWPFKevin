using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetWPFKevin.Data.Repository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly BudgetContext _context;

        public TransactionRepository(BudgetContext context)
        {
            _context = context;
        }

        public async Task<List<Transaction>> GetByMonthAsync(DateTimeOffset month)
        {
            var first = new DateTimeOffset(month.Year, month.Month, 1, 0, 0, 0, month.Offset);
            var last = first.AddMonths(1);

            return await _context.Transactions
                 .AsNoTracking()
                .Include(t => t.Category)
                .Where(t => t.Date >= first && t.Date < last)
                .ToListAsync();
        }

        public async Task AddAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int transactionId)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(c => c.Id == transactionId);

            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            if (transaction == null)
                return;

            var existing = await _context.Transactions.FindAsync(transaction.Id);
            if (existing == null)
                return;

            _context.Entry(existing).CurrentValues.SetValues(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Transaction>> GetAllAsync()
        {
            return await _context.Transactions
                 .AsNoTracking()
                .Include(t => t.Category)
                .OrderBy(t => t.Category.Name)
                .ThenBy(t => t.Date)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetAllIncomes()
        {
            return await _context.Transactions
                 .AsNoTracking()
                .Include(t => t.Category) 
                .Where(t => t.Type == TransactionType.Income)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetAllExpenses()
        {
            return await _context.Transactions
                 .AsNoTracking()
                .Include(t => t.Category) 
                .Where(t => t.Type == TransactionType.Expense)
                .ToListAsync();
        }

        public async Task<Transaction> GetByIdAsync(int transactionId)
        {
            return await _context.Transactions
                 .AsNoTracking()
                .Include(t => t.Category) 
                .FirstOrDefaultAsync(c => c.Id == transactionId);
        }
    }
}