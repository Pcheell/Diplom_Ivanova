using Ivanova_UchitDn.Core;
using Ivanova_UchitDn.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Ivanova_UchitDn.Core.CoreApp;

namespace Ivanova_UchitDn.ViewModel
{
    public class GrupData : INotifyPropertyChanged
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



        private IList<ListItemSelect> ListItemSelectKurSelf;
        public IList<ListItemSelect> ListItemSelectKur
        {
            get => ListItemSelectKurSelf;
            set => ListItemSelectKurSelf = value;
        }

        private IList<GrupModel> GroupsSelf;
        public IList<GrupModel> Users
        {
            get => GroupsSelf;
            set => GroupsSelf = value;
        }

        private GrupModel NewGroupSelf;
        public GrupModel NewGroup
        {
            get => NewGroupSelf;
            set
            {
                NewGroupSelf = value;
                OnPropertyChanged("NewGroup");
            }
        }



        public GrupData()
        {
            ListItemSelectKur = new ObservableCollection<ListItemSelect>();
            LoadData();
        }

        private async void LoadData()
        {
            NewGroup = new GrupModel();
            EditGroup = new GrupModel();
            _ = await GroupDataList();
            OnPropertyChanged("SearchSelectKur");
            _ = await GroupDataSelect();
        }

        private async Task<bool> GroupDataList()
        {
            Connector
                 con = new Connector();
            string
                sql = "select * from `kurator` limit 50";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            await con.GetOpen();

            MySqlDataReader
                reader = await command.ExecuteReaderAsync();

            ListItemSelectKurSelf = new ObservableCollection<ListItemSelect>() { new ListItemSelect() { FIOKur = "Все" } };

            if (!reader.HasRows)
            {
                await con.GetClose();
                return false;
            }

            while (await reader.ReadAsync())
            {
                await Task.Delay(100);
                ListItemSelectKurSelf.Add(new ListItemSelect()
                {
                    IDKur = (int)reader["id_kurator"],
                    FIOKur = string.Format("{0}", reader["FIO_kurator"])
                });

                OnPropertyChanged("ListItemSelectKur");
            }

            NewGroupSelf.ListItemSelectKur = ListItemSelectKurSelf;
            await con.GetClose();
            return true;
        }

        private async Task<bool> GroupDataSelect()
        {
            Connector
                 con = new Connector();
            string
                sql = string.Format("select * from `grup` {0} limit {1}", SearchTypes(), 999);

            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            Debug.WriteLine(sql);
            command.Parameters.Add(new MySqlParameter("@text", string.Format("%{0}%", SearchText)));
            command.Parameters.Add(new MySqlParameter("@kurator", SearchSelectKur));

            await con.GetOpen();
            GroupsSelf = new ObservableCollection<GrupModel>();    

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
                GroupsSelf.Add(new GrupModel()
                {
                    IDGrup = (int)reader["id_grup"],
                    IDKur = (int)reader["id_kurator"],
                    ListItemSelectKur = ListItemSelectKurSelf,
                    NameGrup = (string)reader["name_grup"]
                    
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
                SearchTypesAnd(ref sql, "`name_grup` like @text");

            if (!GroupInsertNotValid(SearchSelectKur))
                SearchTypesAnd(ref sql, "`id_kurator` = @kurator");

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


        private DeleteCommandG DeleteSelf;
        public DeleteCommandG DeleteMe
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

        public GrupModel EditDataSelf { get; set; }
        public GrupModel EditGroup
        {
            get => EditDataSelf;
            set
            {
                if (value == null)
                {
                    EditDataSelf = new GrupModel();
                    return;
                }

                EditDataSelf = new GrupModel()
                {
                    IDGrup = value.IDGrup,
                    IDKur = value.IDKur,
                    NameGrup = value.NameGrup,
                    ListItemSelectKur = value.ListItemSelectKur
                };


                DeleteMe = new DeleteCommandG(DeleteData, EditDataSelf);
                OnPropertyChanged("EditGroup");
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
            if (GroupInsertNotValid(NewGroupSelf.IDKur))
            {
                MessageBox.Show("Не выбран куратор");
                return;
            }

            if (string.IsNullOrEmpty(NewGroupSelf.NameGrup))
            {
                MessageBox.Show("Не указано название группы");
                return;
            }


            Connector
                 con = new Connector();
            string
                sql = "INSERT INTO `grup` (id_kurator, name_grup) VALUES (@f, @name);";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@f", NewGroupSelf.IDKur));
            command.Parameters.Add(new MySqlParameter("@name", NewGroupSelf.NameGrup));

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

        private async void DeleteData(GrupModel a)
        {
            if (MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            Connector
                 con = new Connector();
            string
                sql = "DELETE FROM `grup` WHERE `id_grup` = @i;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            Debug.WriteLine(a.IDGrup);

            command.Parameters.Add(new MySqlParameter("@i", a.IDGrup));

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
            if (GroupInsertNotValid(EditGroup.IDKur))
            {
                MessageBox.Show("Не выбран куратор");
                return;
            }

            if (string.IsNullOrEmpty(EditGroup.NameGrup))
            {
                MessageBox.Show("Не указано название группы");
                return;
            }

            if (element is Grid g)
                g.Visibility = Visibility.Collapsed;

            Connector
                 con = new Connector();
            string
                sql = "UPDATE `grup` SET `id_kurator`=@k, `name_grup`=@n WHERE `id_grup`=@i;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());


            command.Parameters.Add(new MySqlParameter("@k", EditGroup.IDKur));
            command.Parameters.Add(new MySqlParameter("@n", EditGroup.NameGrup));
            command.Parameters.Add(new MySqlParameter("@i", EditGroup.IDGrup));

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

            foreach (ListItemSelect group in ListItemSelectKur)
            {
                if (group.IDKur != key)
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

        private int SearchSelectKurSelf;
        public int SearchSelectKur
        {
            get => SearchSelectKurSelf;
            set { SearchSelectKurSelf = value; LoadData(); OnPropertyChanged("SearchSelectKur"); }
        }
    }
}