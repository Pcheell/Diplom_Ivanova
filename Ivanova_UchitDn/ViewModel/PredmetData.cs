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

namespace Ivanova_UchitDn.ViewModel
{
    public class PredmetData : INotifyPropertyChanged
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


        private IList<PredmetModel> PredmetsSelf;
        public IList<PredmetModel> Users
        {
            get => PredmetsSelf;
            set => PredmetsSelf = value;
        }

        private PredmetModel NewPredmetSelf;
        public PredmetModel NewPredmet
        {
            get => NewPredmetSelf;
            set
            {
                NewPredmetSelf = value;
                OnPropertyChanged("NewPredmet");
            }
        }


        public PredmetData()
        {
            LoadData();
        }

        private async void LoadData()
        {
            NewPredmet = new PredmetModel();
            EditPred = new PredmetModel();
            _ = await GroupDataList();
        }

        private async Task<bool> GroupDataList()
        {
            Connector
                 con = new Connector();
            string
                sql = string.Format("select * from `predmet` {0} limit {1}", SearchTypes(), 999);

            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            Debug.WriteLine(sql);
            command.Parameters.Add(new MySqlParameter("@text", string.Format("%{0}%", SearchText)));

            await con.GetOpen();
            PredmetsSelf = new ObservableCollection<PredmetModel>();

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
                PredmetsSelf.Add(new PredmetModel()
                {
                    IDPred = (int)reader["id_pred"],
                    NamePred = (string)reader["name_pred"],
                    KolChas = (int)reader["kol_chas"]

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
                SearchTypesAnd(ref sql, "`name_pred` like @text");
            if (SearchKol)
                SearchTypesAnd(ref sql, "`kol_chas` like @text");

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

        private DeleteCommandP DeleteSelf;
        public DeleteCommandP DeleteMe
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

        public PredmetModel EditDataSelf { get; set; }
        public PredmetModel EditPred
        {
            get => EditDataSelf;
            set
            {
                if (value == null)
                {
                    EditDataSelf = new PredmetModel();
                    return;
                }

                EditDataSelf = new PredmetModel()
                {
                    IDPred = value.IDPred,
                    NamePred = value.NamePred,
                    KolChas = value.KolChas
                };


                DeleteMe = new DeleteCommandP(DeleteData, EditDataSelf);
                OnPropertyChanged("EditPred");
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

            if (string.IsNullOrEmpty(NewPredmetSelf.NamePred))
            {
                MessageBox.Show("Не указано название предмета");
                return;
            }

            if (string.IsNullOrEmpty(NewPredmetSelf.KolChas.ToString()))
            {
                MessageBox.Show("Не указано кол-о часов");
                return;
            }


            Connector
                 con = new Connector();
            string
                sql = "INSERT INTO `predmet` (name_pred, kol_chas) VALUES (@n, @k);";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@n", NewPredmetSelf.NamePred));
            command.Parameters.Add(new MySqlParameter("@k", NewPredmetSelf.KolChas));

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

        private async void DeleteData(PredmetModel a)
        {
            if (MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            Connector
                 con = new Connector();
            string
                sql = "DELETE FROM `predmet` WHERE `id_pred` = @i;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            Debug.WriteLine(a.IDPred);

            command.Parameters.Add(new MySqlParameter("@i", a.IDPred));

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

            if (string.IsNullOrEmpty(EditPred.NamePred))
            {
                MessageBox.Show("Не указано название предмета");
                return;
            }

            if (string.IsNullOrEmpty(EditPred.KolChas.ToString()))
            {
                MessageBox.Show("Не указано кол-о часов");
                return;
            }

            if (element is Grid g)
                g.Visibility = Visibility.Collapsed;

            Connector
                 con = new Connector();
            string
                sql = "UPDATE `predmet` SET `name_pred`=@n, `kol_chas`=@k WHERE `id_pred`=@i;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@n", EditPred.NamePred));
            command.Parameters.Add(new MySqlParameter("@k", EditPred.KolChas));
            command.Parameters.Add(new MySqlParameter("@i", EditPred.IDPred));

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
            }
        }

        public bool SearchKolSelf;
        public bool SearchKol
        {
            get => SearchKolSelf;
            set
            {
                SearchKolSelf = value;
                OnPropertyChanged("SearchKol");
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

    }
}