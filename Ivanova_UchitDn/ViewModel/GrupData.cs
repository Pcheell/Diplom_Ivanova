using Ivanova_UchitDn.Core;
using Ivanova_UchitDn.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Ivanova_UchitDn.Core.CoreApp;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel.Application;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using MigraDoc.DocumentObjectModel.Tables;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Ivanova_UchitDn.ViewModel
{
    public class GrupData : INotifyPropertyChanged
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

        public ICommand PromoteClassesCommand { get; }


        private bool isAscending = true;
        public ICommand SortCommand { get; private set; }
        public GrupData()
        {
            ListItemSelectKur = new ObservableCollection<ListItemSelect>();
            LoadData();

            PromoteClassesCommand = new RelayCommand(PromoteClasses);
            SortCommand = new RelayCommand(SortUsers);


        }
        private void SortUsers()
        {
            if (isAscending)
            {
                Users = new ObservableCollection<GrupModel>(Users.OrderBy(u => ParseClass(u.NameGrup)));
            }
            else
            {
                Users = new ObservableCollection<GrupModel>(Users.OrderByDescending(u => ParseClass(u.NameGrup)));
            }
            isAscending = !isAscending;
            OnPropertyChanged(nameof(Users));
        }


        private (int, string) ParseClass(string className)
        {
            var match = Regex.Match(className, @"(\d+)(\D+)");
            if (match.Success)
            {
                int number = int.Parse(match.Groups[1].Value);
                string letter = match.Groups[2].Value;
                return (number, letter);
            }
            return (0, className); 
        }


        public async void LoadData()
        {
            if (isUpdating)
            {
                return;
            }
            isUpdating = true;


            NewGroup = new GrupModel();
            EditGroup = new GrupModel();
            _ = await GroupDataList();
            OnPropertyChanged("SearchSelectKur");
            _ = await GroupDataSelect();

            GCount = Users.Count;

            isUpdating = false;

        }

        private int _gCount;
        public int GCount
        {
            get { return _gCount; }
            set
            {
                _gCount = value;
                OnPropertyChanged(nameof(GCount));
            }
        }

        async Task<bool> GroupDataList()
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
                await Task.Delay(1);
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
                await Task.Delay(1);
                GroupsSelf.Add(new GrupModel()
                {
                    IDGrup = (int)reader["id_grup"],
                    IDKur = (int)reader["id_kurator"],
                    ListItemSelectKur = ListItemSelectKurSelf,
                    NameGrup = (string)reader["name_grup"],
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

            if (SearchName)
                SearchTypesAnd(ref sql, "`name_grup` like @text");

            if (!GroupInsertNotValid(SearchSelectKur))
                SearchTypesAnd(ref sql, "`id_kurator` = @kurator");

            if (SearchKurator)
            {
                SearchTypesAnd(ref sql, "`id_kurator` IN (SELECT `id_kurator` FROM `kurator` WHERE `FIO_kurator` LIKE @text)");
            }

            // Если ни один чекбокс не выбран, искать по имени группы и ФИО куратора
            if (!SearchName && !SearchKurator && !string.IsNullOrEmpty(SearchText))
            {
                SearchTypesAnd(ref sql, "`name_grup` like @text OR `id_kurator` IN (SELECT `id_kurator` FROM `kurator` WHERE `FIO_kurator` LIKE @text)");
            }

            // Если оба чекбокса выбраны, искать по имени группы и ФИО куратора
            if (SearchName && SearchKurator && !string.IsNullOrEmpty(SearchText))
            {
                SearchTypesAnd(ref sql, "`name_grup` like @text AND `id_kurator` IN (SELECT `id_kurator` FROM `kurator` WHERE `FIO_kurator` LIKE @text)");
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

        private async void DeleteData(int a)
        {
            if (MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            Connector
                 con = new Connector();
            string
                sql = "DELETE FROM `grup` WHERE `id_grup` = @i;";
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
                LoadData();

            }
        }

        public bool SearchKuratorSelf;
        public bool SearchKurator
        {
            get => SearchKuratorSelf;
            set
            {
                SearchKuratorSelf = value;
                OnPropertyChanged("SearchKurator");
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


        private int SearchSelectKurSelf;
        public int SearchSelectKur
        {
            get => SearchSelectKurSelf;
            set { SearchSelectKurSelf = value; LoadData(); OnPropertyChanged("SearchSelectKur"); }
        }




        private async void PromoteClasses()
        {
            bool confirmed = MessageBox.Show("Перевод классов осуществляется каждый год. Вы точно хотите перевести классы?", 
                "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;

            if (!confirmed)
                return;

            Connector con = new Connector();
            MySqlCommand command;
            await con.GetOpen();

            try
            {
                // Удаляем последний класс для всех буквенных суффиксов
                string deleteSql = "DELETE FROM `grup` WHERE `name_grup` LIKE '11%'";
                command = new MySqlCommand(deleteSql, con.GetCon());
                await command.ExecuteNonQueryAsync();

                string updateSql = "UPDATE `grup` SET `name_grup` = REPLACE(`name_grup`, '10', '11') WHERE `name_grup` LIKE '10%';";
                command = new MySqlCommand(updateSql, con.GetCon());
                await command.ExecuteNonQueryAsync();

               

                // Переименовываем остальные классы
                for (int i = 10; i >= 1; i--)
                {
                    string oldNamePattern = $"{i}%";
                    string newNumber = $"{i + 1}";

                    if (i < 10 && i > 1) // Для классов от 2 до 9 меняем номер на номер+1
                    {
                        updateSql = "UPDATE `grup` SET `name_grup` = CONCAT(@newNumber, SUBSTRING(`name_grup`, 2)) WHERE `name_grup` LIKE @oldNamePattern";
                        command = new MySqlCommand(updateSql, con.GetCon());
                        command.Parameters.AddWithValue("@newNumber", newNumber);
                        command.Parameters.AddWithValue("@oldNamePattern", oldNamePattern);
                        await command.ExecuteNonQueryAsync();
                    }
                    
                }

                string updateSqlFor1To2 = "UPDATE `grup` SET `name_grup` = REPLACE(`name_grup`, '1', '2') " +
                    "WHERE `name_grup` LIKE '1%' AND LENGTH(`name_grup`) > 1 AND SUBSTRING(`name_grup`, 2, 1) BETWEEN 'а' AND 'я';";
                command = new MySqlCommand(updateSqlFor1To2, con.GetCon());
                await command.ExecuteNonQueryAsync();


                MessageBox.Show("Классы успешно переведены", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при переводе классов: {ex.Message}", "Ошибка");
            }
            finally
            {
                await con.GetClose();
                LoadData(); // Обновить данные после перевода
            }
        }




        private IExcelExport ExportPdfSelf;
        public IExcelExport ExportPdf => ExportPdfSelf ?? (ExportPdfSelf = new IExcelExport(ExportDataPdf));

        private IExcelExport ExportExcelSelf;
        public IExcelExport ExportExcel => ExportExcelSelf ?? (ExportExcelSelf = new IExcelExport(ExportDataExcel));

        private string GetKuratorFIO(int kuratorID)
        {
            foreach (var kurator in ListItemSelectKur)
            {
                if (kurator.IDKur == kuratorID)
                {
                    return kurator.FIOKur;
                }
            }
            return ""; // Вернуть пустую строку, если куратор не найден
        }


        private void ExportDataExcel()
        {
            if (Users == null || !Users.Any())
            {
                MessageBox.Show("Нет данных для экспорта", "Ошибка");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Создать Excel документ для таблицы \"Классы\"?", "Подтверждение", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var excelApp = new Excel();
            var workbook = excelApp.Workbooks.Add();
            Worksheet worksheet = (Worksheet)excelApp.ActiveSheet;


            // Переименовываем лист
            worksheet.Name = "Таблица \"Классы\"";

            // Заголовок таблицы
            worksheet.Cells[1, 1] = "Таблица \"Классы\"";
            Range titleRange = worksheet.Range["A1", "B1"];
            titleRange.Merge();
            titleRange.Font.Bold = true;
            titleRange.Font.Size = 12;
            titleRange.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // Заголовки колонок
            worksheet.Cells[3, 1] = "Название класса";
            worksheet.Cells[3, 2] = "Классный руководитель";


            // Делаем заголовки колонок жирными
            for (int i = 1; i <= 2; i++)
            {
                Range headerCell = worksheet.Cells[3, i];
                headerCell.Font.Bold = true;
            }


            // Данные
            int row = 4;
            foreach (var group in Users)
            {
                worksheet.Cells[row, 1] = group.NameGrup;
                worksheet.Cells[row, 2] = GetKuratorFIO(group.IDKur); // Получить ФИО куратора по его ID
                row++;
            }

            // Автоматическое выравнивание столбцов
            worksheet.Columns.AutoFit();

            // Выровнять все ячейки по левому краю
            worksheet.Cells.HorizontalAlignment = XlHAlign.xlHAlignLeft;

            // Добавление границ для таблицы
            Range dataRange = worksheet.Range["A3", $"B{row - 1}"];
            dataRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            dataRange.Borders.Weight = XlBorderWeight.xlThin;

            // Показать Excel
            excelApp.Visible = true;
        }

        private void ExportDataPdf()
        {
            if (Users == null || !Users.Any())
            {
                MessageBox.Show("Нет данных для экспорта", "Ошибка");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Создать PDF документ для таблицы \"Группы\"?", "Подтверждение", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // Создаем новый документ
            Document document = new Document();
            Section section = document.AddSection();
            section.PageSetup.LeftMargin = 150; //отступ слева для таблицы


            // Заголовок таблицы
            Paragraph title = section.AddParagraph("Таблица \"Группы\"");
            title.Format.Font.Bold = true;
            title.Format.Font.Size = 14;
            title.Format.SpaceAfter = "1cm";
            title.Format.LeftIndent = "-2cm";

            title.Format.Alignment = ParagraphAlignment.Center;

            // Таблица
            Table table = section.AddTable();
            table.Borders.Width = 0.75;
            table.Format.Alignment = ParagraphAlignment.Center;

            // Определение столбцов
            Column column1 = table.AddColumn("2cm");
            Column column2 = table.AddColumn("8cm");

            // Заголовок таблицы
            Row headerRow = table.AddRow();
            headerRow.Cells[0].AddParagraph("Название класса");
            headerRow.Cells[1].AddParagraph("ФИО куратора");

            headerRow.Format.Font.Bold = true;
            headerRow.Format.Alignment = ParagraphAlignment.Center;

            // Заполнение таблицы данными
            foreach (var group in Users)
            {
                Row row = table.AddRow();
                row.Cells[0].AddParagraph(group.NameGrup);
                row.Cells[1].AddParagraph(GetKuratorFIO(group.IDKur));
                row.Format.Alignment = ParagraphAlignment.Center;
            }

            // Рендеринг документа
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
            renderer.Document = document;
            renderer.RenderDocument();

            var filePath = "Таблица_Группы.pdf";
            renderer.PdfDocument.Save(filePath);

            // Открытие созданного PDF файла
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }






    }
}