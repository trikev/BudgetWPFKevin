namespace BudgetWPFKevin.Models
{
    public class Category

    {
        public int Id { get; set; }

        public string Name { get; set; }
        public TransactionType AppliesTo { get; set; }

        public ICollection<Transaction> Transactions { get; set; }


    }

}
