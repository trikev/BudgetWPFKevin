using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using BudgetWPFKevin.ViewModels.Categories;
using System.Collections.ObjectModel;

namespace BudgetWPFKevin.ViewModels.Transactions
{

    // Vymodell för en återkommande transaktion
    public class RecurringTransactionItemVM : ViewModelBase, ITransactionVM
    {
        private readonly RecurringTransaction _recurringTransaction;

        public RecurringTransactionItemVM(RecurringTransaction recurringTransaction)
        {
            _recurringTransaction = recurringTransaction;
        }

        public int Id
        {
            get => _recurringTransaction.Id;
            set
            {
                if (_recurringTransaction.Id != value)
                {
                    _recurringTransaction.Id = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsNew));
                }
            }
        }

        public string Description
        {
            get => _recurringTransaction.Description;
            set
            {
                if (_recurringTransaction.Description != value)
                {
                    _recurringTransaction.Description = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName ?? _recurringTransaction.Category?.Name ?? string.Empty;
            set
            {
                if (_categoryName != value)
                {
                    _categoryName = value;
                    OnPropertyChanged();
                }
            }
        }




        public int CategoryId
        {
            get => _recurringTransaction.CategoryId;
            set
            {
                if (_recurringTransaction.CategoryId != value)
                {
                    _recurringTransaction.CategoryId = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CategoryName));
                }
            }
        }

        public decimal Amount
        {
            get => _recurringTransaction.Amount;
            set
            {
                if (_recurringTransaction.Amount != value)
                {
                    _recurringTransaction.Amount = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime StartDate
        {
            get => _recurringTransaction.StartDate.DateTime;
            set
            {
                var newValue = new DateTimeOffset(value);
                if (_recurringTransaction.StartDate != newValue)
                {
                    _recurringTransaction.StartDate = newValue;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Date)); 
                }
            }
        }

        public DateTime? EndDate
        {
            get => _recurringTransaction.EndDate?.DateTime;
            set
            {
                var newValue = value.HasValue ? new DateTimeOffset(value.Value) : (DateTimeOffset?)null;
                if (_recurringTransaction.EndDate != newValue)
                {
                    _recurringTransaction.EndDate = newValue;
                    OnPropertyChanged();
                }
            }
        }
        public RecurrenceType RecurrenceType
        {
            get => _recurringTransaction.RecurrenceType;
            set
            {
                if (_recurringTransaction.RecurrenceType != value)
                {
                    _recurringTransaction.RecurrenceType = value;
                    OnPropertyChanged();

                }
            }
        }

        public int? RecurrenceMonth
        {
            get => _recurringTransaction.Month;
            set
            {
                if (_recurringTransaction.Month != value)
                {
                    _recurringTransaction.Month = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? Month
        {
            get => RecurrenceMonth;
            set => RecurrenceMonth = value;
        }

        public TransactionType Type
        {
            get => _recurringTransaction.Type;
            set
            {
                if (_recurringTransaction.Type != value)
                {
                    _recurringTransaction.Type = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsIncome));
                    OnPropertyChanged(nameof(IsExpense));
                    OnPropertyChanged(nameof(DisplayType));
                }
            }
        }

        public string DisplayType => Type == TransactionType.Income ? "Inkomst" : "Utgift";

        public bool IsIncome
        {
            get => Type == TransactionType.Income;
            set
            {
                if (value && Type != TransactionType.Income)
                {
                    Type = TransactionType.Income;
                }
            }
        }

        public bool IsExpense
        {
            get => Type == TransactionType.Expense;
            set
            {
                if (value && Type != TransactionType.Expense)
                {
                    Type = TransactionType.Expense;
                }
            }
        }

        public bool IsNew => _recurringTransaction.Id == 0;

        public DateTime Date
        {
            get => StartDate;
            set => StartDate = value;
        }

      
    }
}