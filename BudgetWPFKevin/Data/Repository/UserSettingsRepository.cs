using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetWPFKevin.Data.Repository
{
    public class UserSettingsRepository : IUserSettingsRepository
    {
        private readonly BudgetContext _context;

        public UserSettingsRepository(BudgetContext context)
        {
            _context = context;
        }

        public async Task<UserSettings?> GetUserSettingsAsync()
        {
            return await _context.UserSettings.FirstOrDefaultAsync();
        }

        public async Task SaveUserSettingsAsync(UserSettings settings)
        {
            var existing = await _context.UserSettings.FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.YearlyIncome = settings.YearlyIncome;
                existing.YearlyWorkHours = settings.YearlyWorkHours;
                _context.UserSettings.Update(existing);
            }
            else
            {
                await _context.UserSettings.AddAsync(settings);
            }

            await _context.SaveChangesAsync();
        }
    }
}