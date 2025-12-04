using BudgetWPFKevin.Commands;
using BudgetWPFKevin.Models;
using BudgetWPFKevin.ViewModels.Categories;
using System.Collections.ObjectModel;
using System.Globalization;
namespace BudgetWPFKevin.ViewModels.Transactions
{
    // ViewModel för att skapa eller redigera en transaktion (vanlig eller återkommande)
    public class NewTransactionVM : ViewModelBase
    {
        private static readonly CultureInfo SvCulture = CultureInfo.GetCultureInfo("sv-SE");
        private bool _convertToRecurring = false;
        private bool _convertToRegular = false;
        private RecurrenceType _pendingRecurrenceType = RecurrenceType.Monthly;
        private int? _pendingRecurrenceMonth;
        private DateTime? _pendingEndDate;

        public TransactionItemViewModel? Transaction { get; }
        public RecurringTransactionItemVM? RecurringTransaction { get; }
        public CategoryListVM CategoryListVM { get; }
        public IReadOnlyList<string> MonthNames { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public Action<bool?>? CloseAction { get; set; }



        private bool IsRecurringMode => RecurringTransaction != null && Transaction == null;
        private bool IsRegularMode => Transaction != null && RecurringTransaction == null;
        public bool IsYearlyRecurrence => RecurrenceType == RecurrenceType.Yearly;
        public bool ShowRecurringOptions => MakeRecurring;
        public bool ShouldConvertToRecurring => IsRegularMode && _convertToRecurring;
        public bool ShouldConvertToRegular => IsRecurringMode && _convertToRegular;

        public ObservableCollection<CategoryVM> CurrentCategories
        {
            get
            {
                var appliesTo = IsIncome ? TransactionType.Income : TransactionType.Expense;
                var filtered = CategoryListVM.Categories.Where(c => c.AppliesTo == appliesTo).ToList();
                return new ObservableCollection<CategoryVM>(filtered);
            }
        }
        public string WindowTitle
        {
            get
            {
                if (Transaction != null)
                    return Transaction.IsNew ? "Ny transaktion" : "Redigera transaktion";
                if (RecurringTransaction != null)
                    return RecurringTransaction.IsNew ? "Ny återkommande transaktion" : "Redigera återkommande transaktion";
                return "Transaktion";
            }
        }
        public string SaveButtonText
        {
            get
            {
                if (Transaction != null)
                    return Transaction.IsNew ? "Spara" : "Uppdatera";
                if (RecurringTransaction != null)
                    return RecurringTransaction.IsNew ? "Spara" : "Uppdatera";
                return "Spara";
            }
        }
        public string Description
        {
            get => Transaction?.Description ?? RecurringTransaction?.Description ?? string.Empty;
            set
            {
                if (Transaction != null)
                {
                    Transaction.Description = value;
                }
                else if (RecurringTransaction != null)
                {
                    RecurringTransaction.Description = value;
                }

                OnPropertyChanged();
            }
        }
        public decimal Amount
        {
            get => Transaction?.Amount ?? RecurringTransaction?.Amount ?? 0;
            set
            {
                if (Transaction != null)
                {
                    Transaction.Amount = value;
                }
                else if (RecurringTransaction != null)
                {
                    RecurringTransaction.Amount = value;
                }
                OnPropertyChanged();
            }
        }
        public DateTime Date
        {
            get => Transaction?.Date ?? RecurringTransaction?.StartDate ?? DateTime.Today;
            set
            {
                if (Transaction != null)
                {
                    Transaction.Date = value;
                }
                if (RecurringTransaction != null)
                {
                    RecurringTransaction.StartDate = value;
                }
                OnPropertyChanged();
            }
        }
        public DateTime? EndDate
        {
            get
            {
                if (IsRecurringMode)
                    return RecurringTransaction?.EndDate;
                if (IsRegularMode && _convertToRecurring)
                    return _pendingEndDate;
                return null;
            }
            set
            {
                if (IsRecurringMode && RecurringTransaction != null)
                {
                    RecurringTransaction.EndDate = value;
                }
                else if (IsRegularMode)
                {
                    _pendingEndDate = value;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(EndDateDisplay));
            }
        }
        public int CategoryId
        {
            get => Transaction?.CategoryId ?? RecurringTransaction?.CategoryId ?? 0;
            set
            {
                if (Transaction != null)
                {
                    Transaction.CategoryId = value;
                }
                else if (RecurringTransaction != null)
                {
                    RecurringTransaction.CategoryId = value;
                }
                OnPropertyChanged();
            }
        }
        public bool IsIncome
        {
            get => Transaction?.IsIncome ?? (RecurringTransaction?.Type == TransactionType.Income);
            set
            {
                if (Transaction != null)
                {
                    Transaction.IsIncome = value;

                }
                if (RecurringTransaction != null)
                {
                    RecurringTransaction.Type = value ? TransactionType.Income : TransactionType.Expense;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsExpense));
                OnPropertyChanged(nameof(CurrentCategories));

                SelectFirstCategory();
            }
        }

        public bool IsExpense
        {
            get => Transaction?.IsExpense ?? (RecurringTransaction?.Type == TransactionType.Expense);
            set
            {
                if (Transaction != null)
                {
                    Transaction.IsExpense = value;

                }
                if (RecurringTransaction != null)
                {
                    RecurringTransaction.Type = value ? TransactionType.Expense : TransactionType.Income;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsIncome));
                OnPropertyChanged(nameof(CurrentCategories));

                SelectFirstCategory();
            }
        }

