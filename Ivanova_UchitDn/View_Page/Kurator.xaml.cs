using Ivanova_UchitDn.Model;
using Ivanova_UchitDn.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Ivanova_UchitDn.View_Page
{
    /// <summary>
    /// Логика взаимодействия для Kurator.xaml
    /// </summary>
    public partial class Kurator : Page
    {
        public Kurator()
        {
            InitializeComponent();
            GridData.DataContext = new UserData();
           
        }

        private void CloseEdit(object sender, RoutedEventArgs e)
        {
            ShowEditData.Visibility = Visibility.Collapsed;

        }

        private void OpenEdit(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                User selectedUser = btn.DataContext as User;
                if (selectedUser != null)
                {
                    // Получаем DataContext в качестве типа UserData
                    UserData userData = GridData.DataContext as UserData;
                    if (userData != null)
                    {
                        // Передаем выбранного пользователя в свойство EditUser
                        userData.EditUser = selectedUser;
                    }
                    // Показываем окно редактирования
                    ShowEditData.Visibility = Visibility.Visible;
                }
            }
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
