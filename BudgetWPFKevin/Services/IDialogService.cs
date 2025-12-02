using BudgetWPFKevin.ViewModels;
using BudgetWPFKevin.ViewModels.Absence;
using BudgetWPFKevin.ViewModels.Transactions;

namespace BudgetWPFKevin.Services
{
    public interface IDialogService
    {

        bool? ShowNewTransactionDialog(NewTransactionVM viewModel);
        bool? ShowUserSettingsDialog(UserSettingsViewModel viewModel);
        bool? ShowAbsenceDialog(AbsenceItemVM viewModel);



    }
}
