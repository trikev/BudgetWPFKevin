using BudgetWPFKevin.Models;

namespace BudgetWPFKevin.Data.Interface
{
    public interface IUserSettingsRepository
    {
        Task<UserSettings?> GetUserSettingsAsync();
        Task SaveUserSettingsAsync(UserSettings settings);
    }
}