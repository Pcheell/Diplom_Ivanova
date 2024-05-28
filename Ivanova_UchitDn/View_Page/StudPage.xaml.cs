using Ivanova_UchitDn.Model;
using Ivanova_UchitDn.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ivanova_UchitDn.View_Page
{
    /// <summary>
    /// Логика взаимодействия для StudPage.xaml
    /// </summary>
    public partial class StudPage : Page
    {
        public StudPage()
        {
            InitializeComponent();
            GridData.DataContext = new StudData();
            DataContext = new StudData();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Проверяем, был ли клик вне границ содержимого окна добавления или редактирования
            if (e.Source == sender)
            {
                // Закрываем окно добавления или редактирования
                ShowInsertData.Visibility = Visibility.Collapsed;
                ShowEditData.Visibility = Visibility.Collapsed;
            }
        }

        private void OpenEdit(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                StudModel selectedUser = btn.DataContext as StudModel;
                if (selectedUser != null)
                {
                    StudData userData = GridData.DataContext as StudData;
                    if (userData != null)
                    {
                        userData.EditStud = selectedUser;
                    }
                    ShowEditData.Visibility = Visibility.Visible;
                }
            }
        }

        private void CloseEdit(object sender, RoutedEventArgs e)
        {
            ShowEditData.Visibility = Visibility.Collapsed;

        }
        private void CloseInsert(object sender, RoutedEventArgs e)
        {
            ShowInsertData.Visibility = Visibility.Collapsed;
        }

        private void OpenInsert(object sender, RoutedEventArgs e)
        {
            ShowInsertData.Visibility = Visibility.Visible;
        }
        private void SearchByDateOfBirth_Click(object sender, RoutedEventArgs e)
        {
            StudData studData = new StudData(); // Создание экземпляра класса StudData
            studData.SearchByDateOfBirth(); // Вызов метода через экземпляр класса
        }
    }
}
