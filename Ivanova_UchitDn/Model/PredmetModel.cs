using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivanova_UchitDn.Model
{
    public class PredmetModel : INotifyPropertyChanged
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
        /// Свойство id_predmet
        /// </summary>
        private int IDPredSelf;

        /// <summary>
        /// Реализация свойства id_predmet
        /// </summary>
        public int IDPred
        {
            get => IDPredSelf;
            set
            {
                IDPredSelf = value;
                OnPropertyChanged("IDPred");
            }
        }

        /// <summary>
        /// Свойство name_pred
        /// </summary>
        private string NamePredSelf;

        /// <summary>
        /// Реализация свойства name_pred
        /// </summary>
        public string NamePred
        {
            get => NamePredSelf;
            set
            {
                NamePredSelf = value;
                OnPropertyChanged("NamePred");
            }
        }

        /// <summary>
        /// Свойство KolChas
        /// </summary>
        private int KolChasSelf;

        /// <summary>
        /// Реализация свойства KolChas
        /// </summary>
        public int KolChas
        {
            get => KolChasSelf;
            set
            {
                KolChasSelf = value;
                OnPropertyChanged("KolChas");
            }
        }
    }
}
