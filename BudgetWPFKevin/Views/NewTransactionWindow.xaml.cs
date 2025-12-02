using BudgetWPFKevin.ViewModels.Transactions;
using System.Windows;

namespace BudgetWPFKevin.Views
{
    public partial class NewTransactionWindow : Window
    {
        

        public NewTransactionWindow(NewTransactionVM vm)
        {
            InitializeComponent();
            DataContext = vm;

            vm.CloseAction = result =>
            {
                DialogResult = result;
                Close();
            };
        }

        
    }

}
