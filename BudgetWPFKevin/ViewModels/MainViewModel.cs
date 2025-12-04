using BudgetWPFKevin.Commands;
using BudgetWPFKevin.Services;
using BudgetWPFKevin.ViewModels;
using BudgetWPFKevin.ViewModels.Absence;
using BudgetWPFKevin.ViewModels.Summary;
using BudgetWPFKevin.ViewModels.Transactions;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
namespace BudgetWPFKevin.ViewModels
{

    // Huvudvymodell för applikationen som hanterar samordning mellan olika delar
    public class MainViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITransactionDialogService _transactionDialogService;

        public CategoryListVM CategoryListVM { get; }
        public MonthlySummaryVM MonthlySummaryVM { get; }
        public TransactionCoordinatorVM TransactionCoordinator { get; }
        public AbsenceSummaryVM AbsenceSummaryVM { get; }
        public DelegateCommand EditCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand AddCommand { get; }
        public DelegateCommand OpenUserSettingsCommand { get; }
        public DelegateCommand AddAbsenceCommand { get; }
        private DateTime _selectedMonth;
        public DateTime SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (_selectedMonth == value) return;
                _selectedMonth = value;
                OnPropertyChanged();

                if (MonthlySummaryVM?.SelectedMonth != value)
                    MonthlySummaryVM.SelectedMonth = value;

                _ = ReloadForSelectedMonthAsync();
            }
        }
        public MainViewModel(
            IDialogService dialogService,
            IServiceProvider serviceProvider,
            ITransactionDialogService transactionDialogService,
            CategoryListVM categoryListVM,
            MonthlySummaryVM monthlySummaryVM,
            TransactionCoordinatorVM transactionCoordinator,
            AbsenceSummaryVM absenceSummaryVM)
        {
            _dialogService = dialogService;
            _serviceProvider = serviceProvider;
            _transactionDialogService = transactionDialogService;
            CategoryListVM = categoryListVM;
            MonthlySummaryVM = monthlySummaryVM;
            TransactionCoordinator = transactionCoordinator;
            AbsenceSummaryVM = absenceSummaryVM;

            MonthlySummaryVM.PropertyChanged += MonthlySummaryVM_PropertyChanged;
            TransactionCoordinator.PropertyChanged += TransactionCoordinator_PropertyChanged;

            EditCommand = new DelegateCommand(
                async _ => await UpdateTransactionAsync(),
                _ => TransactionCoordinator.SelectedTransaction != null);
            DeleteCommand = new DelegateCommand(async _ => await DeleteTransactionAsync());
            AddCommand = new DelegateCommand(async _ => await AddTransactionAsync());
            OpenUserSettingsCommand = new DelegateCommand(async _ => await OpenUserSettingsAsync());
            AddAbsenceCommand = new DelegateCommand(async _ => await AddAbsenceAsync());
            SelectedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        // Uppdatera kommandots tillstånd när den valda transaktionen ändras
        private void TransactionCoordinator_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TransactionCoordinator.SelectedTransaction))
            {
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        // Synkronisera SelectedMonth mellan MainViewModel och MonthlySummaryVM
        private void MonthlySummaryVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MonthlySummaryVM.SelectedMonth) &&
                SelectedMonth != MonthlySummaryVM.SelectedMonth)
            {
                SelectedMonth = MonthlySummaryVM.SelectedMonth;
            }
        }
       

        // Initial laddning av data
        public async Task LoadAsync()
        {
            await CategoryListVM.LoadCategoriesAsync();
            await ReloadForSelectedMonthAsync();
        }

        // Ladda om data för den valda månaden
        public async Task ReloadForSelectedMonthAsync()
        {
            await TransactionCoordinator.LoadForMonthAsync(SelectedMonth);
            await MonthlySummaryVM.LoadForMonthAsync(SelectedMonth);
            await AbsenceSummaryVM.LoadForMonthAsync(SelectedMonth);
        }

        private async Task AddTransactionAsync()
        {
            if (await _transactionDialogService.ShowAddTransactionDialogAsync(SelectedMonth))
                await MonthlySummaryVM.RefreshAsync();
        }

        private async Task UpdateTransactionAsync()
        {
            var selected = TransactionCoordinator.SelectedTransaction;
            if (selected == null) return;

            if (await _transactionDialogService.ShowEditTransactionDialogAsync(selected, SelectedMonth))
                await ReloadForSelectedMonthAsync();
        }

        private async Task DeleteTransactionAsync()
        {
            if (TransactionCoordinator.SelectedTransaction == null) return;

            if(AbsenceSummaryVM.SelectedAbsence != null) 
            {
                await AbsenceSummaryVM.DeleteAbsenceAsync(AbsenceSummaryVM.SelectedAbsence.Id);
            }

            await TransactionCoordinator.DeleteSelectedAsync();
            await MonthlySummaryVM.RefreshAsync();
        }

        private async Task OpenUserSettingsAsync()
        {
            var vm = _serviceProvider.GetRequiredService<UserSettingsViewModel>();
            await vm.LoadAsync();

            if (_dialogService.ShowUserSettingsDialog(vm) == true)
                await ReloadForSelectedMonthAsync();
        }

        private async Task AddAbsenceAsync()
        {
            var vm = new AbsenceItemVM();

            if (_dialogService.ShowAbsenceDialog(vm) == true && vm.SavedAbsence != null)
            {
                await AbsenceSummaryVM.AddAbsenceAsync(vm.SavedAbsence);
                await MonthlySummaryVM.RefreshAsync();
            }
        }
    }
}