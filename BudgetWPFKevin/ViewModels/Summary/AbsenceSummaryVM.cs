using BudgetWPFKevin.Commands;
using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Models;
using BudgetWPFKevin.Services;
using BudgetWPFKevin.ViewModels.Absence;
using System.Collections.ObjectModel;

namespace BudgetWPFKevin.ViewModels.Summary
{

    // Vymodell för sammanfattning av frånvaro

    public class AbsenceSummaryVM : ViewModelBase
    {
        private readonly IAbsenceRepository _absenceRepository;
        private readonly IUserSettingsRepository _userSettingsRepository;
        private readonly IIncomeCalculationService _incomeCalculationService;

        private ObservableCollection<AbsenceItemVM> _absenceRecords;
        public ObservableCollection<AbsenceItemVM> AbsenceRecords
        {
            get => _absenceRecords;
            set
            {
                if (_absenceRecords != value)
                {
                    _absenceRecords = value;
                    OnPropertyChanged();
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
                    OnPropertyChanged(nameof(AbsenceNetEffect));
                }
            }
        }

        private AbsenceItemVM _selectedAbsence;
        public AbsenceItemVM SelectedAbsence
        {
            get => _selectedAbsence;
            set
            {
                if (_selectedAbsence == value)
                    return;

                _selectedAbsence = value;
                OnPropertyChanged();
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
                    OnPropertyChanged(nameof(AbsenceNetEffect));
                }
            }
        }

        // Nettoeffekt av frånvaro: kompensation minus avdrag
        public decimal AbsenceNetEffect => AbsenceCompensation - AbsenceDeduction;

        public DelegateCommand DeleteCommand { get; }

        public AbsenceSummaryVM(
            IAbsenceRepository absenceRepository,
            IUserSettingsRepository userSettingsRepository,
            IIncomeCalculationService incomeCalculationService)
        {
            _absenceRepository = absenceRepository;
            _userSettingsRepository = userSettingsRepository;
            _incomeCalculationService = incomeCalculationService;

            AbsenceRecords = new ObservableCollection<AbsenceItemVM>();
        }

        public async Task LoadForMonthAsync(DateTime month)
        {
            await RefreshAsync(month);
        }


        // Uppdatera frånvaroposter för en viss månad
        public async Task RefreshAsync(DateTime month)
        {
            var absences = await _absenceRepository.GetByMonthAsync(month);

            UpdateAbsenceRecords(absences);
            await RecalculateAbsenceEffectsAsync();
        }

        public async Task AddAbsenceAsync(AbsenceRecord absence)
        {
            await _absenceRepository.AddAsync(absence);

            var vm = new AbsenceItemVM(absence);
            AbsenceRecords.Add(vm);

            await RecalculateAbsenceEffectsAsync();
        }

        public async Task DeleteAbsenceAsync(int absenceId)
        {
            await _absenceRepository.DeleteAsync(absenceId);

            var vm = AbsenceRecords.FirstOrDefault(a => a.Id == absenceId);
            if (vm != null)
            {
                AbsenceRecords.Remove(vm);
                if (SelectedAbsence == vm)
                {
                    SelectedAbsence = null;
                }
            }

            await RecalculateAbsenceEffectsAsync();
        }

        // Uppdatera frånvaroposter i vymodellen
        private void UpdateAbsenceRecords(IEnumerable<AbsenceRecord> absences)
        {
            AbsenceRecords.Clear();
            SelectedAbsence = null;

            foreach (var absence in absences)
            {
                var vm = new AbsenceItemVM(absence);
                AbsenceRecords.Add(vm);
            }
        }

        // Beräkna effekterna av frånvaro på inkomst
        private async Task RecalculateAbsenceEffectsAsync()
        {
            if (!AbsenceRecords.Any())
            {
                AbsenceDeduction = 0;
                AbsenceCompensation = 0;
                return;
            }

            var userSettings = await _userSettingsRepository.GetUserSettingsAsync();
            if (userSettings == null)
            {
                AbsenceDeduction = 0;
                AbsenceCompensation = 0;
                return;
            }

            var absences = AbsenceRecords.Select(vm => vm.ToModel()).ToList();
            var effect = _incomeCalculationService.CalculateAbsenceEffect(userSettings, absences);

            AbsenceDeduction = effect.TotalDeduction;
            AbsenceCompensation = effect.TotalCompensation;
        }
    }
}