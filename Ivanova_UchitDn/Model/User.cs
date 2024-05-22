
using Ivanova_UchitDn.Core;
using System.ComponentModel;

namespace Ivanova_UchitDn.Model
{
    public class User : INotifyPropertyChanged
    {
        /// <summary>
        /// Событие оповещения об изменениях
        /// </summary>
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

        private int IDSelf;
        public int ID
        {
            get => IDSelf;
            set
            {
                IDSelf = value;
                OnPropertyChanged("ID");
            }
        }

       
        private string NameSelf;
        public string Name
        {
            get => NameSelf;
            set
            {
                NameSelf = value;
                OnPropertyChanged("Name");
            }
        }

        private string LoginSelf;
        public string Login
        {
            get => LoginSelf;
            set
            {
                LoginSelf = value;
                OnPropertyChanged("Login");
            }
        }

        private string ParolSelf;
        public string Parol
        {
            get => ParolSelf;
            set
            {
                ParolSelf = value;
                OnPropertyChanged("Parol");
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
