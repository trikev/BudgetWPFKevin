using BudgetWPFKevin.Commands;
using BudgetWPFKevin.Models;

namespace BudgetWPFKevin.ViewModels.Absence
{
    // ViewModel för en frånvaropost

    public class AbsenceItemVM : ViewModelBase
    {
        private readonly AbsenceRecord _absence;

        public AbsenceItemVM(AbsenceRecord absence)
        {
            _absence = absence;
            SaveCommand = new DelegateCommand(Save, CanSave);
            CancelCommand = new DelegateCommand(Cancel);
            ValidateInput();
        }

        public int Id
        {
            get => _absence.Id;
            set
            {
                if (_absence.Id != value)
                {
                    _absence.Id = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime Date
        {
            get => _absence.Date.DateTime;
            set
            {
                if (_absence.Date.DateTime != value)
                {
                    _absence.Date = new DateTimeOffset(value);
                    OnPropertyChanged();
                    ValidateInput();
                }
            }
        }

        public AbsenceType SelectedType
        {
            get => _absence.Type;
            set
            {
                if (_absence.Type != value)
                {
                    _absence.Type = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayTypeName));
                }
            }
        }

        public string DisplayTypeName => SelectedType == AbsenceType.Sick ? "Sjuk" : "VAB";

        public List<AbsenceType> AbsenceTypes => Enum.GetValues(typeof(AbsenceType))
            .Cast<AbsenceType>()
            .ToList();

        public decimal Hours
        {
            get => _absence.Hours;
            set
            {
                if (_absence.Hours != value)
                {
                    _absence.Hours = value;
                    OnPropertyChanged();
                    ValidateInput();
                }
            }
        }

        private string _validationError;
        public string ValidationError
        {
            get => _validationError;
            set
            {
                if (_validationError != value)
                {
                    _validationError = value;
                    OnPropertyChanged();
                }
            }
        }

        // För att visa användarvänliga namn i UI

        public List<AbsenceTypeDisplay> AbsenceTypeDisplays => new List<AbsenceTypeDisplay>
        {
            new AbsenceTypeDisplay { Type = AbsenceType.Sick, DisplayName = "Sjuk" },
            new AbsenceTypeDisplay { Type = AbsenceType.VAB, DisplayName = "VAB" }
        };

        public bool IsValid { get; private set; } = true;

        public DelegateCommand SaveCommand { get; }
        public DelegateCommand CancelCommand { get; }

        public Action<bool?>? CloseAction { get; set; }

        public AbsenceRecord? SavedAbsence { get; private set; }

        public AbsenceRecord ToModel() => _absence;

        // Validera användarens inmatning

        private void ValidateInput()
        {
            ValidationError = string.Empty;
            IsValid = true;

            if (Hours <= 0 || Hours > 24)
            {
                ValidationError = "Timmar måste vara mellan 0 och 24";
                IsValid = false;
            }
            else if (Date > DateTime.Today)
            {
                ValidationError = "Datum kan inte vara i framtiden";
                IsValid = false;
            }

            SaveCommand?.RaiseCanExecuteChanged();
        }

        private bool CanSave(object parameter)
        {
            return IsValid && Hours > 0 && Date <= DateTime.Today;
        }

        private void Save(object parameter)
        {
            SavedAbsence = _absence;
            CloseAction?.Invoke(true);
        }

        private void Cancel(object parameter)
        {
            CloseAction?.Invoke(false);
        }
    }

    // Hjälpklass för att koppla frånvarotyp med visningsnamn

    public class AbsenceTypeDisplay
    {
        public AbsenceType Type { get; set; }
        public string DisplayName { get; set; }
    }
}