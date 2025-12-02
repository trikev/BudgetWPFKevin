using BudgetWPFKevin.Models;

namespace BudgetWPFKevin.Services
{
    public interface IIncomeCalculationService
    {
        
        decimal CalculateMonthlyIncome(UserSettings settings);

      
        decimal CalculateHourlyWage(UserSettings settings);

      
        AbsenceEffect CalculateAbsenceEffect(UserSettings settings, List<AbsenceRecord> absences);
    }

    public class AbsenceEffect
    {
        public decimal TotalDeduction { get; set; }
        public decimal TotalCompensation { get; set; }
        public decimal NetEffect => TotalCompensation - TotalDeduction;
    }
}