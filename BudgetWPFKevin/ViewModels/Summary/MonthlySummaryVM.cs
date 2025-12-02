using BudgetWPFKevin.Commands;
using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using BudgetWPFKevin.Services;
using System.Globalization;

namespace BudgetWPFKevin.ViewModels.Summary
{

    // ViewModel för att visa sammandattning av månadens ekonomi
    public class MonthlySummaryVM : ViewModelBase
    {
        private static readonly CultureInfo SvCulture = CultureInfo.GetCultureInfo("sv-SE");

        private readonly ITransactionRepository _transactionRepository;
        private readonly IRecurringTransaction _recurringRepository;
        private readonly IUserSettingsRepository _userSettingsRepository;
        private readonly IAbsenceRepository _absenceRepository;
        private readonly IIncomeCalculationService _incomeCalculationService;

        public IReadOnlyList<string> Months { get; }
        public IReadOnlyList<int> Years { get; }
        public string MonthName => _selectedMonth.ToString("MMMM yyyy", SvCulture);

        private string _selectedMonthName;
        public string SelectedMonthName
        {
            get => _selectedMonthName;
            set
            {
                if (_selectedMonthName != value && !string.IsNullOrWhiteSpace(value))
                {
                    _selectedMonthName = value;
                    OnPropertyChanged();
                    UpdateSelectedMonthFromUI();
                }
            }
        }

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (_selectedYear != value)
                {
                    _selectedYear = value;
                    OnPropertyChanged();
                    UpdateSelectedMonthFromUI();
                }
            }
        }

        
        private DateTime _selectedMonth;
        public DateTime SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                var monthOnly = new DateTime(value.Year, value.Month, 1);
                if (_selectedMonth != monthOnly)
                {
                    _selectedMonth = monthOnly;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(MonthName));

                    UpdateUIFromSelectedMonth();
                }
            }
        }

        private decimal _totalIncome;
        public decimal TotalIncome
        {
            get => _totalIncome;
            set
            {
                if (_totalIncome != value)
                {
                    _totalIncome = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NetResult));
                    OnPropertyChanged(nameof(NetResultWithAbsence));
                }
            }
        }

        private decimal _totalExpenses;
        public decimal TotalExpenses
        {
            get => _totalExpenses;
            set
            {
                if (_totalExpenses != value)
                {
                    _totalExpenses = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NetResult));
                    OnPropertyChanged(nameof(NetResultWithAbsence));
                }
            }
        }

        private decimal _calculatedMonthlyIncome;
        public decimal CalculatedMonthlyIncome
        {
            get => _calculatedMonthlyIncome;
            set
            {
                if (_calculatedMonthlyIncome != value)
                {
                    _calculatedMonthlyIncome = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NetResultWithAbsence));
                }
            }
        }

        private decimal _absenceDeduction;
        public decimal AbsenceDeduction
        {
            get => _absenceDeduction;
            set
            {
                if (_absenceDeduction != value)
                {
                    _absenceDeduction = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NetResultWithAbsence));
                }
            }
        }

        private decimal _absenceCompensation;
        public decimal AbsenceCompensation
        {
            get => _absenceCompensation;
            set
            {
                if (_absenceCompensation != value)
                {
                    _absenceCompensation = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NetResultWithAbsence));
                }
            }
        }

        private decimal _taxRate = 0.30m;
        public decimal TaxRate
        {
            get => _taxRate;
            set
            {
                if (_taxRate != value)
                {
                    _taxRate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NetResult));
                    OnPropertyChanged(nameof(NetResultWithAbsence));
                }
            }
        }

        // Beräknat nettoresultat för månaden
        public decimal NetResult => TotalIncome - TotalExpenses;

        // Beräknat nettoresultat inklusive frånvaroeffekter
        public decimal NetResultWithAbsence =>
            NetResult - AbsenceDeduction + AbsenceCompensation;

        public DelegateCommand RefreshCommand { get; }

        public MonthlySummaryVM(
            ITransactionRepository transactionRepository,
            IRecurringTransaction recurringRepository,
            IUserSettingsRepository userSettingsRepository,
            IAbsenceRepository absenceRepository,
            IIncomeCalculationService incomeCalculationService)
        {
            _transactionRepository = transactionRepository;
            _recurringRepository = recurringRepository;
            _userSettingsRepository = userSettingsRepository;
            _absenceRepository = absenceRepository;
            _incomeCalculationService = incomeCalculationService;

            Months = SvCulture.DateTimeFormat.MonthNames
                .Where(m => !string.IsNullOrEmpty(m))
                .Select(m => char.ToUpper(m[0]) + m.Substring(1))
                .ToList();

            var currentYear = DateTime.Today.Year;
            Years = Enumerable.Range(currentYear - 5, 11).ToList();

            _selectedYear = currentYear;
            _selectedMonthName = Months[DateTime.Today.Month - 1];
            _selectedMonth = new DateTime(currentYear, DateTime.Today.Month, 1);

            RefreshCommand = new DelegateCommand(async _ => await RefreshAsync());
        }

        // Uppdatera SelectedMonth baserat på UI-valen
        private void UpdateSelectedMonthFromUI()
        {
            if (string.IsNullOrWhiteSpace(SelectedMonthName) || SelectedYear <= 0)
                return;

            int monthNumber = DateTime.ParseExact(
                SelectedMonthName,
                "MMMM",
                SvCulture,
                DateTimeStyles.None).Month;

            SelectedMonth = new DateTime(SelectedYear, monthNumber, 1);
        }


        // Uppdatera UI-valen baserat på SelectedMonth
        private void UpdateUIFromSelectedMonth()
        {
            _selectedYear = _selectedMonth.Year;
            _selectedMonthName = Months[_selectedMonth.Month - 1];

            OnPropertyChanged(nameof(SelectedYear));
            OnPropertyChanged(nameof(SelectedMonthName));
        }

      
        public async Task LoadForMonthAsync(DateTime month)
        {
            SelectedMonth = month;
            await RefreshAsync();
        }


        // Uppdatera sammanfattningen för den valda månaden
        public async Task RefreshAsync()
        {
            var currentMonthTransactions = await _transactionRepository
                .GetByMonthAsync(SelectedMonth);

            var recurringTransactions = await _recurringRepository
                .GetByMonthAsync(SelectedMonth);

            var incomes = currentMonthTransactions
                .Where(t => t.Type == TransactionType.Income);

            var expenses = currentMonthTransactions
                .Where(t => t.Type == TransactionType.Expense);

            var recurringIncomes = recurringTransactions
                .Where(t => t.Type == TransactionType.Income);

            var recurringExpenses = recurringTransactions
                .Where(t => t.Type == TransactionType.Expense);

            TotalIncome = incomes.Sum(t => t.Amount) +
                         recurringIncomes.Sum(t => t.Amount);

            TotalExpenses = expenses.Sum(t => t.Amount) + 
                           recurringExpenses.Sum(t => t.Amount);

            var userSettings = await _userSettingsRepository.GetUserSettingsAsync();

            if (userSettings != null)
            {
                CalculatedMonthlyIncome = _incomeCalculationService
                    .CalculateMonthlyIncome(userSettings);

                TaxRate = userSettings.TaxRate;

                var absences = await _absenceRepository.GetByMonthAsync(SelectedMonth);
                var absenceEffect = _incomeCalculationService
                    .CalculateAbsenceEffect(userSettings, absences);

                AbsenceDeduction = absenceEffect.TotalDeduction;
                AbsenceCompensation = absenceEffect.TotalCompensation;
            }
            else
            {
                CalculatedMonthlyIncome = 0;
                AbsenceDeduction = 0;
                AbsenceCompensation = 0;
            }
        }
    }
}