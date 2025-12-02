using BudgetWPFKevin.Models;

namespace BudgetWPFKevin.Services
{

    // Tjänst för att beräkna inkomstrelaterade värden baserat på användarinställningar och frånvarodata

    public class IncomeCalculationService : IIncomeCalculationService
    {
        private const decimal VAB_INCOME_CAP = 410000; 
        private const decimal COMPENSATION_RATE = 0.80m; 

        public decimal CalculateMonthlyIncome(UserSettings settings)
        {
            if (settings == null || settings.YearlyWorkHours == 0)
                return 0;

            var hourlyWage = CalculateHourlyWage(settings);
            var monthlyHours = settings.YearlyWorkHours / 12m;

            return hourlyWage * monthlyHours;
        }

        public decimal CalculateHourlyWage(UserSettings settings)
        {
            if (settings == null || settings.YearlyWorkHours == 0)
                return 0;

            return settings.YearlyIncome / settings.YearlyWorkHours;
        }

        public AbsenceEffect CalculateAbsenceEffect(UserSettings settings, List<AbsenceRecord> absences)
        {
            var effect = new AbsenceEffect();

            if (settings == null || absences == null || !absences.Any())
                return effect;

            var hourlyWage = CalculateHourlyWage(settings);

            foreach (var absence in absences)
            {
                decimal deduction;

                if (absence.Type == AbsenceType.VAB)
                {
                    var cappedYearlyIncome = Math.Min(settings.YearlyIncome, VAB_INCOME_CAP);
                    var cappedHourlyWage = cappedYearlyIncome / settings.YearlyWorkHours;
                    deduction = cappedHourlyWage * absence.Hours;
                }
                else 
                {
                    deduction = hourlyWage * absence.Hours;
                }

                var compensation = deduction * COMPENSATION_RATE;

                effect.TotalDeduction += deduction;
                effect.TotalCompensation += compensation;
            }

            return effect;
        }
    }
}