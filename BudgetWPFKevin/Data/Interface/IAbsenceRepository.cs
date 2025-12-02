using BudgetWPFKevin.Models;

namespace BudgetWPFKevin.Data.Interface
{
    public interface IAbsenceRepository
    {
        Task<List<AbsenceRecord>> GetAllAsync();
        Task<List<AbsenceRecord>> GetByMonthAsync(DateTime month);
        Task<AbsenceRecord?> GetByIdAsync(int id);
        Task AddAsync(AbsenceRecord absence);
        Task UpdateAsync(AbsenceRecord absence);
        Task DeleteAsync(int id);
    }
}