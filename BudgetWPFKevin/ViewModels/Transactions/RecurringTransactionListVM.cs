using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using System.Collections.ObjectModel;
using AutoMapper;

namespace BudgetWPFKevin.ViewModels.Transactions
{
    // Vymodell för lista av återkommande transaktioner
    public class RecurringTransactionListVM : ViewModelBase
    {
        private readonly IRecurringTransaction _recurringRepository;
        private readonly IMapper _mapper;

        public ObservableCollection<RecurringTransactionItemVM> RecurringTransactions { get; }

        private RecurringTransactionItemVM? _selectedRecurring;
        public RecurringTransactionItemVM SelectedRecurring
        {
            get => _selectedRecurring;
            set
            {
                if (_selectedRecurring != value)
                {
                    _selectedRecurring = value;
                    OnPropertyChanged();
                }
            }
        }

        public RecurringTransactionListVM(
            IRecurringTransaction recurringRepository,
            IMapper mapper)
        {
            _recurringRepository = recurringRepository;
            _mapper = mapper;
            RecurringTransactions = new ObservableCollection<RecurringTransactionItemVM>();
        }

        // Ladda återkommande transaktioner för en specifik månad
        public async Task LoadForMonthAsync(DateTime month)
        {
            var recurring = await _recurringRepository.GetByMonthAsync(month);

            RecurringTransactions.Clear();

            foreach (var r in recurring)
            {
                RecurringTransactions.Add(new RecurringTransactionItemVM(r));
            }
        }

        public async Task SaveAsync(RecurringTransactionItemVM vm)
        {
            var recurring = _mapper.Map<RecurringTransaction>(vm);
            await _recurringRepository.AddRecurringTransactionAsync(recurring);

            vm.Id = recurring.Id;
            vm.CategoryName = recurring.Category?.Name ?? string.Empty;
            RecurringTransactions.Add(vm);
        }

        public async Task UpdateAsync(RecurringTransactionItemVM vm)
        {
            var recurring = await _recurringRepository.GetRecurringTransactionByIdAsync(vm.Id);
            if (recurring != null)
            {
                _mapper.Map(vm, recurring);
                await _recurringRepository.UpdateRecurringTransactionAsync(recurring);
            }
        }

        public async Task DeleteAsync(RecurringTransactionItemVM vm)
        {
            if (vm.Id != 0)
                await _recurringRepository.DeleteRecurringTransactionAsync(vm.Id);

            RecurringTransactions.Remove(vm);
        }

        // Hämta återkommande transaktioner som är synliga för en specifik månad
        public IEnumerable<RecurringTransactionItemVM> GetVisibleForMonth(DateTime month)
        {
            return RecurringTransactions.Where(r =>
                r.RecurrenceType == RecurrenceType.Monthly ||
                (r.RecurrenceType == RecurrenceType.Yearly && r.Month == month.Month));
        }

        // Kontrollera om en återkommande transaktion ska visas för en specifik månad
        public bool ShouldShowInMonth(RecurringTransactionItemVM vm, DateTime month)
        {
            return vm.RecurrenceType == RecurrenceType.Monthly ||
                   (vm.RecurrenceType == RecurrenceType.Yearly && vm.Month == month.Month);
        }
    }
}