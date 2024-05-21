using System;
using System.Windows.Input;

namespace Ivanova_UchitDn.ViewModel
{
    public class UpdateData : ICommand
    {
        private Action loadData;

        public UpdateData(Action loadData)
        {
            this.loadData = loadData;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            loadData();
        }
    }
}
