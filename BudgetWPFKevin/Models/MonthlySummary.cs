namespace BudgetWPFKevin.Models
{
    public class MonthlySummary

    {
        public int Year { get; set; }
        public int Month { get; set; }

        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetResult => TotalIncome - TotalExpenses;

        public decimal TotalAbsenceDeduction { get; set; }
        public decimal TotalAbsenceCompensation { get; set; }

    }

}
