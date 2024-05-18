using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ivanova_UchitDn.Model;

namespace Ivanova_UchitDn.Core
{

    public class DeleteCommand : ICommand
    {
        private Action<User> deleteData;
        private User v;

        public DeleteCommand(Action<User> deleteData, User v)
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

    public class DeleteCommandG : ICommand
    {
        private Action<GrupModel> deleteData;
        private GrupModel v;

        public DeleteCommandG(Action<GrupModel> deleteData, GrupModel v)
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
    
    public class DeleteCommandP: ICommand
    {
        private Action<PredmetModel> deleteData;
        private PredmetModel v;

        public DeleteCommandP(Action<PredmetModel> deleteData, PredmetModel v)
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
    
    public class DeleteCommandS: ICommand
    {
        private Action<StudModel> deleteData;
        private StudModel v;

        public DeleteCommandS(Action<StudModel> deleteData, StudModel v)
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

    public class DeleteCommandR: ICommand
    {
        private Action<RodModel> deleteData;
        private RodModel v;

        public DeleteCommandR(Action<RodModel> deleteData, RodModel v)
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

    public class DeleteCommandU: ICommand
    {
        private Action<UspModel> deleteData;
        private UspModel v;

        public DeleteCommandU(Action<UspModel> deleteData, UspModel v)
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
