using Ivanova_UchitDn.Core;
using Ivanova_UchitDn.Model;
using MySqlConnector;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Ivanova_UchitDn.Core.CoreApp;

namespace Ivanova_UchitDn.ViewModel
{
    public class RodData : INotifyPropertyChanged
    {
        private bool isUpdating = false;

        /// <summary>
        /// Событие оповещения об изменениях
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Виртуальный метод для оповещения об изменениях
        /// </summary>
        /// <param name="property"></param>
        protected virtual void OnPropertyChanged(string property)
        {
            if (property == null)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }




        private IList<ListItemSelectS> ListItemSelectStudSelf;
        public IList<ListItemSelectS> ListItemSelectStud
        {
            get => ListItemSelectStudSelf;
            set => ListItemSelectStudSelf = value;
        }

      
        private IList<RodModel> RodsSelf;
        public IList<RodModel> Users
        {
            get => RodsSelf;
            set => RodsSelf = value;
        }


        private RodModel NewRodSelf;
        public RodModel NewRod
        {
            get => NewRodSelf;
            set
            {
                NewRodSelf = value;
                OnPropertyChanged("NewRod");
            }
        }


        public RodData()
        {
            ListItemSelectStud = new ObservableCollection<ListItemSelectS>();
            LoadData();
        }

        private async void LoadData()
        {
            if (isUpdating)
            {
                return;
            }
            isUpdating = true;

            NewRod = new RodModel();
            EditRod = new RodModel();
            _ = await StudDataList();
            OnPropertyChanged("SearchSelectStud");
            _ = await RodDataList();

            isUpdating = false;

        }

        private async Task<bool> StudDataList()
        {
            Connector
                 con = new Connector();
            string
                sql = "select * from `kart_stud` limit 50";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            await con.GetOpen();

            MySqlDataReader
                reader = await command.ExecuteReaderAsync();

            ListItemSelectStudSelf = new ObservableCollection<ListItemSelectS>() { new ListItemSelectS() { FIOStud = "Все" } };

            if (!reader.HasRows)
            {
                await con.GetClose();
                return false;
            }


            while (await reader.ReadAsync())
            {
                await Task.Delay(1);
                ListItemSelectStudSelf.Add(new ListItemSelectS()
                {
                    IDStud = (int)reader["id_stud"],
                    FIOStud = string.Format("{0}", reader["FIO_stud"]),

                });

                OnPropertyChanged("ListItemSelectStud");
            }

            NewRodSelf.ListItemSelectStud = ListItemSelectStudSelf;
            await con.GetClose();
            return true;
        }


        private async Task<bool> RodDataList()
        {
            Connector
                 con = new Connector();
            string
                sql = string.Format("select * from `roditeli` {0} limit {1}", SearchTypes(), 999);

            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@text", string.Format("%{0}%", SearchText)));
            command.Parameters.Add(new MySqlParameter("@kart_stud", SearchSelectStud));

            await con.GetOpen();
            RodsSelf = new ObservableCollection<RodModel>();

            MySqlDataReader
                reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows)
            {
                await con.GetClose();
                OnPropertyChanged("Users");
                return false;
            }


            while (await reader.ReadAsync())
            {
                await Task.Delay(1);
                RodsSelf.Add(new RodModel()
                {
                    IDRod = (int)reader["id_roditel"],
                    IDStud = (int)reader["id_stud"],
                    ListItemSelectStud = ListItemSelectStudSelf,
                    FIORod = (string)reader["FIO_roditel"],
                    Adr = (string)reader["address_rod"],
                    Tel = (string)reader["tel_rod"],
                    Rabota = (string)reader["rabota_rod"],
                    Delete = new DeleteCommand(DeleteData, (int)reader[0])

                });

                OnPropertyChanged("Users");
            }

            await con.GetClose();
            return true;
        }

