using BudgetWPFKevin.ViewModels;
using BudgetWPFKevin.ViewModels.Absence;
using BudgetWPFKevin.ViewModels.Transactions;
using BudgetWPFKevin.Views;
using System.Windows;

namespace BudgetWPFKevin.Services
{

    // Service för att hantera dialogrutor i applikationen

    public class DialogService : IDialogService
    {
        public bool? ShowNewTransactionDialog(NewTransactionVM viewModel)
        {
            var window = new NewTransactionWindow(viewModel)
            {
                Owner = Application.Current.MainWindow
            };

            return window.ShowDialog();
        }

        public bool? ShowUserSettingsDialog(UserSettingsViewModel viewModel)
        {
            var window = new UserSettingsWindow(viewModel)
            {
                Owner = Application.Current.MainWindow
            };

            return window.ShowDialog();
        }

        public bool? ShowAbsenceDialog(AbsenceItemVM viewModel)
        {
            var window = new AbsenceWindow(viewModel)
            {
                Owner = Application.Current.MainWindow
            };

            return window.ShowDialog();
        }
    }
}