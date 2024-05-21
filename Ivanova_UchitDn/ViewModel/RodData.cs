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
using static Ivanova_UchitDn.Core.CoreApp;

namespace Ivanova_UchitDn.ViewModel
{
    public class RodData : INotifyPropertyChanged
    {
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
            NewRod = new RodModel();
            EditRod = new RodModel();
            _ = await StudDataList();
            OnPropertyChanged("SearchSelectStud");
            _ = await RodDataList();

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
                await Task.Delay(100);
                ListItemSelectStudSelf.Add(new ListItemSelectS()
                {
                    IDStud = (int)reader["id_stud"],
                    FIOStud = string.Format("{0}", reader["FIO_stud"])
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

            Debug.WriteLine(sql);
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
                await Task.Delay(200);
                RodsSelf.Add(new RodModel()
                {
                    IDRod = (int)reader["id_roditel"],
                    IDStud = (int)reader["id_stud"],
                    ListItemSelectStud = ListItemSelectStudSelf,
                    FIORod = (string)reader["FIO_roditel"],
                   // StepRod = (string)reader["step_rod"],
                    Adr = (string)reader["address_rod"],
                    Tel = (string)reader["tel_rod"],
                    Rabota = (string)reader["rabota_rod"]
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

            if (SearchName)
                SearchTypesAnd(ref sql, "`FIO_roditel` like @text");
          
            if (SearchAdr)
                SearchTypesAnd(ref sql, "`address_rod` like @text");
            if (SearchTel)
                SearchTypesAnd(ref sql, "`tel_rod` like @text");
            if (SearchRab)
                SearchTypesAnd(ref sql, "`rabota_rod` like @text");

            if (!GroupInsertNotValid(SearchSelectStud))
                SearchTypesAnd(ref sql, "`id_stud` = @kart_stud");

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

        private DeleteCommand<RodModel> DeleteSelf;
        public DeleteCommand<RodModel> DeleteMe
        {
            get => DeleteSelf;
            set
            {
                DeleteSelf = value;
                OnPropertyChanged("DeleteMe");
            }
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
                    //StepRod = value.StepRod,
                    Adr = value.Adr,
                    Tel = value.Tel,
                    Rabota = value.Rabota
                };

                DeleteMe = new DeleteCommand<RodModel>(DeleteData, EditDataSelf);
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

           /* if (string.IsNullOrEmpty(NewRodSelf.StepRod))
            {
                MessageBox.Show("Не указано степень родства");
                return;
            }*/

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
            //command.Parameters.Add(new MySqlParameter("@s", NewRodSelf.StepRod));
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

      
        private async void DeleteData(RodModel a)
        {
            if (MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            Connector
                 con = new Connector();
            string
                sql = "DELETE FROM `roditeli` WHERE `id_roditel` = @i;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            Debug.WriteLine(a.IDRod);

            command.Parameters.Add(new MySqlParameter("@i", a.IDRod));

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

           /* if (string.IsNullOrEmpty(EditRod.StepRod))
            {
                MessageBox.Show("Не указано степень родства");
                return;
            }*/

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
                sql = "UPDATE `roditeli` SET `id_stud`=@id, `FIO_roditel`=@f, `address_rod`=@f, `tel_rod`=@t, `rabota_rod`=@r WHERE `id_roditel`=@i;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());


            command.Parameters.Add(new MySqlParameter("@id",EditRod.IDStud));
            command.Parameters.Add(new MySqlParameter("@f", EditRod.FIORod));
            //command.Parameters.Add(new MySqlParameter("@s", EditRod.StepRod));
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