        private string SearchTypes()
        {
            string
                sql = "";

            if (!GroupInsertNotValid(SearchSelectStud))
                SearchTypesAnd(ref sql, "`id_stud` = @kart_stud");


            if (SearchStud)
            {
                SearchTypesAnd(ref sql, "`id_stud` IN (SELECT `id_stud` FROM `kart_stud` WHERE `FIO_stud` LIKE @text)");
            }

            // Если ни один чекбокс не выбран, добавляем условия поиска для всех полей
            if (!SearchName && !SearchAdr && !SearchTel && !string.IsNullOrEmpty(SearchText))
            {
                sql = "`FIO_roditel` LIKE @text OR `address_rod` LIKE @text OR `tel_rod` LIKE @text " +
                    "OR `rabota_rod` LIKE @text OR `id_stud` IN (SELECT `id_stud` FROM `kart_stud` WHERE `FIO_stud` LIKE @text)";
            }
            else
            {
                List<string> conditions = new List<string>();

                // Добавляем условия поиска в зависимости от выбранных чекбоксов
                if (SearchName)
                    conditions.Add("`FIO_roditel` LIKE @text");
                if (SearchAdr)
                    conditions.Add("`address_rod` LIKE @text");
                if (SearchTel)
                    conditions.Add("`tel_rod` LIKE @text");
                if (SearchRab)
                    conditions.Add("`rabota_rod` LIKE @text");
                if (SearchStud)
                    conditions.Add("`id_stud` IN (SELECT `id_stud` FROM `kart_stud` WHERE `FIO_stud` LIKE @text)");


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

        public RodModel EditDataSelf { get; set; }
        public RodModel EditRod
        {
            get => EditDataSelf;
            set
            {
                if (value == null)
                {
                    EditDataSelf = new RodModel();
                    return;
                }

                EditDataSelf = new RodModel()
                {
                    IDRod = value.IDRod,
                    IDStud = value.IDStud,
                    ListItemSelectStud = ListItemSelectStudSelf,
                    FIORod = value.FIORod,
                    Adr = value.Adr,
                    Tel = value.Tel,
                    Rabota = value.Rabota
                };

                OnPropertyChanged("EditRod");
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
            if (GroupInsertNotValid(NewRodSelf.IDStud))
            {
                MessageBox.Show("Не выбран куратор");
                return;
            }

            if (string.IsNullOrEmpty(NewRodSelf.FIORod))
            {
                MessageBox.Show("Не указано ФИО");
                return;
            }

            if (string.IsNullOrEmpty(NewRodSelf.Adr))
            {
                MessageBox.Show("Не указан адрес");
                return;
            }

            if (string.IsNullOrEmpty(NewRodSelf.Tel))
            {
                MessageBox.Show("Не указан телефон");
                return;
            }

            if (string.IsNullOrEmpty(NewRodSelf.Rabota))
            {
                MessageBox.Show("Не указана работа");
                return;
            }

            Connector
                 con = new Connector();
            string
                sql = "INSERT INTO `roditeli` (id_stud, FIO_roditel, address_rod, tel_rod, rabota_rod) " +
            "VALUES (@i, @f, @a, @t, @r);";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@i", NewRodSelf.IDStud));
            command.Parameters.Add(new MySqlParameter("@f", NewRodSelf.FIORod));
            command.Parameters.Add(new MySqlParameter("@a", NewRodSelf.Adr));
            command.Parameters.Add(new MySqlParameter("@t", NewRodSelf.Tel));
            command.Parameters.Add(new MySqlParameter("@r", NewRodSelf.Rabota));

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

      
        private async void DeleteData(int a)
        {
            if (MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            Connector
                 con = new Connector();
            string
                sql = "DELETE FROM `roditeli` WHERE `id_roditel` = @i;";
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
            if (GroupInsertNotValid(EditRod.IDStud))
            {
                MessageBox.Show("Не выбран куратор");
                return;
            }

            if (string.IsNullOrEmpty(EditRod.FIORod))
            {
                MessageBox.Show("Не указано ФИО");
                return;
            }

            if (string.IsNullOrEmpty(EditRod.Adr))
            {
                MessageBox.Show("Не указан адрес");
                return;
            }

            if (string.IsNullOrEmpty(EditRod.Tel))
            {
                MessageBox.Show("Не указан телефон");
                return;
            }

            if (string.IsNullOrEmpty(EditRod.Rabota))
            {
                MessageBox.Show("Не указана работа");
                return;
            }

            if (element is Grid g)
                g.Visibility = Visibility.Collapsed;

            Connector
                 con = new Connector();
            string
                sql = "UPDATE `roditeli` SET `id_stud`=@id, `FIO_roditel`=@f, `address_rod`=@a, `tel_rod`=@t, `rabota_rod`=@r WHERE `id_roditel`=@i;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());


            command.Parameters.Add(new MySqlParameter("@id",EditRod.IDStud));
            command.Parameters.Add(new MySqlParameter("@f", EditRod.FIORod));
            command.Parameters.Add(new MySqlParameter("@a", EditRod.Adr));
            command.Parameters.Add(new MySqlParameter("@t", EditRod.Tel));
            command.Parameters.Add(new MySqlParameter("@r", EditRod.Rabota));
            command.Parameters.Add(new MySqlParameter("@i", EditRod.IDRod));


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

        private bool GroupInsertNotValid(int key)
        {

            if (key < 1)
                return true;

            foreach (ListItemSelectS group in ListItemSelectStud)
            {
                if (group.IDStud != key)
                    continue;

                return false;
            }
            return true;
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
        
        public bool SearchStepSelf;
        public bool SearchStep
        {
            get => SearchStepSelf;
            set
            {
                SearchStepSelf = value;
                OnPropertyChanged("SearchStep");
                LoadData();

            }
        }
        
        public bool SearchAdrSelf;
        public bool SearchAdr
        {
            get => SearchAdrSelf;
            set
            {
                SearchAdrSelf = value;
                OnPropertyChanged("SearchAdr");
                LoadData();

            }
        }

        public bool SearchStudSelf;
        public bool SearchStud
        {
            get => SearchStudSelf;
            set
            {
                SearchStudSelf = value;
                OnPropertyChanged("SearchStud");
                LoadData();

            }
        }

        public bool SearchTelSelf;
        public bool SearchTel
        {
            get => SearchTelSelf;
            set
            {
                SearchTelSelf = value;
                OnPropertyChanged("SearchTel");
                LoadData();

            }
        }

        private bool SearchRabSelf;
        public bool SearchRab
        {
            get => SearchRabSelf;
            set
            {
                SearchRabSelf = value;
                OnPropertyChanged("SearchRab");
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

        private int SearchSelectStudSelf;
        public int SearchSelectStud
        {
            get => SearchSelectStudSelf;
            set { SearchSelectStudSelf = value; LoadData(); OnPropertyChanged("SearchSelectStud"); }
        }

    }
}