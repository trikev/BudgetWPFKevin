using BudgetWPFKevin.Models;

namespace BudgetWPFKevin.ViewModels.Categories
{

    // ViewModel för en kategori

    public class CategoryVM : ViewModelBase
    {
        private readonly Category _category;

        public CategoryVM(Category category)
        {
            _category = category;
        }

        public int Id
        {
            get => _category.Id;
            set
            {
                if (_category.Id != value)
                {
                    _category.Id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _category.Name;
            set
            {
                if (_category.Name != value)
                {
                    _category.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public TransactionType AppliesTo
        {
            get => _category.AppliesTo;
            set
            {
                if(_category.AppliesTo != value)
                {
                    _category.AppliesTo = value;
                    OnPropertyChanged();
                }
            }
        }



        public Category ToModel() => _category;
    }
}
