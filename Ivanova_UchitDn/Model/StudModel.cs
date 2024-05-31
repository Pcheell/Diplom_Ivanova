
using Ivanova_UchitDn.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
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

        private string FAdrSelf;
        public string FAdr
        {
            get => FAdrSelf;
            set
            {
                FAdrSelf = value;
                OnPropertyChanged("FAdr");
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

        private string SectionSelf;
        public string Section
        {
            get => SectionSelf;
            set
            {
                SectionSelf = value;
                OnPropertyChanged("Section");
            }
        }

        private string NoteSelf;
        public string Note
        {
            get => NoteSelf;
            set
            {
                NoteSelf = value;
                OnPropertyChanged("Note");
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

        private IList<ListItemSelectN> ListItemSelectNationSelf;
        public IList<ListItemSelectN> ListItemSelectNation
        {
            get => ListItemSelectNationSelf;
            set
            {
                ListItemSelectNationSelf = value;
                OnPropertyChanged("ListItemSelectNation");
            }
        }

        private DeleteCommand DeleteSelf;
        public DeleteCommand Delete
        {
            get => DeleteSelf;
            set => DeleteSelf = value;
        }

        private string _img;
        private const string DefaultImgPath = "pack://application:,,,/Ivanova_UchitDn;component/Images/avatar.jpg";

        public string Img
        {
            get => _img;
            set
            {
                _img = value;
                OnPropertyChanged(nameof(Img));
                OnPropertyChanged(nameof(ImgPath));
            }
        }
        public string ImgPath => string.IsNullOrEmpty(Img) || !File.Exists(Img) ? DefaultImgPath : Img;


        private ICommand _uploadPhotoCommand;

        public ICommand UploadPhotoCommand
        {
            get
            {
                return _uploadPhotoCommand ?? (_uploadPhotoCommand = new PhotoRelayCommand(UploadPhoto));
            }
        }

        private void UploadPhoto(object parameter)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.jpg, *.png)|*.jpg;*.png|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                string savedPath = PhotoUploader.UploadPhotoAndSavePath(selectedFilePath);
                Img = savedPath; // Устанавливаем свойство Img, чтобы обновить TextBox
            }
        }

        public class PhotoRelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Func<bool> _canExecute;

            public PhotoRelayCommand(Action<object> execute, Func<bool> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return _canExecute == null || _canExecute();
            }

            public void Execute(object parameter)
            {
                _execute(parameter);
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

    }
}