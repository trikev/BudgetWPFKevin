using BudgetWPFKevin.ViewModels.Absence;
using System.Windows;

namespace BudgetWPFKevin.Views
{
    public partial class AbsenceWindow : Window
    {
        public AbsenceWindow(AbsenceItemVM viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Setup CloseAction
            viewModel.CloseAction = (result) =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}