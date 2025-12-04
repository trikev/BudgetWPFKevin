using System.Windows.Controls;

namespace BudgetWPFKevin.Views
{
    public partial class IncomeListView : UserControl
    {
        public IncomeListView()
        {
            InitializeComponent();
        }

        private void ListBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                listBox.SelectedItem = null;
            }
        }
    }
}