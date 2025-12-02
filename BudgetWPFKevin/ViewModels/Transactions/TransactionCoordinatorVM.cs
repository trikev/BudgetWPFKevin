using AutoMapper;
using BudgetWPFKevin.Commands;
using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using BudgetWPFKevin.ViewModels.Categories;
using System.ComponentModel;

namespace BudgetWPFKevin.ViewModels.Transactions
{
    // Koordinatorvymodell för att hantera både reguljära och återkommande transaktioner
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

            // Synka selection mellan listorna
            RegularTransactions.PropertyChanged += OnRegularSelectionChanged;
            RecurringTransactions.PropertyChanged += OnRecurringSelectionChanged;

            // Delete command
            DeleteCommand = new DelegateCommand(
                async _ => await DeleteSelectedAsync(),
                _ => SelectedTransaction != null);
        }

        // Hantera ändringar i valda transaktioner
        private void OnRegularSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RegularTransactions.SelectedTransaction))
            {
                SelectedTransaction = RegularTransactions.SelectedTransaction;
            }
        }

        // Hantera ändringar i valda återkommande transaktioner
        private void OnRecurringSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RecurringTransactions.SelectedRecurring))
            {
                SelectedTransaction = RecurringTransactions.SelectedRecurring;
            }
        }

        public async Task LoadForMonthAsync(DateTime month)
        {
            await RegularTransactions.LoadForMonthAsync(month);
            await RecurringTransactions.LoadForMonthAsync(month);

            // Skapa mirror transactions för recurring incomes
            CreateMirrorTransactions(month);
        }


        // Radera vald transaktion
        public async Task DeleteSelectedAsync()
        {
            if (SelectedTransaction == null) return;

            if (SelectedTransaction is TransactionItemViewModel item)
            {
                if (item.IsFromRecurring)
                {
                    // Hitta och ta bort recurring + alla mirrors
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

        public async Task UpdateTransactionAsync(ITransactionVM transactionVM, DateTime selectedMonth)
        {
            if (transactionVM is TransactionItemViewModel item)
            {
                await RegularTransactions.UpdateAsync(item);
            }
            else if (transactionVM is RecurringTransactionItemVM recurring)
            {
                await RecurringTransactions.UpdateAsync(recurring);

                // Uppdatera mirrors
                RemoveAllMirrorTransactions(recurring.Id);
                if (recurring.Type == TransactionType.Income &&
                    RecurringTransactions.ShouldShowInMonth(recurring, selectedMonth))
                {
                    var mirror = CreateMirrorTransaction(recurring, selectedMonth);
                    RegularTransactions.Add(mirror);
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

            // Ta bort reguljär
            await RegularTransactions.DeleteAsync(item);

            if (RecurringTransactions.ShouldShowInMonth(recurringVM, selectedMonth))
            {
                SelectedTransaction = recurringVM;

                // Skapa mirror om det är income
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
            // Ta bort recurring
            await RecurringTransactions.DeleteAsync(rItem);

            // Ta bort mirrors
            RemoveAllMirrorTransactions(rItem.Id);

            // Skapa reguljär
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

        // Hämta redigerbar version av transaktionen
        public ITransactionVM GetEditableTransaction(ITransactionVM transaction)
        {
            if (transaction is TransactionItemViewModel item && item.IsFromRecurring)
            {
                return RecurringTransactions.RecurringTransactions
                    .FirstOrDefault(r => r.Id == item.RecurringTransactionId) ?? transaction;
            }

            return transaction;
        }

        // Skapa mirror transactions för återkommande inkomster så den har en plats i inkomstlistan
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