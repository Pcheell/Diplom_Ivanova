using System;
using System.Windows.Input;

namespace Ivanova_UchitDn.Core
{
    public class DeleteCommand : ICommand
    {
        public Action<int> deleteData;
        public int v;

        public DeleteCommand(Action<int> deleteData, int v)
        {
            this.deleteData = deleteData;
            this.v = v;
        }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            deleteData(v);
        }
    }
}
