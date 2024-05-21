using System.Windows.Input;
using System;

public class EditCommand : ICommand
{
    private Action<object> editData;
    public event EventHandler CanExecuteChanged;

    public EditCommand(Action<object> editData)
    {
        this.editData = editData;
    }

    public bool CanExecute(object parameter)
    {
        // Логика определения возможности выполнения команды
        return true;
    }

    public void Execute(object parameter)
    {
        editData(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
