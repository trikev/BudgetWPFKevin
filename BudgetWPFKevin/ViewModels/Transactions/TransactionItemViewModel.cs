using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using BudgetWPFKevin.ViewModels.Categories;
using System.Collections.ObjectModel;

namespace BudgetWPFKevin.ViewModels.Transactions
{

    // Vymodell för en transaktion

    public class TransactionItemViewModel : ViewModelBase, ITransactionVM
    {
        private readonly Transaction _transaction;

        public int? RecurringTransactionId => _transaction.RecurringTransactionId;
        public bool IsFromRecurring => RecurringTransactionId.HasValue;

        public TransactionItemViewModel(Transaction transaction)
        {
            _transaction = transaction;
        }

        public int Id
        {
            get => _transaction.Id;
            set
            {
                if (_transaction.Id != value)
                {
                    _transaction.Id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => _transaction.Description;
            set
            {
                if (_transaction.Description != value)
                {
                    _transaction.Description = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _categoryName;
        public string CategoryName
        {
            get => _categoryName ?? _transaction.Category?.Name ?? string.Empty;
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
            get => _transaction.CategoryId;
            set
            {
                if (_transaction.CategoryId != value)
                {
                    _transaction.CategoryId = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CategoryName));
                }
            }
        }

        public decimal Amount
        {
            get => _transaction.Amount;
            set
            {
                if (_transaction.Amount != value)
                {
                    _transaction.Amount = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime Date
        {
            get => _transaction.Date.DateTime;
            set
            {
                if (_transaction.Date.DateTime != value)
                {
                    _transaction.Date = new DateTimeOffset(value);
                    OnPropertyChanged();
                }
            }
        }

        public TransactionType Type
        {
            get => _transaction.Type;
            set
            {
                if (_transaction.Type != value)
                {
                    _transaction.Type = value;
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

        public bool IsNew => _transaction.Id == 0;


    }
}