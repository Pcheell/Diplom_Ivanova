using Ivanova_UchitDn.Core;
using Ivanova_UchitDn.Model;
using MySqlConnector;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace Ivanova_UchitDn.ViewModel
{
    public class UserData : INotifyPropertyChanged
    {
        private bool isUpdating = false;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string property)
        {
            if (property == null)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private ObservableCollection<User> UsersSelf;
        public IList<User> Users
        {
            get => UsersSelf;
            set
            {
                UsersSelf = value as ObservableCollection<User>;
                OnPropertyChanged("Users");
            }
        }

        private User NewUserSelf;
        public User NewUser
        {
            get => NewUserSelf;
            set
            {
                NewUserSelf = value;
                OnPropertyChanged("NewUser");
            }
        }

        public UserData()
        {
            LoadData();
        }

        private async void LoadData()
        {
            if (isUpdating)
            {
                // Если уже идет процесс обновления данных, прерываем выполнение метода
                return;
            }
            isUpdating = true;


            NewUser = new User();
            EditUser = new User();
            await UserDataSelect();
            isUpdating = false;
        }

        private async Task<bool> UserDataSelect()
        {
            Connector con = new Connector();
            string sql = string.Format("select * from `kurator` {0} limit {1}", SearchTypes(), 999);
            MySqlCommand command = new MySqlCommand(sql, con.GetCon());

            Debug.WriteLine(sql);
            command.Parameters.Add(new MySqlParameter("@text", string.Format("%{0}%", SearchText)));

            await con.GetOpen();
            UsersSelf = new ObservableCollection<User>();

            MySqlDataReader reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows)
            {
                await con.GetClose();
                OnPropertyChanged("Users");
                return false;
            }

            while (await reader.ReadAsync())
            {
                await Task.Delay(1);
                UsersSelf.Add(new User()
                {
                    ID = (int)reader[0],
                    Name = (string)reader[1],
                    Login = (string)reader[2],
                    Parol = (string)reader[3],
                    Delete = new DeleteCommand(DeleteData, (int)reader[0])

                });

                OnPropertyChanged("Users");
            }

            await con.GetClose();
            OnPropertyChanged("Users");
            return true;
        }

     
        private string SearchTypes()
        {
            string sql = "";

            // Если ни один чекбокс не выбран, добавляем условия поиска для всех полей
            if (!SearchName && !SearchLog && !SearchParol)
            {
                sql = "`FIO_kurator` LIKE @text OR `login` LIKE @text OR `parol` LIKE @text";
            }
            else
            {
                List<string> conditions = new List<string>();

                // Добавляем условия поиска в зависимости от выбранных чекбоксов
                if (SearchName)
                    conditions.Add("`FIO_kurator` LIKE @text");
                if (SearchLog)
                    conditions.Add("`login` LIKE @text");
                if (SearchParol)
                    conditions.Add("`parol` LIKE @text");

                // Соединяем условия с помощью оператора OR
                sql = string.Join(" OR ", conditions);
            }

            return SearchTypesSet(sql);
        }

        private string SearchTypesSet(string sql)
        {
            return string.IsNullOrEmpty(sql) ? sql : " where " + sql;
        }

        private void SearchTypesAnd(ref string sql, string value)
        {
            sql += string.IsNullOrEmpty(sql) ? value : " and " + value;
        }



        private UpdateData UpdateSelf;
        public UpdateData Update
        {
            get
            {
                if (UpdateSelf == null)
                    UpdateSelf = new UpdateData(LoadData);

                return UpdateSelf;
            }
        }


        public User EditUserSelf { get; set; }
        public User EditUser
        {
            get => EditUserSelf;
            set
            {
                if (value == null)
                {
                    EditUserSelf = new User();
                    return;
                }

                EditUserSelf = new User()
                {
                    ID = value.ID,
                    Name = value.Name,
                    Login = value.Login,
                    Parol = value.Parol
                };


                OnPropertyChanged("EditUser");
            }
        }

        public InsertCommand InsertSelf { get; set; }
        public InsertCommand Insert
        {
            get
            {
                if (InsertSelf == null)
                    InsertSelf = new InsertCommand(InsertData);

                return InsertSelf;
            }
        }


        public ReloadCommand ReloadSelf { get; set; }
        public ReloadCommand Reload
        {
            get
            {
                if (ReloadSelf == null)
                    ReloadSelf = new ReloadCommand(LoadData);

                return ReloadSelf;
            }
        }

        public EditCommand EditSelf { get; set; }
        public EditCommand Edit
        {
            get
            {
                if (EditSelf == null)
                    EditSelf = new EditCommand(EditData);

                return EditSelf;
            }
        }

       
        private async void InsertData()
        {
            if (string.IsNullOrEmpty(NewUserSelf.Name))
            {
                MessageBox.Show("Не указано ФИО");
                return;
            }
            if (string.IsNullOrEmpty(NewUserSelf.Login))
            {
                MessageBox.Show("Не указан логин");
                return;
            }
            if (string.IsNullOrEmpty(NewUserSelf.Parol))
            {
                MessageBox.Show("Не указано пароль");
                return;
            }


            Connector
                 con = new Connector();
            string
                sql = "INSERT INTO `kurator` (FIO_kurator, login, parol) VALUES (@f, @l, @p);";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@f", NewUserSelf.Name));
            command.Parameters.Add(new MySqlParameter("@l", NewUserSelf.Login));
            command.Parameters.Add(new MySqlParameter("@p", NewUserSelf.Parol));

            await con.GetOpen();

            if (await command.ExecuteNonQueryAsync() != 1)
            {
                await con.GetClose();
                MessageBox.Show("Таблица не добавлена", "Ошибка");
                return;

            }
            await con.GetClose();
            LoadData();
            MessageBox.Show("Таблица добавлена", "Подтверждение");
        }



        public async void DeleteData(int a)
        {
            if (MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            Connector 
                 con = new Connector();
            string
                sql = "DELETE FROM `kurator` WHERE `id_kurator` = @i;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

           
            command.Parameters.Add(new MySqlParameter("@i", a));

            await con.GetOpen();

            if (await command.ExecuteNonQueryAsync() != 1)
            {
                await con.GetClose();
                MessageBox.Show("Запись не удалена", "Ошибка");
                return;
            }

            await con.GetClose();
            LoadData();
            MessageBox.Show("Запись удалена", "Подтверждение");
        }


        private async void EditData(object element)
        {
            if (string.IsNullOrEmpty(EditUser.Name))
            {
                MessageBox.Show("Не указано ФИО");
                return;
            }
            if (string.IsNullOrEmpty(EditUser.Login))
            {
                MessageBox.Show("Не указан логин");
                return;
            }
            if (string.IsNullOrEmpty(EditUser.Parol))
            {
                MessageBox.Show("Не указано пароль");
                return;
            }

            if (element is Grid g)
                g.Visibility = Visibility.Collapsed;

            Connector
                 con = new Connector();
            string
                sql = "UPDATE `kurator` SET `FIO_kurator`=@f, `login`=@l, `parol`=@p WHERE `id_kurator`=@i;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@f", EditUser.Name));
            command.Parameters.Add(new MySqlParameter("@l", EditUser.Login));
            command.Parameters.Add(new MySqlParameter("@p", EditUser.Parol));
            command.Parameters.Add(new MySqlParameter("@i", EditUser.ID));

            await con.GetOpen();

            if (await command.ExecuteNonQueryAsync() != 1)
            {
                await con.GetClose();
                MessageBox.Show("Запись не изменена", "Ошибка");
                return;
            }

            await con.GetClose();
            LoadData();
            MessageBox.Show("Запись изменена", "Успех");
        }


        public SearchCommand SearchSelf { get; set; }
        public SearchCommand Search
        {
            get
            {
                if (SearchSelf == null)
                    SearchSelf = new SearchCommand(LoadData);

                return SearchSelf;
            }
        }

        public bool SearchNameSelf;
        public bool SearchName
        {
            get => SearchNameSelf;
            set
            {
                SearchNameSelf = value;
                OnPropertyChanged("SearchName");
                LoadData();
            }
        }

        public bool SearchLogSelf;
        public bool SearchLog
        {
            get => SearchLogSelf;
            set
            {
                SearchLogSelf = value;
                OnPropertyChanged("SearchLog");
                LoadData();
            }
        }

        public bool SearchParolSelf;
        public bool SearchParol
        {
            get => SearchParolSelf;
            set
            {
                SearchParolSelf = value;
                OnPropertyChanged("SearchParol");
                LoadData();
            }
        }

        private string SearchTextSelf;
        public string SearchText
        {
            get => SearchTextSelf;
            set
            {
                SearchTextSelf = value;
                OnPropertyChanged("SearchText");
                LoadData();
            }
        }

     



    }
}
