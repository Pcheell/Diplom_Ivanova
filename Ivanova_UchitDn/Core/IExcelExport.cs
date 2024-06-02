using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ivanova_UchitDn.Core
{
    public class IExcelExport : ICommand
    {
        private Action excelExport;

        public IExcelExport(Action insertData)
        {
            this.excelExport = insertData;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            excelExport();
        }
    }
}
