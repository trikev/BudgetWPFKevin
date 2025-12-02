using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetWPFKevin.Data.Repository
{
    public class RecurringTransactionRepository : IRecurringTransaction
    {
        private readonly BudgetContext _context;

        public RecurringTransactionRepository(BudgetContext context)
        {
            _context = context;
        }

        public async Task AddRecurringTransactionAsync(RecurringTransaction recurringTransaction)
        {
            _context.RecurringTransactions.Add(recurringTransaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRecurringTransactionAsync(int recurringTransactionId)
        {
            var category = await _context.RecurringTransactions
                .FirstOrDefaultAsync(c => c.Id == recurringTransactionId);

            if (category != null)
            {
                _context.RecurringTransactions.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<RecurringTransaction>> GetAllRecurringTransactionsAsync()
        {
            return await _context.RecurringTransactions
                .AsNoTracking()
                .Include(c => c.Category)
                .OrderBy(c => c.StartDate)
                .ToListAsync();
        }

        public async Task<RecurringTransaction> GetRecurringTransactionByIdAsync(int recurringTransactionId)
        {
            return await _context.RecurringTransactions
                 .AsNoTracking()
                .Include(r => r.Category)
                .FirstOrDefaultAsync(c => c.Id == recurringTransactionId);
        }

        public async Task UpdateRecurringTransactionAsync(RecurringTransaction recurringTransaction)
        {
            if (recurringTransaction == null)
                return;

            var existing = await _context.RecurringTransactions.FindAsync(recurringTransaction.Id);
            if (existing == null)
                return;

            _context.Entry(existing).CurrentValues.SetValues(recurringTransaction);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RecurringTransaction>> GetByMonthAsync(DateTimeOffset month)
        {
            var firstDay = new DateTime(month.Year, month.Month, 1);
            var firstDayNextMonth = firstDay.AddMonths(1);
            var currentMonth = month.Month;

            var allRecurring = await _context.RecurringTransactions
                 .AsNoTracking()
                .Include(r => r.Category)
                .ToListAsync();

            var activeInMonth = allRecurring.Where(r =>
            {
                var start = r.StartDate.Date;
                var end = r.EndDate?.Date;

                bool isActive;
                if (end == null)
                    isActive = start < firstDayNextMonth;
                else
                    isActive = start < firstDayNextMonth && end.Value >= firstDay;

                if (!isActive)
                    return false;

                if (r.RecurrenceType == RecurrenceType.Yearly)
                {
                    return r.Month == currentMonth;
                }

                return true;
            }).ToList();

            return activeInMonth;
        }
    }
}