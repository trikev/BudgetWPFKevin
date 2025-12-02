using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using System.Collections.ObjectModel;
using AutoMapper;

namespace BudgetWPFKevin.ViewModels.Transactions
{
    // Vymodell för att hantera en lista av transaktioner
    public class TransactionListVM : ViewModelBase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;

        public ObservableCollection<TransactionItemViewModel> Incomes { get; }
        public ObservableCollection<TransactionItemViewModel> Expenses { get; }

        private TransactionItemViewModel _selectedTransaction;
        public TransactionItemViewModel SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                if (_selectedTransaction != value)
                {
                    _selectedTransaction = value;
                    OnPropertyChanged();
                }
            }
        }

        public TransactionListVM(
            ITransactionRepository transactionRepository,
            IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _mapper = mapper;

            Incomes = new ObservableCollection<TransactionItemViewModel>();
            Expenses = new ObservableCollection<TransactionItemViewModel>();
        }

        
        public async Task LoadForMonthAsync(DateTime month)
        {
            var transactions = await _transactionRepository.GetByMonthAsync(month);

            Incomes.Clear();
            Expenses.Clear();

            var incomes = transactions.Where(t => t.Type == TransactionType.Income);
            var expenses = transactions.Where(t => t.Type == TransactionType.Expense);

            foreach (var income in incomes)
            {
                Incomes.Add(new TransactionItemViewModel(income));
            }

            foreach (var expense in expenses)
            {
                Expenses.Add(new TransactionItemViewModel(expense));
            }
        }

        public async Task SaveAsync(TransactionItemViewModel vm)
        {
            var transaction = _mapper.Map<Transaction>(vm);
            await _transactionRepository.AddAsync(transaction);

            var saved = await _transactionRepository.GetByIdAsync(transaction.Id);
            vm.Id = transaction.Id;
            vm.CategoryName = saved.Category?.Name ?? string.Empty;

            AddToCorrectCollection(vm);
        }

        public async Task UpdateAsync(TransactionItemViewModel vm)
        {
            RemoveFromCollections(vm);
            AddToCorrectCollection(vm);

            var transaction = await _transactionRepository.GetByIdAsync(vm.Id);
            if (transaction != null)
            {
                _mapper.Map(vm, transaction);
                await _transactionRepository.UpdateAsync(transaction);
                vm.CategoryName = transaction.Category?.Name ?? string.Empty;
            }
        }

        public async Task DeleteAsync(TransactionItemViewModel vm)
        {
            if (vm.Id != 0)
                await _transactionRepository.DeleteAsync(vm.Id);

            RemoveFromCollections(vm);
        }

        public void Add(TransactionItemViewModel vm)
        {
            AddToCorrectCollection(vm);
        }

        public void Remove(TransactionItemViewModel vm)
        {
            RemoveFromCollections(vm);
        }

        // Hjälpmetoder för att addera till rätt lista
        private void AddToCorrectCollection(TransactionItemViewModel vm)
        {
            if (vm.IsIncome)
                Incomes.Add(vm);
            else
                Expenses.Add(vm);
        }

        // Hjälpmetod för att ta bort från båda listorna
        private void RemoveFromCollections(TransactionItemViewModel vm)
        {
            Incomes.Remove(vm);
            Expenses.Remove(vm);
        }
    }
}