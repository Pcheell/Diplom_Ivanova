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
    public class UspData : INotifyPropertyChanged
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



        private IList<ListItemSelectP> ListItemSelectPredSelf;
        public IList<ListItemSelectP> ListItemSelectPred
        {
            get => ListItemSelectPredSelf;
            set => ListItemSelectPredSelf = value;
        }

        private IList<ListItemSelectS> ListItemSelectStudSelf;
        public IList<ListItemSelectS> ListItemSelectStud
        {
            get => ListItemSelectStudSelf;
            set => ListItemSelectStudSelf = value;
        }

        private IList<UspModel> UspsSelf;
        public IList<UspModel> Users
        {
            get => UspsSelf;
            set => UspsSelf = value;
        }


        private UspModel NewUspSelf;
        public UspModel NewUsp
        {
            get => NewUspSelf;
            set
            {
                NewUspSelf = value;
                OnPropertyChanged("NewUsp");
            }
        }

       



        public UspData()
        {
            ListItemSelectPred = new ObservableCollection<ListItemSelectP>();
            ListItemSelectStud = new ObservableCollection<ListItemSelectS>();
            LoadData();
        }

        private async void LoadData()
        {
            NewUsp = new UspModel();
            EditUsp = new UspModel();
            _ = await PredmetDataList();
            OnPropertyChanged("SearchSelectPred");
            _ = await StudentDataList();
            OnPropertyChanged("SearchSelectStud");
            _ = await UspDataSelect();
        }

        private async Task<bool> PredmetDataList()
        {
            Connector
                 con = new Connector();
            string
                sql = "select * from `predmet` limit 50";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            await con.GetOpen();

            MySqlDataReader
                reader = await command.ExecuteReaderAsync();

            ListItemSelectPredSelf = new ObservableCollection<ListItemSelectP>() { new ListItemSelectP() { NamePred = "Все" } };

            if (!reader.HasRows)
            {
                await con.GetClose();
                return false;
            }

            while (await reader.ReadAsync())
            {
                await Task.Delay(100);
                ListItemSelectPredSelf.Add(new ListItemSelectP()
                {
                    IDPred = (int)reader["id_pred"],
                    NamePred = string.Format("{0}", reader["name_pred"])
                });

                OnPropertyChanged("ListItemSelectPred");
            }

            NewUspSelf.ListItemSelectPred = ListItemSelectPredSelf;
            await con.GetClose();
            return true;
        }


        private async Task<bool> StudentDataList()
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

            NewUspSelf.ListItemSelectStud = ListItemSelectStudSelf;
            await con.GetClose();
            return true;
        }

        private async Task<bool> UspDataSelect()
        {
            Connector
                 con = new Connector();
            string
                sql = string.Format("select * from `uspevaemost` {0} limit {1}", SearchTypes(), 999);

            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            Debug.WriteLine(sql);
            command.Parameters.Add(new MySqlParameter("@text", string.Format("%{0}%", SearchText)));
            command.Parameters.Add(new MySqlParameter("@predmet", SearchSelectPred));
            command.Parameters.Add(new MySqlParameter("@kart_stud", SearchSelectStud));

            await con.GetOpen();
            UspsSelf = new ObservableCollection<UspModel>();

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
                UspsSelf.Add(new UspModel()
                {
                    IDStud = (int)reader["id_stud"],
                    ListItemSelectStud = ListItemSelectStudSelf,
                    IDPred = (int)reader["id_pred"],
                    ListItemSelectPred = ListItemSelectPredSelf,
                    Ocenka = (string)reader["ocenka"],
                    //Semestr = (string)reader["semestr"]

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

          if (SearchOcenka)
            SearchTypesAnd(ref sql, "`ocenka` like @text");
          

            if (!GroupInsertNotValidPred(SearchSelectPred))
                SearchTypesAnd(ref sql, "`id_pred` = @predmet");

            if (!GroupInsertNotValidStud(SearchSelectStud))
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


        private DeleteCommandU DeleteSelf;
        public DeleteCommandU DeleteMe
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

        public UspModel EditDataSelf { get; set; }
        public UspModel EditUsp
        {
            get => EditDataSelf;
            set
            {
                if (value == null)
                {
                    EditDataSelf = new UspModel();
                    return;
                }

                EditDataSelf = new UspModel()
                {

                    IDStud = value.IDStud,
                    ListItemSelectStud = value.ListItemSelectStud,
                    IDPred = value.IDPred,
                    ListItemSelectPred = value.ListItemSelectPred,
                    Ocenka = value.Ocenka,
                    //Semestr = value.Semestr,
                };


                DeleteMe = new DeleteCommandU(DeleteData, EditDataSelf);
                OnPropertyChanged("EditUsp");
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
            if (GroupInsertNotValidStud(NewUspSelf.IDStud))
            {
                MessageBox.Show("Не выбран студент");
                return;
            }

            if (GroupInsertNotValidPred(NewUspSelf.IDPred))
            {
                MessageBox.Show("Не указан предмет");
                return;
            }


            Connector
                 con = new Connector();
            string
                sql = "INSERT INTO `uspevaemost` (id_stud, id_pred, ocenka) VALUES (@is, @p, @o);";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@is", NewUspSelf.IDStud));
            command.Parameters.Add(new MySqlParameter("@p", NewUspSelf.IDPred));
            command.Parameters.Add(new MySqlParameter("@o", NewUspSelf.Ocenka));
          // command.Parameters.Add(new MySqlParameter("@s", NewUspSelf.Semestr));

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

        private async void DeleteData(UspModel a)
        {
            if (MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            Connector
                 con = new Connector();
            string
                sql = "DELETE FROM `uspevaemost` WHERE `id_stud` = @i;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            Debug.WriteLine(a.IDStud);

            command.Parameters.Add(new MySqlParameter("@i", a.IDStud));

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
            if (GroupInsertNotValidStud(EditUsp.IDStud))
            {
                MessageBox.Show("Не выбран студент");
                return;
            }

            if (GroupInsertNotValidPred(EditUsp.IDPred))
            {
                MessageBox.Show("Не указан предмет");
                return;
            }

            if (element is Grid g)
                g.Visibility = Visibility.Collapsed;

            Connector
                 con = new Connector();
            string
                sql = "UPDATE `uspevaemost` SET `ocenka`=@o,  WHERE `id_stud`=@is AND `id_pred`=@ip;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());



            command.Parameters.Add(new MySqlParameter("@o", EditUsp.IDStud));
            command.Parameters.Add(new MySqlParameter("@s", EditUsp.IDPred));
            command.Parameters.Add(new MySqlParameter("@is", EditUsp.Ocenka));
           // command.Parameters.Add(new MySqlParameter("@ip", EditUsp.Semestr));


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


        private bool GroupInsertNotValidStud(int key)
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

        private bool GroupInsertNotValidPred(int key)
        {
            if (key < 1)
                return true;
            foreach (ListItemSelectP group in ListItemSelectPred)
            {
                if (group.IDPred != key)
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

        public bool SearchOcenkaSelf;
        public bool SearchOcenka
        {
            get => SearchOcenkaSelf;
            set
            {
                SearchOcenkaSelf = value;
                OnPropertyChanged("SearchOcenka");
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

        private int SearchSelectPredSelf;
        public int SearchSelectPred
        {
            get => SearchSelectPredSelf;
            set { SearchSelectPredSelf = value; LoadData(); OnPropertyChanged("SearchSelectPred"); }
        }

        private int SearchSelectStudSelf;
        public int SearchSelectStud
        {
            get => SearchSelectStudSelf;
            set { SearchSelectStudSelf = value; LoadData(); OnPropertyChanged("SearchSelectStud"); }
        }


    }
}
