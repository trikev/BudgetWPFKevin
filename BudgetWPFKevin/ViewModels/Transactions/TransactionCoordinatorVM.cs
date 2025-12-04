using AutoMapper;
using BudgetWPFKevin.Commands;
using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using BudgetWPFKevin.ViewModels.Categories;
using System.ComponentModel;

namespace BudgetWPFKevin.ViewModels.Transactions
{
    // Koordinatorvymodell för att hantera transaktioner (inkomster, utgifter och återkommande)
    public class TransactionCoordinatorVM : ViewModelBase
    {
        private readonly CategoryListVM _categoryListVM;
        private readonly IMapper _mapper;
        private bool _isUpdatingSelection = false;

        public IncomeListVM IncomesListVM { get; }
        public ExpenseListVM ExpensesListVM { get; }
        public RecurringTransactionListVM RecurringTransactionsListVM { get; }

        private ITransactionVM? _selectedTransaction;
        public ITransactionVM? SelectedTransaction
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
            IncomeListVM incomes,
            ExpenseListVM expenses,
            RecurringTransactionListVM recurringTransactions,
            CategoryListVM categoryListVM,
            IMapper mapper)
        {
            IncomesListVM = incomes;
            ExpensesListVM = expenses;
            RecurringTransactionsListVM = recurringTransactions;
            _categoryListVM = categoryListVM;
            _mapper = mapper;

            IncomesListVM.PropertyChanged += OnIncomeSelectionChanged;
            ExpensesListVM.PropertyChanged += OnExpenseSelectionChanged;
            RecurringTransactionsListVM.PropertyChanged += OnRecurringSelectionChanged;

            DeleteCommand = new DelegateCommand(
                async _ => await DeleteSelectedAsync(),
                _ => SelectedTransaction != null);
        }

