using AutoMapper;
using BudgetWPFKevin.Commands;
using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using BudgetWPFKevin.ViewModels.Categories;
using System.ComponentModel;

namespace BudgetWPFKevin.ViewModels.Transactions
{
    public class TransactionCoordinatorVM : ViewModelBase
    {
        private readonly CategoryListVM _categoryListVM;
        private readonly IMapper _mapper;

        public TransactionListVM RegularTransactions { get; }
        public RecurringTransactionListVM RecurringTransactions { get; }

        private ITransactionVM _selectedTransaction;
        public ITransactionVM SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                if (_selectedTransaction != value)
                {
                    _selectedTransaction = value;
                    OnPropertyChanged();
                    DeleteCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public DelegateCommand DeleteCommand { get; }

        public TransactionCoordinatorVM(
            TransactionListVM regularTransactions,
            RecurringTransactionListVM recurringTransactions,
            CategoryListVM categoryListVM,
            IMapper mapper)
        {
            RegularTransactions = regularTransactions;
            RecurringTransactions = recurringTransactions;
            _categoryListVM = categoryListVM;
            _mapper = mapper;

            RegularTransactions.PropertyChanged += OnRegularSelectionChanged;
            RecurringTransactions.PropertyChanged += OnRecurringSelectionChanged;

            DeleteCommand = new DelegateCommand(
                async _ => await DeleteSelectedAsync(),
                _ => SelectedTransaction != null);

           
        }

        private void OnRegularSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RegularTransactions.SelectedTransaction))
            {
                SelectedTransaction = RegularTransactions.SelectedTransaction;

                // Rensa recurring selection när vi väljer en regular transaction
                if (SelectedTransaction != null)
                {
                    RecurringTransactions.SelectedRecurring = null;
                }
            }
        }

        private void OnRecurringSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RecurringTransactions.SelectedRecurring))
            {
                SelectedTransaction = RecurringTransactions.SelectedRecurring;

                // Rensa regular selection när vi väljer en recurring transaction
                if (SelectedTransaction != null)
                {
                    RegularTransactions.SelectedTransaction = null;
                }
            }
        }

        public async Task LoadForMonthAsync(DateTime month)
        {
            await RegularTransactions.LoadForMonthAsync(month);
            await RecurringTransactions.LoadForMonthAsync(month);
            CreateMirrorTransactions(month);
        }

        public async Task DeleteSelectedAsync()
        {
            if (SelectedTransaction == null) return;

            if (SelectedTransaction is TransactionItemViewModel item)
            {
                if (item.IsFromRecurring)
                {
                    var recurring = RecurringTransactions.RecurringTransactions
                        .FirstOrDefault(r => r.Id == item.RecurringTransactionId);

                    if (recurring != null)
                    {
                        await RecurringTransactions.DeleteAsync(recurring);
                        RemoveAllMirrorTransactions(recurring.Id);
                    }
                }
                else
                {
                    await RegularTransactions.DeleteAsync(item);
                }
            }
            else if (SelectedTransaction is RecurringTransactionItemVM recurring)
            {
                await RecurringTransactions.DeleteAsync(recurring);
                RemoveAllMirrorTransactions(recurring.Id);
            }

            SelectedTransaction = null;
        }

        public async Task UpdateTransactionAsync(ITransactionVM editedVM, DateTime selectedMonth)
        {
            if (editedVM is TransactionItemViewModel editedItem)
            {
                var original = RegularTransactions.Incomes
                    .Concat(RegularTransactions.Expenses)
                    .FirstOrDefault(t => t.Id == editedItem.Id);

                if (original != null)
                {
                    CopyTransactionValues(editedItem, original);
                    await RegularTransactions.UpdateAsync(original);
                }
            }
            else if (editedVM is RecurringTransactionItemVM editedRecurring)
            {
                var original = RecurringTransactions.RecurringTransactions
                    .FirstOrDefault(r => r.Id == editedRecurring.Id);

                if (original != null)
                {
                    CopyRecurringValues(editedRecurring, original);
                    await RecurringTransactions.UpdateAsync(original);

                    RemoveAllMirrorTransactions(original.Id);
                    if (original.Type == TransactionType.Income &&
                        RecurringTransactions.ShouldShowInMonth(original, selectedMonth))
                    {
                        var mirror = CreateMirrorTransaction(original, selectedMonth);
                        RegularTransactions.Add(mirror);
                    }
                }
            }
        }

        public async Task SaveAsRecurringTransactionAsync(
            TransactionItemViewModel transactionVM,
            RecurringTransactionData recurringData,
            DateTime selectedMonth)
        {
            var recurring = new RecurringTransaction
            {
                Type = transactionVM.Type,
                Amount = transactionVM.Amount,
                Description = transactionVM.Description,
                CategoryId = transactionVM.CategoryId,
                StartDate = transactionVM.Date,
                EndDate = recurringData.EndDate.HasValue
                    ? new DateTimeOffset(recurringData.EndDate.Value)
                    : (DateTimeOffset?)null,
                RecurrenceType = recurringData.RecurrenceType,
                Month = recurringData.RecurrenceMonth,
                IsRecurring = true
            };

            var recurringVM = new RecurringTransactionItemVM(recurring);
            await RecurringTransactions.SaveAsync(recurringVM);

            if (RecurringTransactions.ShouldShowInMonth(recurringVM, selectedMonth))
            {
                SelectedTransaction = recurringVM;

                if (recurring.Type == TransactionType.Income)
                {
                    var mirror = CreateMirrorTransaction(recurringVM, selectedMonth);
                    RegularTransactions.Add(mirror);
                }
            }
        }

        public async Task ConvertToRecurringAsync(
            TransactionItemViewModel item,
            RecurringTransactionData recurringData,
            DateTime selectedMonth)
        {
            var recurring = new RecurringTransaction
            {
                Type = item.Type,
                Amount = item.Amount,
                Description = item.Description,
                CategoryId = item.CategoryId,
                StartDate = item.Date,
                EndDate = recurringData.EndDate.HasValue
                    ? new DateTimeOffset(recurringData.EndDate.Value)
                    : (DateTimeOffset?)null,
                RecurrenceType = recurringData.RecurrenceType,
                Month = recurringData.RecurrenceMonth,
                IsRecurring = true
            };

            var recurringVM = new RecurringTransactionItemVM(recurring);
            await RecurringTransactions.SaveAsync(recurringVM);

            await RegularTransactions.DeleteAsync(item);

            if (RecurringTransactions.ShouldShowInMonth(recurringVM, selectedMonth))
            {
                SelectedTransaction = recurringVM;

                if (recurring.Type == TransactionType.Income)
                {
                    var mirror = CreateMirrorTransaction(recurringVM, selectedMonth);
                    RegularTransactions.Add(mirror);
                }
            }
            else
            {
                SelectedTransaction = null;
            }
        }

        public async Task ConvertToRegularAsync(RecurringTransactionItemVM rItem)
        {
            await RecurringTransactions.DeleteAsync(rItem);
            RemoveAllMirrorTransactions(rItem.Id);

            var transaction = _mapper.Map<Transaction>(rItem);
            var newVM = new TransactionItemViewModel(transaction);
            await RegularTransactions.SaveAsync(newVM);

            SelectedTransaction = newVM;
        }

        public TransactionItemViewModel CreateNewTransaction()
        {
            var newTransaction = new Transaction
            {
                Date = DateTimeOffset.Now,
                CategoryId = _categoryListVM.Categories?.FirstOrDefault()?.Id ?? 0,
                Type = TransactionType.Expense
            };

            return new TransactionItemViewModel(newTransaction);
        }

        // Skapa kopia för editering
        public ITransactionVM GetEditableTransaction(ITransactionVM transaction)
        {
            if (transaction is TransactionItemViewModel item)
            {
                if (item.IsFromRecurring)
                {
                    var recurrance = RecurringTransactions.RecurringTransactions
                        .FirstOrDefault(r => r.Id == item.RecurringTransactionId);

                    if (recurrance != null)
                    {
                        return CreateRecurringCopy(recurrance);
                    }
                }

                return CreateTransactionCopy(item);
            }

            if (transaction is RecurringTransactionItemVM recurring)
            {
                return CreateRecurringCopy(recurring);
            }

            return transaction;
        }

        // Skapa kopia av TransactionItemViewModel
        private TransactionItemViewModel CreateTransactionCopy(TransactionItemViewModel original)
        {
            var transaction = new Transaction
            {
                Id = original.Id,
                Description = original.Description,
                Amount = original.Amount,
                Date = new DateTimeOffset(original.Date),
                CategoryId = original.CategoryId,
                Type = original.Type,
                RecurringTransactionId = original.RecurringTransactionId
            };

            var copy = new TransactionItemViewModel(transaction);
            copy.CategoryName = original.CategoryName;

            return copy;
        }

        // Skapa kopia av RecurringTransactionItemVM
        private RecurringTransactionItemVM CreateRecurringCopy(RecurringTransactionItemVM original)
        {
            var recurring = new RecurringTransaction
            {
                Id = original.Id,
                Description = original.Description,
                Amount = original.Amount,
                StartDate = original.StartDate,
                EndDate = original.EndDate,
                CategoryId = original.CategoryId,
                Type = original.Type,
                RecurrenceType = original.RecurrenceType,
                Month = original.Month,
                IsRecurring = true
            };

            var copy = new RecurringTransactionItemVM(recurring);
            copy.CategoryName = original.CategoryName;

            return copy;
        }

        // Kopiera värden mellan TransactionItemViewModel
        private void CopyTransactionValues(TransactionItemViewModel from, TransactionItemViewModel to)
        {
            to.Description = from.Description;
            to.Amount = from.Amount;
            to.Date = from.Date;
            to.CategoryId = from.CategoryId;
            to.Type = from.Type;
        }

        // NY METOD - Kopiera värden mellan RecurringTransactionItemVM
        private void CopyRecurringValues(RecurringTransactionItemVM from, RecurringTransactionItemVM to)
        {
            to.Description = from.Description;
            to.Amount = from.Amount;
            to.StartDate = from.StartDate;
            to.EndDate = from.EndDate;
            to.CategoryId = from.CategoryId;
            to.Type = from.Type;
            to.RecurrenceType = from.RecurrenceType;
            to.Month = from.Month;
        }

        private void CreateMirrorTransactions(DateTime month)
        {
            var visibleRecurring = RecurringTransactions.GetVisibleForMonth(month);

            foreach (var recurring in visibleRecurring)
            {
                if (recurring.Type == TransactionType.Income)
                {
                    var mirror = CreateMirrorTransaction(recurring, month);
                    RegularTransactions.Add(mirror);
                }
            }
        }

        private TransactionItemViewModel CreateMirrorTransaction(
            RecurringTransactionItemVM recurringVM,
            DateTime month)
        {
            var firstDay = new DateTime(month.Year, month.Month, 1);
            var mirrorTransaction = _mapper.Map<Transaction>(recurringVM);
            mirrorTransaction.Date = firstDay;

            var category = _categoryListVM.Categories
                .FirstOrDefault(c => c.Id == recurringVM.CategoryId);

            mirrorTransaction.Category = new Category
            {
                Id = category?.Id ?? 0,
                Name = category?.Name ?? string.Empty
            };

            mirrorTransaction.RecurringTransactionId = recurringVM.Id;

            return new TransactionItemViewModel(mirrorTransaction);
        }

        private void RemoveAllMirrorTransactions(int recurringId)
        {
            var incomeMirrors = RegularTransactions.Incomes
                .Where(x => x.RecurringTransactionId == recurringId)
                .ToList();

            foreach (var mirror in incomeMirrors)
            {
                RegularTransactions.Remove(mirror);
            }

            var expenseMirrors = RegularTransactions.Expenses
                .Where(x => x.RecurringTransactionId == recurringId)
                .ToList();

            foreach (var mirror in expenseMirrors)
            {
                RegularTransactions.Remove(mirror);
            }
        }
    }
}