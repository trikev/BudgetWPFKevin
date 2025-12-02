using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using BudgetWPFKevin.ViewModels.Categories;
using System.Collections.ObjectModel;

namespace BudgetWPFKevin.ViewModels
{

    // Vymodell för att hantera en lista av kategorier
    public class CategoryListVM : ViewModelBase
    {
        private readonly ICategoryRepository _repository;
        public ObservableCollection<CategoryVM> Categories { get; }


        // Filtrerade vyer för inkomst- och utgiftskategorier

        public IEnumerable<CategoryVM> IncomeCategories =>
            Categories.Where(c => c.AppliesTo == TransactionType.Income);

        public IEnumerable<CategoryVM> ExpenseCategories =>
            Categories.Where(c => c.AppliesTo == TransactionType.Expense);


        private CategoryVM _selectedCategory;
        public CategoryVM SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public CategoryListVM(ICategoryRepository repository)
        {
            _repository = repository;

            Categories = new ObservableCollection<CategoryVM>();
            
        }


        // Laddar kategorier från repo
        public async Task LoadCategoriesAsync()
        {
            try
            {
                IsLoading = true;

                Categories.Clear();
        

                var categoriesFromRepo = await _repository.GetAllCategoriesAsync();

                foreach (var category in categoriesFromRepo)
                {
                    var categoryVM = new CategoryVM(category);
                    Categories.Add(categoryVM);

                    
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

       

       
    }
}