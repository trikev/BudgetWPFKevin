using BudgetWPFKevin.Commands;
using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Services;
using BudgetWPFKevin.ViewModels.Absence;
using BudgetWPFKevin.ViewModels.Summary;
using BudgetWPFKevin.ViewModels.Transactions;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace BudgetWPFKevin.ViewModels
{

    // Huvud-ViewModel för applikationen som binder samman olika delar
    public class MainViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IServiceProvider _serviceProvider;

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
                if (_selectedMonth == value)
                    return;

                _selectedMonth = value;
                OnPropertyChanged();

                if (MonthlySummaryVM?.SelectedMonth != value)
                {
                    MonthlySummaryVM.SelectedMonth = value;
                }

                _ = ReloadForSelectedMonthAsync();
            }
        }


        public MainViewModel(
            IDialogService dialogService,
            IServiceProvider serviceProvider,
            CategoryListVM categoryListVM,
            MonthlySummaryVM monthlySummaryVM,
            TransactionCoordinatorVM transactionCoordinator,
            AbsenceSummaryVM absenceSummaryVM)
        {
            _dialogService = dialogService;
            _serviceProvider = serviceProvider;
            CategoryListVM = categoryListVM;
            MonthlySummaryVM = monthlySummaryVM;
            TransactionCoordinator = transactionCoordinator;
            AbsenceSummaryVM = absenceSummaryVM;

            MonthlySummaryVM.PropertyChanged += MonthlySummaryVM_PropertyChanged;
            TransactionCoordinator.PropertyChanged += TransactionCoordinator_PropertyChanged;

            EditCommand = new DelegateCommand(async _ => await UpdateTransactionAsync(),
                 _ => TransactionCoordinator.SelectedTransaction != null);
            DeleteCommand = new DelegateCommand(async _ => await DeleteTransactionAsync());
            AddCommand = new DelegateCommand(async _ => await AddTransactionAsync());
            OpenUserSettingsCommand = new DelegateCommand(async _ => await OpenUserSettingsAsync());
            AddAbsenceCommand = new DelegateCommand(async _ => await AddAbsenceAsync());

            SelectedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        private void TransactionCoordinator_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TransactionCoordinator.SelectedTransaction))
            {
                EditCommand.RaiseCanExecuteChanged();
            }
        }

        private void MonthlySummaryVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MonthlySummaryVM.SelectedMonth) &&
                SelectedMonth != MonthlySummaryVM.SelectedMonth)
            {
                SelectedMonth = MonthlySummaryVM.SelectedMonth;
            }
        }

        public async Task LoadAsync()
        {
            await CategoryListVM.LoadCategoriesAsync();
            await ReloadForSelectedMonthAsync();
        }

        public async Task ReloadForSelectedMonthAsync()
        {
            await TransactionCoordinator.LoadForMonthAsync(SelectedMonth);
            await MonthlySummaryVM.LoadForMonthAsync(SelectedMonth);
            await AbsenceSummaryVM.LoadForMonthAsync(SelectedMonth);
        }

        private async Task RefreshSummaryAsync()
        {
            await MonthlySummaryVM.RefreshAsync();
        }

        // Lägger till en ny transaktion baserat på om den är recurring eller inte
        private async Task AddTransactionAsync()
        {
            var transactionVM = TransactionCoordinator.CreateNewTransaction();
            var editorVm = new NewTransactionVM(
                transactionVM,
                null,
                CategoryListVM);

            var result = _dialogService.ShowNewTransactionDialog(editorVm);

            if (result != true)
                return;

            if (editorVm.ShouldConvertToRecurring)
            {
                var recurringData = editorVm.GetRecurringData();
                await TransactionCoordinator.SaveAsRecurringTransactionAsync(
                    transactionVM,
                    recurringData,
                    SelectedMonth);
            }
            else
            {
                await TransactionCoordinator.RegularTransactions.SaveAsync(transactionVM);
            }

            await RefreshSummaryAsync();
        }


        // Uppdaterar en befintlig transaktion, med möjlighet att konvertera mellan regular och recurring
        private async Task UpdateTransactionAsync()
        {
            var selected = TransactionCoordinator.SelectedTransaction;
            if (selected == null)
                return;

            var transactionToEdit = TransactionCoordinator.GetEditableTransaction(selected);
            var editorVm = CreateEditorForTransaction(transactionToEdit);

            var result = _dialogService.ShowNewTransactionDialog(editorVm);

            if (result != true)
                return;

            if (editorVm.ShouldConvertToRecurring && transactionToEdit is TransactionItemViewModel regularItem)
            {
                await TransactionCoordinator.ConvertToRecurringAsync(
                    regularItem,
                    editorVm.GetRecurringData(),
                    SelectedMonth);
            }
            else if (editorVm.ShouldConvertToRegular && transactionToEdit is RecurringTransactionItemVM recurringItem)
            {
                await TransactionCoordinator.ConvertToRegularAsync(recurringItem);
            }
            else
            {
                await TransactionCoordinator.UpdateTransactionAsync(transactionToEdit, SelectedMonth);
            }

            await ReloadForSelectedMonthAsync();
        }

        // Skapar en lämplig editor-ViewModel baserat på transaktionstypen
        private NewTransactionVM CreateEditorForTransaction(ITransactionVM transaction)
        {
            if (transaction is TransactionItemViewModel regularTransaction)
                return new NewTransactionVM(regularTransaction, null, CategoryListVM);

            if (transaction is RecurringTransactionItemVM recurringTransaction)
                return new NewTransactionVM(null, recurringTransaction, CategoryListVM);

            throw new InvalidOperationException("Unknown transaction type");
        }

        private async Task DeleteTransactionAsync()
        {
            var selected = TransactionCoordinator.SelectedTransaction;
            if (selected == null)
                return;

            await TransactionCoordinator.DeleteSelectedAsync();
            await RefreshSummaryAsync();
        }

        // Öppnar användarinställningar och laddar om data vid ändringar
        private async Task OpenUserSettingsAsync()
        {
            var vm = _serviceProvider.GetRequiredService<UserSettingsViewModel>();
            await vm.LoadAsync();

            var result = _dialogService.ShowUserSettingsDialog(vm);

            if (result == true)
            {
                await ReloadForSelectedMonthAsync();
            }
        }

        // Lägger till en ny frånvaro och uppdaterar sammanfattningen
        private async Task AddAbsenceAsync()
        {
            var vm = new AbsenceItemVM();
            var result = _dialogService.ShowAbsenceDialog(vm);

            if (result == true && vm.SavedAbsence != null)
            {
                await AbsenceSummaryVM.AddAbsenceAsync(vm.SavedAbsence);
                await RefreshSummaryAsync();
            }
        }
    }
}