        private void SelectFirstCategory()
        {
            if (CurrentCategories != null && CurrentCategories.Any())
            {
                var firstCategory = CurrentCategories.First();
                CategoryId = firstCategory.Id;

                // Uppdatera även Transaction/RecurringTransaction objektet
                if (Transaction != null)
                {
                    Transaction.CategoryId = firstCategory.Id;

                }
                if (RecurringTransaction != null)
                {
                    RecurringTransaction.CategoryId = firstCategory.Id;

                }
                OnPropertyChanged(nameof(CategoryId));
            }
        }

        public bool MakeRecurring
        {
            get
            {
                if (IsRecurringMode)
                    return !_convertToRegular;

                if (IsRegularMode)
                    return _convertToRecurring;

                return false;
            }
            set
            {
                bool hasChanged = false;

                if (IsRecurringMode && _convertToRegular == value)
                {
                    _convertToRegular = !value;
                    hasChanged = true;
                }
                else if (IsRegularMode && _convertToRecurring != value)
                {
                    _convertToRecurring = value;
                    hasChanged = true;
                }
                if (hasChanged)
                {
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ShowRecurringOptions));
                }
            }
        }




        public RecurrenceType RecurrenceType
        {
            get
            {
                if (IsRecurringMode)
                    return RecurringTransaction?.RecurrenceType ?? RecurrenceType.Monthly;
                if (IsRegularMode && _convertToRecurring)
                    return _pendingRecurrenceType;
                return RecurrenceType.Monthly;
            }
            set
            {
                if (IsRecurringMode && RecurringTransaction != null)
                {
                    RecurringTransaction.RecurrenceType = value;
                }
                else if (IsRegularMode)
                {
                    _pendingRecurrenceType = value;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsYearlyRecurrence));
            }
        }
        public string EndDateDisplay
        {
            get
            {
                if (MakeRecurring)
                {
                    return EndDate?.ToString("yyyy-MM-dd") ?? "Pågående";
                }
                return Date.ToString("yyyy-MM-dd");
            }
        }



        private string _selectedMonthName;
        public string SelectedMonthName
        {
            get => _selectedMonthName;
            set
            {
                if (_selectedMonthName != value && !string.IsNullOrWhiteSpace(value))
                {
                    _selectedMonthName = value;
                    OnPropertyChanged();
                    UpdateRecurrenceMonthFromUI();
                }
            }
        }
        public int? RecurrenceMonth
        {
            get
            {
                if (IsRecurringMode)
                    return RecurringTransaction?.RecurrenceMonth;
                if (IsRegularMode && _convertToRecurring)
                    return _pendingRecurrenceMonth;
                return null;
            }
            set
            {
                if (IsRecurringMode && RecurringTransaction != null)
                {
                    RecurringTransaction.RecurrenceMonth = value;
                }
                else if (IsRegularMode)
                {
                    _pendingRecurrenceMonth = value;
                }
                OnPropertyChanged();
            }
        }



        // Metod för att hämta data för återkommande transaktion
        public RecurringTransactionData? GetRecurringData()
        {
            if (!ShouldConvertToRecurring || Transaction == null)
                return null;
            return new RecurringTransactionData
            {
                RecurrenceType = _pendingRecurrenceType,
                RecurrenceMonth = _pendingRecurrenceMonth,
                EndDate = _pendingEndDate
            };
        }
        public NewTransactionVM(
            TransactionItemViewModel? transaction,
            RecurringTransactionItemVM? recurringTransaction,
            CategoryListVM categoryListVM)
        {
            Transaction = transaction;
            RecurringTransaction = recurringTransaction;
            CategoryListVM = categoryListVM;
            MonthNames = SvCulture.DateTimeFormat.MonthNames
                .Where(m => !string.IsNullOrEmpty(m))
                .Select(m => char.ToUpper(m[0]) + m.Substring(1))
                .ToList();
            if (IsRecurringMode && RecurringTransaction != null)
            {
                var currentMonth = RecurringTransaction.RecurrenceMonth;
                if (currentMonth.HasValue && currentMonth.Value >= 1 && currentMonth.Value <= 12)
                {
                    _selectedMonthName = MonthNames[currentMonth.Value - 1];
                }
            }
            SaveCommand = new DelegateCommand(OnSave);
            CancelCommand = new DelegateCommand(OnCancel);
        }
        // Uppdatera RecurrenceMonth baserat på användarens val i UI
        private void UpdateRecurrenceMonthFromUI()
        {
            if (string.IsNullOrWhiteSpace(SelectedMonthName))
                return;
            int monthNumber = DateTime.ParseExact(
                SelectedMonthName,
                "MMMM",
                SvCulture,
                DateTimeStyles.None).Month;
            RecurrenceMonth = monthNumber;
        }



        private void OnSave(object? parameter)
        {
            CloseAction?.Invoke(true);
        }
        private void OnCancel(object? parameter)
        {
            CloseAction?.Invoke(false);
        }
    }


    public class RecurringTransactionData
    {
        public RecurrenceType RecurrenceType { get; set; }
        public int? RecurrenceMonth { get; set; }
        public DateTime? EndDate { get; set; }
    }
}