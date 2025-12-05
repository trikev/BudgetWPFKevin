using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using System.Collections.ObjectModel;
using AutoMapper;

namespace BudgetWPFKevin.ViewModels.Transactions
{
    public class IncomeListVM : ViewModelBase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;

        public ObservableCollection<TransactionItemViewModel> Incomes { get; }

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


        public IncomeListVM(ITransactionRepository transactionRepository, IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _mapper = mapper;
            Incomes = new ObservableCollection<TransactionItemViewModel>();
        }

        public async Task LoadForMonthAsync(DateTime month)
        {
            var transactions = await _transactionRepository.GetByMonthAsync(month);
            Incomes.Clear();

            var incomes = transactions.Where(t => t.Type == TransactionType.Income);
            foreach (var income in incomes)
            {
                Incomes.Add(new TransactionItemViewModel(income));
            }
        }

        public async Task SaveAsync(TransactionItemViewModel vm)
        {
            var transaction = _mapper.Map<Transaction>(vm);
            await _transactionRepository.AddAsync(transaction);
            var saved = await _transactionRepository.GetByIdAsync(transaction.Id);
            vm.Id = transaction.Id;
            vm.CategoryName = saved.Category?.Name ?? string.Empty;

            if (vm.IsIncome)
                Add(vm);
        }

        public async Task UpdateAsync(TransactionItemViewModel vm)
        {
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
            Incomes.Remove(vm);
        }

        public void Add(TransactionItemViewModel vm)
        {
            if (vm.IsIncome)
                Incomes.Add(vm);
        }

        public void Remove(TransactionItemViewModel vm)
        {
            Incomes.Remove(vm);
        }
    }
}