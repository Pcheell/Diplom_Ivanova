using Google.Protobuf.WellKnownTypes;
using Ivanova_UchitDn.Core;
using Ivanova_UchitDn.Model;
using Ivanova_UchitDn.View_Page;
using MySqlConnector;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using static Ivanova_UchitDn.Core.CoreApp;
using static System.Net.Mime.MediaTypeNames;

namespace Ivanova_UchitDn.ViewModel
{
    public class StudData : INotifyPropertyChanged
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


        private IList<ListItemSelectG> ListItemSelectGrupSelf;
        public IList<ListItemSelectG> ListItemSelectGrup
        {
            get => ListItemSelectGrupSelf;
            set => ListItemSelectGrupSelf = value;
        }


        private IList<StudModel> StudsSelf;
        public IList<StudModel> Users
        {
            get => StudsSelf;
            set => StudsSelf = value;
        }


        private StudModel NewStudSelf;
        public StudModel NewStud
        {
            get => NewStudSelf;
            set
            {
                NewStudSelf = value;
                OnPropertyChanged("NewStud");
            }
        }



        public StudData()
        {
            SearchDataStartSelf = DateTime.Today;
            SearchDataEndSelf = DateTime.Today;
            ListItemSelectGrup = new ObservableCollection<ListItemSelectG>();
            LoadData();
        }

        private async void LoadData()
        {
            if (isUpdating)
            {
                return;
            }
            isUpdating = true;

            NewStud = new StudModel();
            EditStud = new StudModel();
            _ = await GroupDataList();
            OnPropertyChanged("SearchSelectGroup");
            _ = await StudDataSelect();

            isUpdating = false;

        }

        private async Task<bool> GroupDataList()
        {
            Connector
                 con = new Connector();
            string
                sql = "select * from `grup` limit 50";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            await con.GetOpen();

            MySqlDataReader
                reader = await command.ExecuteReaderAsync();

            ListItemSelectGrupSelf = new ObservableCollection<ListItemSelectG>() { new ListItemSelectG() { NameGrup = "Все" } };

            if (!reader.HasRows)
            {
                await con.GetClose();
                return false;
            }

            while (await reader.ReadAsync())
            {
                ListItemSelectGrupSelf.Add(new ListItemSelectG()
                {
                    IDGrup = (int)reader["id_grup"],
                    NameGrup = string.Format("{0}", reader["name_grup"])
                });

                OnPropertyChanged("ListItemSelectGrup");
            }

            NewStudSelf.ListItemSelectGrup = ListItemSelectGrupSelf;
            await con.GetClose();
            return true;
        }

