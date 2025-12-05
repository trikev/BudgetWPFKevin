using BudgetWPFKevin.Commands;
using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;

namespace BudgetWPFKevin.ViewModels
{

    // ViewModel för användarinställningar
    public class UserSettingsViewModel : ViewModelBase
    {
        private readonly IUserSettingsRepository _userSettingsRepository;
        private readonly IRecurringTransaction _recurringTransactionRepository;
        private readonly ICategoryRepository _categoryRepository;


        private decimal _yearlyIncome;
        public decimal YearlyIncome
        {
            get => _yearlyIncome;
            set
            {
                _yearlyIncome = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(YearlyIncomeAfterTax));
                OnPropertyChanged(nameof(MonthlyIncomeAfterTax));
                ValidateInput();
            }
        }

        private decimal _taxRate = 0.32m;
        public decimal TaxRate
        {
            get => _taxRate;
            set
            {
                _taxRate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(YearlyIncomeAfterTax));
                OnPropertyChanged(nameof(MonthlyIncomeAfterTax));
            }
        }


        private int _yearlyWorkHours;
        public int YearlyWorkHours
        {
            get => _yearlyWorkHours;
            set
            {
                _yearlyWorkHours = value;
                OnPropertyChanged();
                ValidateInput();
            }
        }

        private string _validationError;
        public string ValidationError
        {
            get => _validationError;
            set
            {
                _validationError = value;
                OnPropertyChanged();
            }
        }


        private bool _isEditing = false;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
            }
        }


        public decimal YearlyIncomeAfterTax => YearlyIncome * (1 - TaxRate);
        public decimal MonthlyIncomeAfterTax => YearlyIncomeAfterTax / 12;

        public bool IsValid { get; private set; }

        public DelegateCommand SaveCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand ToggleEditCommand { get; }

        public Action<bool?>? CloseAction { get; set; } 

        public UserSettingsViewModel(
            IUserSettingsRepository userSettingsRepository,
            IRecurringTransaction recurringTransactionRepository,
            ICategoryRepository categoryRepository)
        {
            _userSettingsRepository = userSettingsRepository;
            _recurringTransactionRepository = recurringTransactionRepository;
            _categoryRepository = categoryRepository;

            SaveCommand = new DelegateCommand(async _ => await SaveAsync(), CanSave);
            CancelCommand = new DelegateCommand(Cancel);
            ToggleEditCommand = new DelegateCommand(ToggleEdit);

            YearlyWorkHours = 1920;
        }

        public async Task LoadAsync()
        {
            var userSettings = await _userSettingsRepository.GetUserSettingsAsync();

            if (userSettings != null)
            {
                YearlyIncome = userSettings.YearlyIncome;
                YearlyWorkHours = userSettings.YearlyWorkHours;
                TaxRate = userSettings.TaxRate;
            }
        }

        private void ToggleEdit(object parameter)
        {
            IsEditing = !IsEditing;
        }

        private void ValidateInput()
        {
            ValidationError = string.Empty;
            IsValid = true;

            if (YearlyIncome <= 0)
            {
                ValidationError = "Årsinkomst måste vara större än 0";
                IsValid = false;
            }
            else if (YearlyWorkHours <= 0 || YearlyWorkHours > 8760)
            {
                ValidationError = "Arbetstimmar måste vara mellan 1 och 8760 (24*365)";
                IsValid = false;
            }

            SaveCommand.RaiseCanExecuteChanged();
        }

        private bool CanSave(object parameter)
        {
            return IsValid && YearlyIncome > 0 && YearlyWorkHours > 0;
        }

        private async Task SaveAsync()
        {
            var settings = new UserSettings
            {
                YearlyIncome = YearlyIncome,
                YearlyWorkHours = YearlyWorkHours,
                TaxRate = TaxRate,
            };

            await _userSettingsRepository.SaveUserSettingsAsync(settings);

            await UpdateSystemSalaryTransactionAsync();

            CloseAction?.Invoke(true);
        }


        // Uppdatera, ta bort gamla och lägg till en ny systemgenererad lönepost
        private async Task UpdateSystemSalaryTransactionAsync()
        {
            var recurringTransactions = await _recurringTransactionRepository
                .GetAllRecurringTransactionsAsync();

            var systemTransaction = recurringTransactions
                .FirstOrDefault(r => r.IsSystemGenerated);

            if (systemTransaction != null)
            {
                await _recurringTransactionRepository
                    .DeleteRecurringTransactionAsync(systemTransaction.Id);
            }

            var categories = await _categoryRepository.GetAllCategoriesAsync();
            var salaryCategory = categories.FirstOrDefault(c => c.Name == "Lön");

            if (salaryCategory == null)
                return;

            var recurringTransaction = new RecurringTransaction
            {
                Amount = MonthlyIncomeAfterTax,
                CategoryId = salaryCategory.Id,
                Category = salaryCategory,
                IsSystemGenerated = true,
                Description = "Månadslön",
                StartDate = DateTime.Now,
                EndDate = null,
                IsRecurring = true,
                RecurrenceType = RecurrenceType.Monthly,
                Type = TransactionType.Income,
            };

            await _recurringTransactionRepository
                .AddRecurringTransactionAsync(recurringTransaction);
        }

        private void Cancel(object parameter)
        {
            CloseAction?.Invoke(false);
        }

    }
}