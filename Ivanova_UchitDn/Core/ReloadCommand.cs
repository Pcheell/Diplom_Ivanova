using System;
using System.Windows.Input;

namespace Ivanova_UchitDn.Core
{
    public class ReloadCommand : ICommand
    {
        private Action loadDataDB;

        public ReloadCommand(Action loadDataDB)
        {
            this.loadDataDB = loadDataDB;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            loadDataDB();
        }
    }
}
