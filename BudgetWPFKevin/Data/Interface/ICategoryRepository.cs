using BudgetWPFKevin.Models;

namespace BudgetWPFKevin.Data.Interface
{
    public interface ICategoryRepository
    {
        Task AddCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int categoryId);
        Task<List<Category>> GetAllCategoriesAsync();
        
    }
}
