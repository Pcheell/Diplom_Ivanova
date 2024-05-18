using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ivanova_UchitDn.Core.CoreApp;

namespace Ivanova_UchitDn.Model
{
    public class UspModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string property)
        {
            if (property == null)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        
        private int IDStudSelf;
        public int IDStud
        {
            get => IDStudSelf;
            set
            {
                IDStudSelf = value;
                OnPropertyChanged("IDStud");
            }
        }


        private int IDPredSelf;
        public int IDPred
        {
            get => IDPredSelf;
            set
            {
                IDPredSelf = value;
                OnPropertyChanged("IDPred");
            }
        }

        private string OcenkaSelf;
        public string Ocenka
        {
            get => OcenkaSelf;
            set
            {
                OcenkaSelf = value;
                OnPropertyChanged("Ocenka");
            }
        }


        private IList<ListItemSelectP> ListItemSelectPredSelf;
        public IList<ListItemSelectP> ListItemSelectPred
        {
            get => ListItemSelectPredSelf;
            set
            {
                ListItemSelectPredSelf = value;
                OnPropertyChanged("ListItemSelectPred");
            }
        }

        private IList<ListItemSelectS> ListItemSelectStudSelf;
        public IList<ListItemSelectS> ListItemSelectStud
        {
            get => ListItemSelectStudSelf;
            set
            {
                ListItemSelectStudSelf = value;
                OnPropertyChanged("ListItemSelectStud");
            }
        }
    }
}
