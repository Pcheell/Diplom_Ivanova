using Ivanova_UchitDn.Core;
using Ivanova_UchitDn.Model;
using Ivanova_UchitDn.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Ivanova_UchitDn.View_Page
{
    /// <summary>
    /// Логика взаимодействия для GrupPage.xaml
    /// </summary>
    public partial class GrupPage : Page
    {
        private GrupData grupData;

        public GrupPage()
        {
            InitializeComponent();
            GridData.DataContext = new GrupData();
        
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
                GrupModel selectedUser = btn.DataContext as GrupModel;
                if (selectedUser != null)
                {
                    GrupData userData = GridData.DataContext as GrupData;
                    if (userData != null)
                    {
                        userData.EditGroup = selectedUser;
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
