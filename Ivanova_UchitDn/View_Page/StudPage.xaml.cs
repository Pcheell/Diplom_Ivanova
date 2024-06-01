using Ivanova_UchitDn.Core;
using Ivanova_UchitDn.Model;
using Ivanova_UchitDn.ViewModel;
using MySqlConnector;
using System.Collections.ObjectModel;
using System.Linq;
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
        private StudModel selectedStudent;
        public StudPage(int userId)
        {
            InitializeComponent();
            GridData.DataContext = new StudData(userId);
            GridDataRoditeli.DataContext = new RodData(userId);
            DataContext = new StudData(userId);
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Проверяем, был ли клик вне границ содержимого окна добавления или редактирования
            if (e.Source == sender)
            {
                // Закрываем окно добавления или редактирования
                ShowInsertData.Visibility = Visibility.Collapsed;
                ShowEditData.Visibility = Visibility.Collapsed;
                GridDataRoditeli.Visibility = Visibility.Collapsed;
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
                        // Установите выбранного ученика в качестве редактируемого ученика

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

        private void OpenRoditeli(object sender, RoutedEventArgs e)
        {

            Button btn = sender as Button;
            if (btn != null)
            {
                StudModel selectedUser = btn.DataContext as StudModel;
                if (selectedUser != null)
                {
                    selectedStudent = selectedUser;
                    LoadParentsForStudent(selectedStudent);
                }
            }
        }

        private async void LoadParentsForStudent(StudModel student)
        {
            Connector con = new Connector();
            string sql = "SELECT * FROM roditeli WHERE id_stud=@idStud;";
            MySqlCommand command = new MySqlCommand(sql, con.GetCon());
            command.Parameters.Add(new MySqlParameter("@idStud", student.IDStud));

            await con.GetOpen();
            MySqlDataReader reader = await command.ExecuteReaderAsync();

            ObservableCollection<RodModel> parents = new ObservableCollection<RodModel>();

            while (await reader.ReadAsync())
            {
                RodModel parent = new RodModel
                {
                    IDRod = reader.GetInt32("id_roditel"),
                    IDStud = reader.GetInt32("id_stud"),
                    FIORod = reader.GetString("FIO_roditel"),
                    Tel = reader.GetString("tel_rod"),
                    Adr = reader.GetString("address_rod"),
                    Rabota = reader.GetString("rabota_rod")
                };
                parents.Add(parent);
            }

            await con.GetClose();

            RodData rodData = GridDataRoditeli.DataContext as RodData;
            if (rodData != null)
            {
                rodData.RodList = parents;
                rodData.FIOStud = student.FIOStud; // Устанавливаем ФИО ученика
                GridDataRoditeli.Visibility = Visibility.Visible;
            }
        }


        private void CloseRoditeli(object sender, RoutedEventArgs e)
        {
            GridDataRoditeli.Visibility = Visibility.Collapsed;

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
