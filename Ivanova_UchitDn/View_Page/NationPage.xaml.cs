using Ivanova_UchitDn.Model;
using Ivanova_UchitDn.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ivanova_UchitDn.View_Page
{
    /// <summary>
    /// Логика взаимодействия для NationPage.xaml
    /// </summary>
    public partial class NationPage : Page
    {
        public NationPage()
        {
            InitializeComponent();
            GridData.DataContext = new NationData();

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
                NationModel selectedNatiion = btn.DataContext as NationModel;
                if (selectedNatiion != null)
                {
                    // Получаем DataContext в качестве типа UserData
                    NationData nationData = GridData.DataContext as NationData;
                    if (nationData != null)
                    {
                        // Передаем выбранного пользователя в свойство EditUser
                        nationData.EditNation = selectedNatiion;
                    }
                    // Показываем окно редактирования
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
