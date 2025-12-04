using BudgetWPFKevin.Data.Interface;
using BudgetWPFKevin.Services;
using BudgetWPFKevin.ViewModels;
using BudgetWPFKevin.ViewModels.Transactions;

public interface ITransactionDialogService
{
    Task<bool> ShowAddTransactionDialogAsync(DateTime selectedMonth);
    Task<bool> ShowEditTransactionDialogAsync(ITransactionVM transaction, DateTime selectedMonth);
}

public class TransactionDialogService : ITransactionDialogService
{
    private readonly IDialogService _dialogService;
    private readonly TransactionCoordinatorVM _coordinator;
    private readonly CategoryListVM _categoryListVM;

    public TransactionDialogService(
        IDialogService dialogService,
        TransactionCoordinatorVM coordinator,
        CategoryListVM categoryListVM)
    {
        _dialogService = dialogService;
        _coordinator = coordinator;
        _categoryListVM = categoryListVM;
    }

    public async Task<bool> ShowAddTransactionDialogAsync(DateTime selectedMonth)
    {
        var transactionVM = _coordinator.CreateNewTransaction();
        var editorVm = new NewTransactionVM(transactionVM, null, _categoryListVM);

        var result = _dialogService.ShowNewTransactionDialog(editorVm);
        if (result != true) return false;

        await _coordinator.AddNewTransactionAsync(editorVm, selectedMonth);
        return true;
    }

    public async Task<bool> ShowEditTransactionDialogAsync(
        ITransactionVM transaction,
        DateTime selectedMonth)
    {
        var transactionToEdit = _coordinator.GetEditableTransaction(transaction);
        var editorVm = _coordinator.CreateEditorForTransaction(transactionToEdit, _categoryListVM);

        var result = _dialogService.ShowNewTransactionDialog(editorVm);
        if (result != true) return false;

        await _coordinator.UpdateExistingTransactionAsync(editorVm, selectedMonth);
        return true;
    }
}