using System.Windows;
using System.Windows.Controls;

namespace BudgetWPFKevin.Views
{
    public partial class TransactionListsView : UserControl
    {
        public TransactionListsView()
        {
            InitializeComponent();
        }

        private void DataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            var focusedDataGrid = sender as DataGrid;

        }
    }
}