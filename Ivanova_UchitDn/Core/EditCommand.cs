using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ivanova_UchitDn.Core
{
    public class EditCommand : ICommand
    {
        private Action<object> editData;

        public EditCommand(Action<object> editData)
        {
            this.editData = editData;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            editData(parameter);
        }
    }
}
