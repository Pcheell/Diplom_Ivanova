using Ivanova_UchitDn.Core;
using Ivanova_UchitDn.Model;
using MySqlConnector;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace Ivanova_UchitDn.ViewModel
{
    public class NationData : INotifyPropertyChanged
    {
        private bool isUpdating = false;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string property)
        {
            if (property == null)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private ObservableCollection<NationModel> NationsSelf;
        public IList<NationModel> Nations
        {
            get => NationsSelf;
            set
            {
                NationsSelf = value as ObservableCollection<NationModel>;
                OnPropertyChanged("Nations");
            }
        }

        private NationModel NewNationSelf;
        public NationModel NewNation
        {
            get => NewNationSelf;
            set
            {
                NewNationSelf = value;
                OnPropertyChanged("NewNation");
            }
        }

        public NationData()
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


            NewNation = new NationModel();
            NewNation = new NationModel();
            await NationDataSelect();
            isUpdating = false;
        }

        private async Task<bool> NationDataSelect()
        {
            Connector
                con = new Connector();
            string
                sql = string.Format("select * from `nation` {0} limit {1}", SearchTypes(), 999);
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@text", string.Format("%{0}%", SearchText)));

            await con.GetOpen();
            NationsSelf = new ObservableCollection<NationModel>();

            MySqlDataReader reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows)
            {
                await con.GetClose();
                OnPropertyChanged("Nations");
                return false;
            }

            while (await reader.ReadAsync())
            {
                await Task.Delay(1);
                NationsSelf.Add(new NationModel()
                {
                    IDNation = (int)reader[0],
                    NameNation = (string)reader[1],
                    Delete = new DeleteCommand(DeleteData, (int)reader[0])

                });

                OnPropertyChanged("Nations");
            }

            await con.GetClose();
            OnPropertyChanged("Nations");
            return true;
        }


        private string SearchTypes()
        {
            string sql = "";
            sql = "`name_nation` LIKE @text";

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


        public NationModel EditNationSelf { get; set; }
        public NationModel EditNation
        {
            get => EditNationSelf;
            set
            {
                if (value == null)
                {
                    EditNationSelf = new NationModel();
                    return;
                }

                EditNationSelf = new NationModel()
                {
                    IDNation = value.IDNation,
                    NameNation = value.NameNation
                };


                OnPropertyChanged("EditNation");
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
            if (string.IsNullOrEmpty(NewNationSelf.NameNation))
            {
                MessageBox.Show("Не указано название национальности", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Connector
                 con = new Connector();
            string
                sql = "INSERT INTO `nation` (name_nation) VALUES (@n);";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@n", NewNationSelf.NameNation));

            await con.GetOpen();

            if (await command.ExecuteNonQueryAsync() != 1)
            {
                await con.GetClose();
                MessageBox.Show("Таблица не добавлена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }
            await con.GetClose();
            LoadData();
            MessageBox.Show("Таблица добавлена", "Подтверждение");
        }



        public async void DeleteData(int a)
        {
            if (MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            Connector
                 con = new Connector();
            string
                sql = "DELETE FROM `nation` WHERE `id_nation` = @i;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());


            command.Parameters.Add(new MySqlParameter("@i", a));

            await con.GetOpen();

            if (await command.ExecuteNonQueryAsync() != 1)
            {
                await con.GetClose();
                MessageBox.Show("Запись не удалена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await con.GetClose();
            LoadData();
            MessageBox.Show("Запись удалена", "Подтверждение");
        }


        private async void EditData(object element)
        {
            if (string.IsNullOrEmpty(EditNation.NameNation))
            {
                MessageBox.Show("Не указано название национальности");
                return;
            }


            if (element is Grid g)
                g.Visibility = Visibility.Collapsed;

            Connector
                 con = new Connector();
            string
                sql = "UPDATE `nation` SET `name_nation`=@n WHERE `id_nation`=@i;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@n", EditNation.NameNation));
            command.Parameters.Add(new MySqlParameter("@i", EditNation.IDNation));

            await con.GetOpen();

            if (await command.ExecuteNonQueryAsync() != 1)
            {
                await con.GetClose();
                MessageBox.Show("Запись не изменена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
