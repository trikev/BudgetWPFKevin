using BudgetWPFKevin.Commands;
using BudgetWPFKevin.Models;

namespace BudgetWPFKevin.ViewModels.Absence
{
    // ViewModel för en frånvaropost

    public class AbsenceItemVM : ViewModelBase
    {
        private int _id;
        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime _date = DateTime.Today;
        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged();
                    ValidateInput();
                }
            }
        }

        private AbsenceType _selectedType = AbsenceType.Sick;
        public AbsenceType SelectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayTypeName));
                }
            }
        }

        public string DisplayTypeName => SelectedType == AbsenceType.Sick ? "Sjuk" : "VAB";

        public List<AbsenceType> AbsenceTypes => Enum.GetValues(typeof(AbsenceType))
            .Cast<AbsenceType>()
            .ToList();

        private decimal _hours = 8;
        public decimal Hours
        {
            get => _hours;
            set
            {
                if (_hours != value)
                {
                    _hours = value;
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

        public AbsenceItemVM()
        {
            SaveCommand = new DelegateCommand(Save, CanSave);
            CancelCommand = new DelegateCommand(Cancel);
        }

        public AbsenceItemVM(AbsenceRecord absence) : this()
        {
            Id = absence.Id;
            Date = absence.Date.DateTime;
            SelectedType = absence.Type;
            Hours = absence.Hours;
        }

        public AbsenceRecord ToAbsenceRecord()
        {
            return new AbsenceRecord
            {
                Id = Id,
                Date = new DateTimeOffset(Date),
                Type = SelectedType,
                Hours = Hours
            };
        }

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

            SaveCommand.RaiseCanExecuteChanged();
        }

        private bool CanSave(object parameter)
        {
            return IsValid && Hours > 0 && Date <= DateTime.Today;
        }

        private void Save(object parameter)
        {
            var absence = new AbsenceRecord
            {
                Id = Id, 
                Date = new DateTimeOffset(Date),
                Type = SelectedType,
                Hours = Hours
            };

            SavedAbsence = absence;
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