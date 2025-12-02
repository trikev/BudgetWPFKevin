using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetWPFKevin.Data.Repository
{

    public class CategoryRepository : ICategoryRepository
    {
        private readonly BudgetContext _context;

        public CategoryRepository(BudgetContext context)
        {
            _context = context;
        }

        public async Task AddCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.Transactions)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            if (category == null)
                return;

            var existing = await _context.Categories.FindAsync(category.Id);

            if (existing == null)
                return;

            _context.Entry(existing).CurrentValues.SetValues(category);
            await _context.SaveChangesAsync();
        }
    }
}