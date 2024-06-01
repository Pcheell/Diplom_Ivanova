using Ivanova_UchitDn.Model;
using Ivanova_UchitDn.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ivanova_UchitDn.View_Page
{
    /// <summary>
    /// Логика взаимодействия для RoditeliPage.xaml
    /// </summary>
    public partial class RoditeliPage : Page
    {
        public RoditeliPage(int userId)
        {
            InitializeComponent();
            GridData.DataContext = new RodData(userId);
            DataContext = new RodData(userId);
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
                RodModel selectedUser = btn.DataContext as RodModel;
                if (selectedUser != null)
                {
                    RodData userData = GridData.DataContext as RodData;
                    if (userData != null)
                    {
                        userData.EditRod = selectedUser;
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
     
    }
}
