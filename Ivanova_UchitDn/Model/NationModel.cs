using Ivanova_UchitDn.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static Ivanova_UchitDn.Core.CoreApp;

namespace Ivanova_UchitDn.Model
{
    public class NationModel : INotifyPropertyChanged
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

        private int IDNationSelf;
        public int IDNation
        {
            get => IDNationSelf;
            set
            {
                IDNationSelf = value;
                OnPropertyChanged("IDNation");
            }
        }

       
        private string NameNationSelf;
        public string NameNation
        {
            get => NameNationSelf;
            set
            {
                NameNationSelf = value;
                OnPropertyChanged("NameNation");
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