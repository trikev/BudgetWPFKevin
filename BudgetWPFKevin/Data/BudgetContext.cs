using BudgetWPFKevin.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetWPFKevin.Data
{
    public class BudgetContext : DbContext
    {

        public BudgetContext(DbContextOptions<BudgetContext> options) : base(options) { }
 
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<AbsenceRecord> AbsenceRecords { get; set; }


        
    }
}