        private async Task<bool> StudDataSelect()
        {

            Connector
                 con = new Connector();
            string
                sql = string.Format("select * from `kart_stud` {0} limit {1}", SearchTypes(), 999);

            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            Debug.WriteLine(sql);
            command.Parameters.Add(new MySqlParameter("@text", string.Format("%{0}%", SearchText)));
            command.Parameters.Add(new MySqlParameter("@grup", SearchSelectGroup));
            command.Parameters.Add(new MySqlParameter("@date_start", SearchDataStart));
            command.Parameters.Add(new MySqlParameter("@date_end", SearchDataEnd));

            await con.GetOpen();
            StudsSelf = new ObservableCollection<StudModel>();

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
                StudsSelf.Add(new StudModel()
                {
                    IDStud = (int)reader["id_stud"],
                    IDGrup = (int)reader["id_grup"],
                    ListItemSelectGrup = ListItemSelectGrupSelf,
                    FIOStud = (string)reader["FIO_stud"],
                    DRStud = Convert.ToDateTime(reader["dr_stud"]),
                    Adr = (string)reader["address_stud"],
                    Tel = (string)reader["tel_stud"],
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

            if (!GroupInsertNotValid(SearchSelectGroup))
                SearchTypesAnd(ref sql, "`id_grup` = @grup");

            if (SearchDate)
                SearchTypesAnd(ref sql, "CAST(`dr_stud` AS DATE) BETWEEN str_to_date(@date_start, '%Y-%m-%d') AND str_to_date(@date_end, '%Y-%m-%d')");
          

            // Если ни один чекбокс не выбран, добавляем условия поиска для всех полей
            if (!SearchName && !SearchAdr && !SearchTel)
            {
                sql = "`FIO_stud` LIKE @text OR `address_stud` " +
                    "LIKE @text OR `tel_stud` LIKE @text";
            }
            else
            {
                List<string> conditions = new List<string>();

                // Добавляем условия поиска в зависимости от выбранных чекбоксов
                if (SearchName)
                    conditions.Add("`FIO_stud` LIKE @text");
                if (SearchAdr)
                    conditions.Add("`address_stud` LIKE @text");
                if (SearchTel)
                    conditions.Add("`tel_stud` LIKE @text");
              

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

        public StudModel EditDataSelf { get; set; }
        public StudModel EditStud
        {
            get => EditDataSelf;
            set
            {
                if (value == null)
                {
                    EditDataSelf = new StudModel();
                    return;
                }



                EditDataSelf = new StudModel()
                {
                    IDStud = value.IDStud,
                    IDGrup = value.IDGrup,
                    ListItemSelectGrup = ListItemSelectGrupSelf,
                    FIOStud = value.FIOStud,
                    DRStud = value.DRStud,
                    Adr = value.Adr,
                    Tel = value.Tel
                };

                OnPropertyChanged("EditStud");
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

            if (GroupInsertNotValid(NewStudSelf.IDGrup))
            {
                MessageBox.Show("Не выбрана группа");
                return;
            }

            if (string.IsNullOrEmpty(NewStudSelf.FIOStud))
            {
                MessageBox.Show("Не указано ФИО");
                return;
            }

            if (NewStudSelf.DRStud == null)
            {
                MessageBox.Show("Не указана дата рождения");
                return;
            }

            if (string.IsNullOrEmpty(NewStudSelf.Adr))
            {
                MessageBox.Show("Не указан адрес");
                return;
            }

            if (string.IsNullOrEmpty(NewStudSelf.Tel))
            {
                MessageBox.Show("Не указан телефон");
                return;
            }


            Connector
                 con = new Connector();
            string
                sql = "INSERT INTO `kart_stud` (id_grup, FIO_stud, dr_stud, address_stud, tel_stud) VALUES (@id, @f, @d, @a, @t);";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@id", NewStudSelf.IDGrup));
            command.Parameters.Add(new MySqlParameter("@f", NewStudSelf.FIOStud));
            command.Parameters.Add(new MySqlParameter("@d", NewStudSelf.DRStud));
            command.Parameters.Add(new MySqlParameter("@a", NewStudSelf.Adr));
            command.Parameters.Add(new MySqlParameter("@t", NewStudSelf.Tel));

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
                sql = "DELETE FROM `kart_stud` WHERE `id_stud` = @i;";
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
            if (GroupInsertNotValid(EditStud.IDGrup))
            {
                MessageBox.Show("Не выбрана группа");
                return;
            }

            if (string.IsNullOrEmpty(EditStud.FIOStud))
            {
                MessageBox.Show("Не указано ФИО");
                return;
            }

            if (NewStudSelf.DRStud == null)
            {
                MessageBox.Show("Не указана дата рождения");
                return;
            }

            if (string.IsNullOrEmpty(EditStud.Adr))
            {
                MessageBox.Show("Не указан адрес");
                return;
            }

            if (string.IsNullOrEmpty(EditStud.Tel))
            {
                MessageBox.Show("Не указан телефон");
                return;
            }

            if (element is Grid g)
                g.Visibility = Visibility.Collapsed;

            Connector
                 con = new Connector();
            string
                sql = "UPDATE `kart_stud` SET `id_grup`=@id, `FIO_stud`=@f, `dr_stud`=@d, `address_stud`=@a, `tel_stud`=@t WHERE `id_stud`=@i;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@id",EditStud.IDGrup));
            command.Parameters.Add(new MySqlParameter("@f", EditStud.FIOStud));
            command.Parameters.Add(new MySqlParameter("@d", EditStud.DRStud));
            command.Parameters.Add(new MySqlParameter("@a", EditStud.Adr));
            command.Parameters.Add(new MySqlParameter("@t", EditStud.Tel));
            command.Parameters.Add(new MySqlParameter("@i", EditStud.IDStud));


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

            foreach (ListItemSelectG group in ListItemSelectGrup)
            {
                if (group.IDGrup != key)
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

        private int SearchSelectGroupSelf;
        public int SearchSelectGroup
        {
            get => SearchSelectGroupSelf;
            set { SearchSelectGroupSelf = value; LoadData(); OnPropertyChanged("SearchSelectGroup"); }
        }

        private bool SearchDateSelf;
        public bool SearchDate
        {
            get => SearchDateSelf;
            set { SearchDateSelf = value; OnPropertyChanged("SearchDate"); }
        }

        private DateTime SearchDataStartSelf;
        public DateTime SearchDataStart
        {
            get => SearchDataStartSelf;
            set
            {
                if (value > SearchDataEndSelf)
                {
                    SearchDataEndSelf = value;
                    OnPropertyChanged("SearchDataEnd");
                }

                SearchDataStartSelf = value;
                LoadData();
                OnPropertyChanged("SearchDataStart");
            }
        }

        private DateTime SearchDataEndSelf;
        public DateTime SearchDataEnd
        {
            get => SearchDataEndSelf;
            set
            {
                if (value < SearchDataStartSelf)
                {
                    SearchDataStartSelf = value;
                    OnPropertyChanged("SearchDataStart");
                }

                SearchDataEndSelf = value;
                LoadData();
                OnPropertyChanged("SearchDataEnd");
            }
        }
    }
}