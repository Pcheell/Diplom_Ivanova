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
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Скрываем поле для фактического адреса
            FAdrTextBox.Visibility = Visibility.Collapsed;
            FAdrText.Visibility = Visibility.Collapsed;

            // Дублируем значение из поля для адреса проживания в поле для фактического адреса
            FAdrTextBox.Text = AdrTextBox.Text;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Показываем поле для фактического адреса
            FAdrTextBox.Visibility = Visibility.Visible;
            FAdrText.Visibility = Visibility.Visible;

        }

        private void CheckBoxEdit_Checked(object sender, RoutedEventArgs e)
        {
            // Скрываем поле для фактического адреса
            FAdrTextBoxEdit.Visibility = Visibility.Collapsed;
            FAdrTextEdit.Visibility = Visibility.Collapsed;

            // Дублируем значение из поля для адреса проживания в поле для фактического адреса
            FAdrTextBoxEdit.Text = AdrTextBoxEdit.Text;
        }

        private void CheckBoxEdit_Unchecked(object sender, RoutedEventArgs e)
        {
            // Показываем поле для фактического адреса
            FAdrTextBoxEdit.Visibility = Visibility.Visible;
            FAdrTextEdit.Visibility = Visibility.Visible;

        }

        private void AdrTextBoxEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Если чекбокс "Совпадает с фактическим" отмечен, обновляем текст в поле фактического адреса
            if (FactCheckBox.IsChecked == true)
            {
                FAdrTextBoxEdit.Text = AdrTextBoxEdit.Text;
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
            
        }
    }
}
