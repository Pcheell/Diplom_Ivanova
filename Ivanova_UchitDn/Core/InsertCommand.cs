using System;
using System.Windows.Input;

namespace Ivanova_UchitDn.Core
{
    public class InsertCommand : ICommand
    {
        private Action insertData;

        public InsertCommand(Action insertData)
        {
            this.insertData = insertData;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            insertData();
        }
    }
}
