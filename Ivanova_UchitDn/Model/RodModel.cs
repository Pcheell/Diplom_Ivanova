using Ivanova_UchitDn.Core;
using System.Collections.Generic;
using System.ComponentModel;
using static Ivanova_UchitDn.Core.CoreApp;

namespace Ivanova_UchitDn.Model
{
    public class RodModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Виртуальный метод для оповещения об изменениях
        /// </summary>
        /// <param name="property"></param>
        protected virtual void OnPropertyChanged(string property)
        {
            if (property == null)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private int IDRodSelf;
        public int IDRod
        {
            get => IDRodSelf;
            set
            {
                IDRodSelf = value;
                OnPropertyChanged("IDRod");
            }
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


        private string FIORodSelf;
        public string FIORod
        {
            get => FIORodSelf;
            set
            {
                FIORodSelf = value;
                OnPropertyChanged("FIORod");
            }
        }


        private string AdrSelf;

        public string Adr
        {
            get => AdrSelf;
            set
            {
                AdrSelf = value;
                OnPropertyChanged("Adr");
            }
        }

        private string TelSelf;

        public string Tel
        {
            get => TelSelf;
            set
            {
                TelSelf = value;
                OnPropertyChanged("Tel");
            }
        }

        private string RabotaSelf;

        public string Rabota
        {
            get => RabotaSelf;
            set
            {
                RabotaSelf = value;
                OnPropertyChanged("Rabota");
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

        private DeleteCommand DeleteSelf;
        public DeleteCommand Delete
        {
            get => DeleteSelf;
            set => DeleteSelf = value;
        }
    }
}
