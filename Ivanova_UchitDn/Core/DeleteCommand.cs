using System;
using System.Windows.Input;

namespace Ivanova_UchitDn.Core
{
    public class DeleteCommand<T> : ICommand
    where T : class
    {
        private Action<T> deleteData;
        private T item;

        public DeleteCommand(Action<T> deleteData, T item)
        {
            this.deleteData = deleteData;
            this.item = item;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            deleteData(item);
        }
    }
}
