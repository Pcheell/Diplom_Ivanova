using Ivanova_UchitDn.Core;
using Ivanova_UchitDn.View_Page;
using Ivanova_UchitDn.ViewModel;
using MySqlConnector;
using System;
using System.Windows;
using System.Windows.Controls;


namespace Ivanova_UchitDn
{
    /// <summary>
    /// Логика взаимодействия для MainWindowKurator.xaml
    /// </summary>
    public partial class MainWindowKurator : Window
    {
        private int userId; // Переменная для хранения ID пользователя

        public MainWindowKurator(int userId)
        {
            InitializeComponent();
            MyFrame.NavigationService.Navigate(new StudPage(userId));

            this.userId = userId; // Сохраняем ID пользователя
            var studData = new StudData(userId);
            var rodData = new RodData(userId);

            LoadKuratorFIO();
        }

        private async void LoadKuratorFIO()
        {
            try
            {
                Connector con = new Connector();
                string sql = "SELECT FIO_kurator FROM kurator WHERE id_kurator = @userId";
                MySqlCommand command = new MySqlCommand(sql, con.GetCon());
                command.Parameters.Add(new MySqlParameter("@userId", userId));

                await con.GetOpen();
                MySqlDataReader reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    KuratorFIO.Text = reader["FIO_kurator"].ToString();
                }

                await con.GetClose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке ФИО куратора: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Stud_btn(object sender, RoutedEventArgs e)
        {
            MyFrame.NavigationService.Navigate(new StudPage(userId));
        }

        private void Rod_btn(object sender, RoutedEventArgs e)
        {
            MyFrame.NavigationService.Navigate(new RoditeliPage(userId));
        }

        private void Nat_btn(object sender, RoutedEventArgs e)
        {
            MyFrame.NavigationService.Navigate(new NationPage());

        }

        private void LoginPageOpen(object sender, RoutedEventArgs e)
        {
            if (this.Owner is VhodPage v)
            {
                v.LoginTxt.Text = "";
                v.ParolTxt.Text = "";
            }

            this.Owner.Show();
            this.Close();
        }

    }
}
