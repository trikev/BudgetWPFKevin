namespace BudgetWPFKevin.Models
{
    public class UserSettings

    {
        public int Id { get; set; }

        public decimal YearlyIncome { get; set; }
        public int YearlyWorkHours { get; set; }

        public decimal TaxRate { get; set; } = 0.32m;
    }

}
