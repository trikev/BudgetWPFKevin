using System.ComponentModel;

namespace BudgetWPFKevin.Models
{
    public enum AbsenceType
    {
        [Description("VAB")]
        VAB,
        [Description("Sjuk")]
        Sick
    }

    public class AbsenceRecord

    {
        public int Id { get; set; }

        public DateTimeOffset Date { get; set; }
        public AbsenceType Type { get; set; }

        public decimal Hours { get; set; }

    }

}
