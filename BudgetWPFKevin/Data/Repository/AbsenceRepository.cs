using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetWPFKevin.Data.Repository
{
    public class AbsenceRepository : IAbsenceRepository
    {
        private readonly BudgetContext _context;

        public AbsenceRepository(BudgetContext context)
        {
            _context = context;
        }

        public async Task<List<AbsenceRecord>> GetAllAsync()
        {
            return await _context.AbsenceRecords.ToListAsync();
        }

        public async Task<List<AbsenceRecord>> GetByMonthAsync(DateTime month)
        {
            var firstDay = new DateTime(month.Year, month.Month, 1);
            var lastDay = firstDay.AddMonths(1);

            return await _context.AbsenceRecords
                .Where(a => a.Date >= firstDay && a.Date < lastDay)
                .OrderBy(a => a.Date)
                .ToListAsync();
        }

        public async Task<AbsenceRecord?> GetByIdAsync(int id)
        {
            return await _context.AbsenceRecords.FindAsync(id);
        }

        public async Task AddAsync(AbsenceRecord absence)
        {
            await _context.AbsenceRecords.AddAsync(absence);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AbsenceRecord absence)
        {
            _context.AbsenceRecords.Update(absence);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var absence = await GetByIdAsync(id);
            if (absence != null)
            {
                _context.AbsenceRecords.Remove(absence);
                await _context.SaveChangesAsync();
            }
        }
    }
}