        // Hantera ändringar i valda transaktioner från underliggande income-lista
        private void OnIncomeSelectionChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IncomesListVM.SelectedTransaction) && !_isUpdatingSelection)
            {
                _isUpdatingSelection = true;
                try
                {
                    SelectedTransaction = IncomesListVM.SelectedTransaction;

                    if (SelectedTransaction != null)
                    {
                        ExpensesListVM.SelectedTransaction = null;
                        RecurringTransactionsListVM.SelectedRecurring = null;
                    }
                }
                finally
                {
                    _isUpdatingSelection = false;
                }
            }
        }

        // Hantera ändringar i valda transaktioner från underliggande Expense-lista
        private void OnExpenseSelectionChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExpensesListVM.SelectedTransaction) && !_isUpdatingSelection)
            {
                _isUpdatingSelection = true;
                try
                {
                    SelectedTransaction = ExpensesListVM.SelectedTransaction;

                    if (SelectedTransaction != null)
                    {
                        IncomesListVM.SelectedTransaction = null;
                        RecurringTransactionsListVM.SelectedRecurring = null;
                    }
                }
                finally
                {
                    _isUpdatingSelection = false;
                }
            }
        }

        // Hantera ändringar i valda transaktioner från underliggande återkommande transaktionslista
        private void OnRecurringSelectionChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RecurringTransactionsListVM.SelectedRecurring) && !_isUpdatingSelection)
            {
                _isUpdatingSelection = true;
                try
                {
                    SelectedTransaction = RecurringTransactionsListVM.SelectedRecurring;

                    if (SelectedTransaction != null)
                    {
                        IncomesListVM.SelectedTransaction = null;
                        ExpensesListVM.SelectedTransaction = null;
                    }
                }
                finally
                {
                    _isUpdatingSelection = false;
                }
            }
        }

        // Laddar transaktioner för en specifik månad
        public async Task LoadForMonthAsync(DateTime month)
        {
            await IncomesListVM.LoadForMonthAsync(month);
            await ExpensesListVM.LoadForMonthAsync(month);
            await RecurringTransactionsListVM.LoadForMonthAsync(month);
            CreateMirrorTransactions(month);
        }

        // Skapar en ny transaktion med standardvärden
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

        // Skapar en redigerbar kopia av en transaktion
        public ITransactionVM GetEditableTransaction(ITransactionVM transaction)
        {
            if (transaction is TransactionItemViewModel item)
            {
                if (item.IsFromRecurring)
                {
                    var recurrance = RecurringTransactionsListVM.RecurringTransactions
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

        // Skapar en redigeringsvy för en transaktion baserat på dess typ
        public NewTransactionVM CreateEditorForTransaction(
            ITransactionVM transaction,
            CategoryListVM categoryListVM)
        {
            return transaction switch
            {
                TransactionItemViewModel regular => new NewTransactionVM(regular, null, categoryListVM),
                RecurringTransactionItemVM recurring => new NewTransactionVM(null, recurring, categoryListVM),
                _ => throw new InvalidOperationException("Unknown transaction type")
            };
        }

        // Lägger till en ny transaktion, antingen som regelbunden eller återkommande
        public async Task AddNewTransactionAsync(NewTransactionVM editorVm, DateTime selectedMonth)
        {
            var transactionVM = editorVm.Transaction;
            if (transactionVM == null) return;

            if (editorVm.ShouldConvertToRecurring)
            {
                var recurringData = editorVm.GetRecurringData();
                await SaveAsRecurringTransactionAsync(transactionVM, recurringData, selectedMonth);
            }
            else
            {
                if (transactionVM.IsIncome)
                    await IncomesListVM.SaveAsync(transactionVM);
                else
                    await ExpensesListVM.SaveAsync(transactionVM);
            }
        }

        // Uppdaterar en befintlig transaktion, med möjlighet att konvertera mellan regelbunden och återkommande
        public async Task UpdateExistingTransactionAsync(NewTransactionVM editorVm, DateTime selectedMonth)
        {
            var transactionToEdit = editorVm.Transaction ?? (ITransactionVM)editorVm.RecurringTransaction;
            if (transactionToEdit == null) return;

            if (editorVm.ShouldConvertToRecurring && transactionToEdit is TransactionItemViewModel regularItem)
            {
                await ConvertToRecurringAsync(regularItem, editorVm.GetRecurringData(), selectedMonth);
            }
            else if (editorVm.ShouldConvertToRegular && transactionToEdit is RecurringTransactionItemVM recurringItem)
            {
                await ConvertToRegularAsync(recurringItem);
            }
            else
            {
                await UpdateTransactionAsync(transactionToEdit, selectedMonth);
            }
        }

        // Tar bort den valda transaktionen
        public async Task DeleteSelectedAsync()
        {
            if (SelectedTransaction == null) return;

            if (SelectedTransaction is TransactionItemViewModel item)
            {
                if (item.IsFromRecurring)
                {
                    var recurring = RecurringTransactionsListVM.RecurringTransactions
                        .FirstOrDefault(r => r.Id == item.RecurringTransactionId);

                    if (recurring != null)
                    {
                        await RecurringTransactionsListVM.DeleteAsync(recurring);
                        RemoveAllMirrorTransactions(recurring.Id);
                    }
                }
                else
                {
                    if (item.IsIncome)
                        await IncomesListVM.DeleteAsync(item);
                    else
                        await ExpensesListVM.DeleteAsync(item);
                }
            }
            else if (SelectedTransaction is RecurringTransactionItemVM recurring)
            {
                // Ta bort alla spegeltransaktioner kopplade till den återkommande transaktionen
                await RecurringTransactionsListVM.DeleteAsync(recurring);
                RemoveAllMirrorTransactions(recurring.Id);
            }

            SelectedTransaction = null;
        }

        // Uppdaterar en befintlig transaktion med nya värden
        private async Task UpdateTransactionAsync(ITransactionVM editedVM, DateTime selectedMonth)
        {
            if (editedVM is TransactionItemViewModel editedItem)
            {
                var original = GetAllTransactions()
                    .FirstOrDefault(t => t.Id == editedItem.Id);

                if (original != null)
                {
                    bool typeChanged = original.Type != editedItem.Type;

                    if (typeChanged)
                    {
                        if (original.IsIncome)
                            IncomesListVM.Remove(original);
                        else
                            ExpensesListVM.Remove(original);
                    }

                    CopyTransactionValues(editedItem, original);

                    if (original.IsIncome)
                        await IncomesListVM.UpdateAsync(original);
                    else
                        await ExpensesListVM.UpdateAsync(original);

                    if (typeChanged)
                    {
                        if (original.IsIncome)
                            IncomesListVM.Add(original);
                        else
                            ExpensesListVM.Add(original);
                    }
                }
            }
            // Uppdatera återkommande transaktioner om typen stämmer
            else if (editedVM is RecurringTransactionItemVM editedRecurring)
            {
                var original = RecurringTransactionsListVM.RecurringTransactions
                    .FirstOrDefault(r => r.Id == editedRecurring.Id);

                if (original != null)
                {
                    CopyRecurringValues(editedRecurring, original);
                    await RecurringTransactionsListVM.UpdateAsync(original);

                    RemoveAllMirrorTransactions(original.Id);
                    if (original.Type == TransactionType.Income &&
                        RecurringTransactionsListVM.ShouldShowInMonth(original, selectedMonth))
                    {
                        var mirror = CreateMirrorTransaction(original, selectedMonth);
                        IncomesListVM.Add(mirror);
                    }
                }
            }
        }

        // Sparar en ny återkommande transaktion
        private async Task SaveAsRecurringTransactionAsync(
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
            await RecurringTransactionsListVM.SaveAsync(recurringVM);

            if (RecurringTransactionsListVM.ShouldShowInMonth(recurringVM, selectedMonth))
            {
                SelectedTransaction = recurringVM;

                if (recurring.Type == TransactionType.Income)
                {
                    var mirror = CreateMirrorTransaction(recurringVM, selectedMonth);
                    IncomesListVM.Add(mirror);
                }
            }
        }

        // Konverterar en regelbunden transaktion till en återkommande transaktion
        private async Task ConvertToRecurringAsync(
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
            await RecurringTransactionsListVM.SaveAsync(recurringVM);

            if (item.IsIncome)
                await IncomesListVM.DeleteAsync(item);
            else
                await ExpensesListVM.DeleteAsync(item);

            if (RecurringTransactionsListVM.ShouldShowInMonth(recurringVM, selectedMonth))
            {
                SelectedTransaction = recurringVM;

                if (recurring.Type == TransactionType.Income)
                {
                    var mirror = CreateMirrorTransaction(recurringVM, selectedMonth);
                    IncomesListVM.Add(mirror);
                }
            }
            else
            {
                SelectedTransaction = null;
            }
        }

        // Konverterar en återkommande transaktion till en regelbunden transaktion
        private async Task ConvertToRegularAsync(RecurringTransactionItemVM rItem)
        {
            await RecurringTransactionsListVM.DeleteAsync(rItem);
            RemoveAllMirrorTransactions(rItem.Id);

            var transaction = _mapper.Map<Transaction>(rItem);
            var newVM = new TransactionItemViewModel(transaction);

            if (newVM.IsIncome)
                await IncomesListVM.SaveAsync(newVM);
            else
                await ExpensesListVM.SaveAsync(newVM);

            SelectedTransaction = newVM;
        }

        // Skapa en kopia av en regelbunden transaktion
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

        // Skapa en kopia av en återkommande transaktion
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

        // Kopiera värden från en transaktionsvy till en annan
        private void CopyTransactionValues(TransactionItemViewModel from, TransactionItemViewModel to)
        {
            to.Description = from.Description;
            to.Amount = from.Amount;
            to.Date = from.Date;
            to.CategoryId = from.CategoryId;
            to.Type = from.Type;
        }

        // Kopiera värden från en återkommande transaktionsvy till en annan
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
            var visibleRecurring = RecurringTransactionsListVM.GetVisibleForMonth(month);

            foreach (var recurring in visibleRecurring)
            {
                if (recurring.Type == TransactionType.Income)
                {
                    var mirror = CreateMirrorTransaction(recurring, month);
                    IncomesListVM.Add(mirror);
                }
            }
        }

        // Skapar en spegeltransaktion för en återkommande transaktion
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

        // Tar bort alla spegeltransaktioner kopplade till en återkommande transaktion
        private void RemoveAllMirrorTransactions(int recurringId)
        {
            var incomeMirrors = IncomesListVM.Incomes
                .Where(x => x.RecurringTransactionId == recurringId)
                .ToList();

            foreach (var mirror in incomeMirrors)
            {
                IncomesListVM.Remove(mirror);
            }

            var expenseMirrors = ExpensesListVM.Expenses
                .Where(x => x.RecurringTransactionId == recurringId)
                .ToList();

            foreach (var mirror in expenseMirrors)
            {
                ExpensesListVM.Remove(mirror);
            }
        }

        // Hämtar alla transaktioner (inkomster och utgifter)
        private IEnumerable<TransactionItemViewModel> GetAllTransactions()
        {
            return IncomesListVM.Incomes.Concat(ExpensesListVM.Expenses);
        }
    }
}