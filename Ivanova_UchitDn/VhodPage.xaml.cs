using Ivanova_UchitDn.Core;
using Ivanova_UchitDn.Model;
using MySqlConnector;
using System.Threading.Tasks;
using System.Windows;


namespace Ivanova_UchitDn
{
    /// <summary>
    /// Логика взаимодействия для VhodPage.xaml
    /// </summary>
    public partial class VhodPage : Window
    {
        private int userId;
        public VhodPage()
        {
            InitializeComponent();
        }

        private async void Btn_Vhod(object sender, RoutedEventArgs e)
        {
            userId = await GetUserId(); // Сохраняем ID пользователя

            if (userId == -1)
            {
                MessageBox.Show("Неправильно введен логин или пароль");
                return;
            }

            string login = LoginTxt.Text;
            // Проверяем, если FIO_kurator = "admin", то переходим на меню администратора
            if (login == "admin")
            {
                MainWindow menu = new MainWindow(userId); // Передаем ID пользователя в конструктор MainWindow
                menu.Owner = this;
                menu.Show();
                this.Hide();
            }
            else
            {
                // В остальных случаях оставляем текущее поведение
                MainWindowKurator menuKurator = new MainWindowKurator(userId); // Передаем ID пользователя в конструктор MainWindowKurator
                menuKurator.Owner = this;
                menuKurator.Show();
                this.Hide();
            }
        }

        private async Task<int> GetUserId()
        {
            Connector con = new Connector();
            string sql = string.Format("SELECT id_kurator FROM `kurator` WHERE login = @l AND parol = @p");

            MySqlCommand command = new MySqlCommand(sql, con.GetCon());
            command.Parameters.Add(new MySqlParameter("@l", LoginTxt.Text));
            command.Parameters.Add(new MySqlParameter("@p", ParolTxt.Text));

            await con.GetOpen();

            MySqlDataReader reader = await command.ExecuteReaderAsync();

            int userId = -1;

            if (reader.HasRows)
            {
                reader.Read();
                userId = reader.GetInt32(0);
            }

            await con.GetClose();
            return userId;
        }
    }
}