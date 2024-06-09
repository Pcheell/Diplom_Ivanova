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
using static Ivanova_UchitDn.Core.CoreApp;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel.Application;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using MigraDoc.DocumentObjectModel.Tables;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Ivanova_UchitDn.ViewModel
{
    public class RodData : INotifyPropertyChanged
    {
        private readonly int userId;
        private bool isUpdating = false;

        public int IDStud { get; set; }
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




        private ObservableCollection<RodModel> rodList;
        public ObservableCollection<RodModel> RodList
        {
            get { return rodList; }
            set
            {
                rodList = value;
                OnPropertyChanged("RodList");
            }
        }


        private string fioStud;
        public string FIOStud
        {
            get { return fioStud; }
            set
            {
                fioStud = value;
                OnPropertyChanged("FIOStud");
            }
        }

        private bool isAscending = true;
        public ICommand SortCommand { get; private set; }

        public RodData(int userId)
        {
            ListItemSelectStud = new ObservableCollection<ListItemSelectS>();
            rodList = new ObservableCollection<RodModel>();
            LoadData();
            this.userId = userId;

            SortCommand = new RelayCommand(SortUsers);

        }
        private void SortUsers()
        {
            if (isAscending)
            {
                Users = new ObservableCollection<RodModel>(Users.OrderBy(u => u.FIORod));
            }
            else
            {
                Users = new ObservableCollection<RodModel>(Users.OrderByDescending(u => u.FIORod));
            }
            isAscending = !isAscending;
            OnPropertyChanged(nameof(Users));
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
            OnPropertyChanged("Users");

            RCount = Users.Count;

            isUpdating = false;

        }

        private int _rCount;
        public int RCount
        {
            get { return _rCount; }
            set
            {
                _rCount = value;
                OnPropertyChanged(nameof(RCount));
            }
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
            try
            {
                Connector con = new Connector();
                string existingSql = "SELECT * FROM `roditeli`";

                if (userId != 1)
                {
                    existingSql += " WHERE `id_stud` IN (SELECT `id_stud` FROM `kart_stud` WHERE `id_grup` IN (SELECT `id_grup` FROM `grup` WHERE `id_kurator` = @kuratorId))";
                }

                string sql = SearchTypes(existingSql);

                if (string.IsNullOrEmpty(sql))
                {
                    throw new InvalidOperationException("CommandText must be specified");
                }

                MySqlCommand command = new MySqlCommand(sql, con.GetCon());

                // Параметры для поиска
                command.Parameters.Add(new MySqlParameter("@text", string.Format("%{0}%", SearchText)));
                command.Parameters.Add(new MySqlParameter("@kart_stud", SearchSelectStud));
                command.Parameters.Add(new MySqlParameter("@kuratorId", userId));  // Добавляем параметр kuratorId

                await con.GetOpen();
                RodsSelf = new ObservableCollection<RodModel>();

                MySqlDataReader reader = await command.ExecuteReaderAsync();

                if (!reader.HasRows)
                {
                    await con.GetClose();
                    OnPropertyChanged("Users");
                    return false;
                }

                while (await reader.ReadAsync())
                {
                    await Task.Delay(3);

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
            catch (Exception ex)
            {
                Console.WriteLine("Вызвано исключение: " + ex.Message);
                return false;
            }
        }



        private string SearchTypes(string existingSql)
        {
            try
            {
                string sql = "";

                if (!GroupInsertNotValid(SearchSelectStud))
                    SearchTypesAnd(ref sql, "`id_stud` = @kart_stud");

                if (SearchStud)
                {
                    SearchTypesAnd(ref sql, "`id_stud` IN (SELECT `id_stud` FROM `kart_stud` WHERE `FIO_stud` LIKE @text)");
                } 

                if (!SearchName && !SearchAdr && !SearchTel && !string.IsNullOrEmpty(SearchText))
                {
                    sql = "`FIO_roditel` LIKE @text OR `address_rod` LIKE @text OR `tel_rod` LIKE @text " +
                          "OR `rabota_rod` LIKE @text OR `id_stud` IN (SELECT `id_stud` FROM `kart_stud` WHERE `FIO_stud` LIKE @text)";
                }
                else
                {
                    List<string> conditions = new List<string>();

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

                    sql = string.Join(" OR ", conditions);
                }

                if (!string.IsNullOrEmpty(sql))
                {
                    existingSql += existingSql.Contains("WHERE") ? " AND (" + sql + ")" : " WHERE " + sql;
                }

                return existingSql;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Вызвано исключение в SearchTypes: " + ex.Message);
                return existingSql;
            }
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
                MessageBox.Show("Не выбран куратор", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(NewRodSelf.FIORod))
            {
                MessageBox.Show("Не указано ФИО", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(NewRodSelf.Adr))
            {
                MessageBox.Show("Не указан адрес", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(NewRodSelf.Tel))
            {
                MessageBox.Show("Не указан телефон", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(NewRodSelf.Rabota))
            {
                MessageBox.Show("Не указана работа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Таблица не добавлена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }
            await con.GetClose();
            LoadData();
            MessageBox.Show("Таблица добавлена", "Подтверждение");
        }

      
        private async void DeleteData(int a)
        {
            if (MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButton.YesNo,  MessageBoxImage.Question) != MessageBoxResult.Yes)
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
                MessageBox.Show("Запись не удалена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Не выбран куратор", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(EditRod.FIORod))
            {
                MessageBox.Show("Не указано ФИО", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(EditRod.Adr))
            {
                MessageBox.Show("Не указан адрес", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(EditRod.Tel))
            {
                MessageBox.Show("Не указан телефон", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(EditRod.Rabota))
            {
                MessageBox.Show("Не указана работа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Запись не изменена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private IExcelExport ExportPdfSelf;
        public IExcelExport ExportPdf => ExportPdfSelf ?? (ExportPdfSelf = new IExcelExport(ExportDataPdf));

        private IExcelExport ExportExcelSelf;
        public IExcelExport ExportExcel => ExportExcelSelf ?? (ExportExcelSelf = new IExcelExport(ExportDataExcel));

        private string GetStudName(int studID)
        {
            foreach (var stud in ListItemSelectStud)
            {
                if (stud.IDStud == studID)
                {
                    return stud.FIOStud;
                }
            }
            return "";
        }


        private void ExportDataExcel()
        {
            if (Users == null || !Users.Any())
            {
                MessageBox.Show("Нет данных для экспорта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Создать Excel документ для таблицы \"Родители\"?", "Подтверждение", MessageBoxButton.YesNo,  MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var excelApp = new Excel();
            var workbook = excelApp.Workbooks.Add();
            Worksheet worksheet = (Worksheet)excelApp.ActiveSheet;


            // Переименовываем лист
            worksheet.Name = "Таблица \"Родители\"";

            // Заголовок таблицы
            worksheet.Cells[1, 1] = "Таблица \"Родители\"";
            Range titleRange = worksheet.Range["A1", "B1"];
            titleRange.Merge();
            titleRange.Font.Bold = true;
            titleRange.Font.Size = 12;
            titleRange.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // Заголовки колонок
            worksheet.Cells[3, 1] = "ФИО родителя";
            worksheet.Cells[3, 2] = "ФИО студента";
            worksheet.Cells[3, 3] = "Адрес";
            worksheet.Cells[3, 4] = "Телефон";
            worksheet.Cells[3, 5] = "Работа";


            // Делаем заголовки колонок жирными
            for (int i = 1; i <= 5; i++)
            {
                Range headerCell = worksheet.Cells[3, i];
                headerCell.Font.Bold = true;
            }


            // Данные
            int row = 4; // Начинаем с 4-й строки, так как 3-я строка занята заголовками колонок
            foreach (var student in Users)
            {
                worksheet.Cells[row, 1] = student.FIORod;
                worksheet.Cells[row, 2] = GetStudName(student.IDStud);
                worksheet.Cells[row, 3] = student.Adr;
                worksheet.Cells[row, 4] = student.Tel;
                worksheet.Cells[row, 5] = student.Rabota;
               

                row++;
            }

            // Автоматическое выравнивание столбцов
            worksheet.Columns.AutoFit();

            // Выровнять все ячейки по левому краю
            worksheet.Cells.HorizontalAlignment = XlHAlign.xlHAlignLeft;

            // Добавление границ для таблицы
            Range dataRange = worksheet.Range["A3", $"E{row - 1}"];
            dataRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            dataRange.Borders.Weight = XlBorderWeight.xlThin;

            // Показать Excel
            excelApp.Visible = true;
        }

        private void ExportDataPdf()
        {
            if (Users == null || !Users.Any())
            {
                MessageBox.Show("Нет данных для экспорта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Создать PDF документ для таблицы \"Родители\"?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // Создаем новый документ
            Document document = new Document();
            Section section = document.AddSection();
            section.PageSetup.LeftMargin = 70; //отступ слева для таблицы


            // Заголовок таблицы
            Paragraph title = section.AddParagraph("Таблица \"Родители\"");
            title.Format.Font.Bold = true;
            title.Format.Font.Size = 14;
            title.Format.SpaceAfter = "1cm";
            title.Format.LeftIndent = "2cm";

            title.Format.Alignment = ParagraphAlignment.Center;

            // Таблица
            Table table = section.AddTable();
            table.Borders.Width = 0.75;


            // Определение столбцов
            Column column1 = table.AddColumn("4cm");
            Column column2 = table.AddColumn("4cm");
            Column column3 = table.AddColumn("4cm");
            Column column4 = table.AddColumn("2cm");
            Column column5 = table.AddColumn("3cm");
     


            // Заголовок таблицы
            Row headerRow = table.AddRow();
            headerRow.Cells[0].AddParagraph("ФИО родителя");
            headerRow.Cells[1].AddParagraph("ФИО студента");
            headerRow.Cells[2].AddParagraph("Адрес");
            headerRow.Cells[3].AddParagraph("Телефон");
            headerRow.Cells[4].AddParagraph("Работа");

            headerRow.Format.Font.Bold = true;

            // Заполнение таблицы данными
            foreach (var student in Users)
            {
                Row row = table.AddRow();
                row.Cells[0].AddParagraph(student.FIORod);
                row.Cells[1].AddParagraph(GetStudName(student.IDStud));
                row.Cells[2].AddParagraph(student.Adr.ToString());
                row.Cells[3].AddParagraph(student.Tel);
                row.Cells[4].AddParagraph(student.Rabota);


            }

            // Рендеринг документа
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
            renderer.Document = document;
            renderer.RenderDocument();

            var filePath = "Таблица_Ученики.pdf";
            renderer.PdfDocument.Save(filePath);

            // Открытие созданного PDF файла
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }



    }
}