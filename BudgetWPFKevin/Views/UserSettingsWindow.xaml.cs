using BudgetWPFKevin.ViewModels;
using System.Windows;

namespace BudgetWPFKevin.Views
{
    public partial class UserSettingsWindow : Window
    {
        public UserSettingsWindow(UserSettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Setup CloseAction så ViewModel kan stänga fönstret
            viewModel.CloseAction = (result) =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}