
using Ivanova_UchitDn.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static Ivanova_UchitDn.Core.CoreApp;

namespace Ivanova_UchitDn.Model
{
    public class StudModel : INotifyPropertyChanged
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

        /// <summary>
        /// Свойство id_stud
        /// </summary>
        private int IDStudSelf;

        /// <summary>
        /// Реализация свойства id_stud
        /// </summary>
        public int IDStud
        {
            get => IDStudSelf;
            set
            {
                IDStudSelf = value;
                OnPropertyChanged("IDStud");
            }
        }

        /// <summary>
        /// Свойство id_grup
        /// </summary>
        private int IDGrupSelf;

        /// <summary>
        /// Реализация свойства id_grup
        /// </summary>
        public int IDGrup
        {
            get => IDGrupSelf;
            set
            {
                IDGrupSelf = value;
                OnPropertyChanged("IDGrup");
            }
        }


        /// <summary>
        /// Свойство FIO_stid
        /// </summary>
        private string FIOStudSelf;

        /// <summary>
        /// Реализация свойства FIO_stid
        /// </summary>
        public string FIOStud
        {
            get => FIOStudSelf;
            set
            {
                FIOStudSelf = value;
                OnPropertyChanged("FIOStud");
            }
        }


        private DateTime DRStudSelf;
        public DateTime DRStud
        {
            get => DRStudSelf;
            set
            {
                DRStudSelf = value;
                OnPropertyChanged("DRStud");
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


        private IList<ListItemSelectG> ListItemSelectGrupSelf;
        public IList<ListItemSelectG> ListItemSelectGrup
        {
            get => ListItemSelectGrupSelf;
            set
            {
                ListItemSelectGrupSelf = value;
                OnPropertyChanged("ListItemSelectGrup");
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