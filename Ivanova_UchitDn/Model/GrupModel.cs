using Ivanova_UchitDn.Core;
using Ivanova_UchitDn.ViewModel;
using System.Collections.Generic;
using System.ComponentModel;
using static Ivanova_UchitDn.Core.CoreApp;

namespace Ivanova_UchitDn.Model
{
    public class GrupModel : INotifyPropertyChanged
    {
        private GrupData grupData;
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
        /// Свойство id_kurator
        /// </summary>
        private int IDKurSelf;

        /// <summary>
        /// Реализация свойства id_kurator
        /// </summary>
        public int IDKur
        {
            get => IDKurSelf;
            set
            {
                IDKurSelf = value;
                OnPropertyChanged("IDKur");
            }
        }

        private string NameGrupSelf;

        /// <summary>
        /// Реализация свойства NameGrup
        /// </summary>
        public string NameGrup
        {
            get => NameGrupSelf;
            set
            {
                NameGrupSelf = value;
                OnPropertyChanged("NameGrup");
            }
        }

        private IList<ListItemSelect> ListItemSelectKurSelf;
        public IList<ListItemSelect> ListItemSelectKur
        {
            get => ListItemSelectKurSelf;
            set
            {
                ListItemSelectKurSelf = value;
                OnPropertyChanged("ListItemSelectKur");
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
