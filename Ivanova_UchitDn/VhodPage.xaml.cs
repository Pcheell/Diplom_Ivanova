using Ivanova_UchitDn.Core;
using Ivanova_UchitDn.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ivanova_UchitDn
{
    /// <summary>
    /// Логика взаимодействия для VhodPage.xaml
    /// </summary>
    public partial class VhodPage : Window
    {
        public VhodPage()
        {
            InitializeComponent();
        }

        private async void Btn_Vhod(object sender, RoutedEventArgs e)
        {
            if (await UserDataSelectFalse())
            {
                MessageBox.Show("Неправильно введен логин или пароль");
                return;
            }

            MainWindow menu = new MainWindow();

            menu.Owner = this;
            menu.Show();
            this.Hide();
        }

        private async Task<bool> UserDataSelectFalse()
        {
            Connector
                 con = new Connector();
            string
                sql = string.Format("select * from `kurator`  where login = @l and parol = @p ");

            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());
 
            command.Parameters.Add(new MySqlParameter("@l", LoginTxt.Text));
            command.Parameters.Add(new MySqlParameter("@p", ParolTxt.Text));

            await con.GetOpen();

            MySqlDataReader
                reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows)
            {
                await con.GetClose();
                return true;
            }

            await con.GetClose();
            return false;
        }
    }
}
