using Ivanova_UchitDn.Core;
using Ivanova_UchitDn.Model;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
using Ivanova_UchitDn.View_Page;
using PdfSharp;
using MigraDoc.DocumentObjectModel.Shapes;

namespace Ivanova_UchitDn.ViewModel
{
    public class StudData : INotifyPropertyChanged
    {
        private readonly int userId;
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


        private IList<ListItemSelectN> ListItemSelectNationSelf;
        public IList<ListItemSelectN> ListItemSelectNation
        {
            get => ListItemSelectNationSelf;
            set => ListItemSelectNationSelf = value;
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

        public StudData(int userId)
        {
            SearchDataStartSelf = DateTime.Today;
            SearchDataEndSelf = DateTime.Today;

            ListItemSelectGrup = new ObservableCollection<ListItemSelectG>();
            ListItemSelectNation = new ObservableCollection<ListItemSelectN>();
            LoadData();
            this.userId = userId;
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
            _ = await NationDataList();
            OnPropertyChanged("SearchSelectGroup");
            OnPropertyChanged("SearchSelectNation");
            _ = await StudDataSelect();
            OnPropertyChanged("Users");

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

        private async Task<bool> NationDataList()
        {
            Connector
                 con = new Connector();
            string
                sql = "select * from `nation` limit 50";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            await con.GetOpen();

            MySqlDataReader
                reader = await command.ExecuteReaderAsync();

            ListItemSelectNationSelf = new ObservableCollection<ListItemSelectN>() { new ListItemSelectN() { NameNation = "Все" } };

            if (!reader.HasRows)
            {
                await con.GetClose();
                return false;
            }

            while (await reader.ReadAsync())
            {
                ListItemSelectNationSelf.Add(new ListItemSelectN()
                {
                    IDNation = (int)reader["id_nation"],
                    NameNation = string.Format("{0}", reader["name_nation"])
                });

                OnPropertyChanged("ListItemSelectNation");
            }

            NewStudSelf.ListItemSelectNation = ListItemSelectNationSelf;
            await con.GetClose();
            return true;
        }


        private async Task<bool> StudDataSelect()
        {
            try
            {
                Connector con = new Connector();
                string existingSql = "SELECT * FROM `kart_stud`";

                // Для администратора выводим всех учеников без ограничений
                if (userId != 1)
                {
                    existingSql += " WHERE `id_grup` IN (SELECT `id_grup` FROM `grup` WHERE `id_kurator` = @kuratorId)";
                }

                string sql = SearchTypes(existingSql);

                MySqlCommand command = new MySqlCommand(sql, con.GetCon());

                // Параметры для поиска
                command.Parameters.Add(new MySqlParameter("@kuratorId", userId));
                command.Parameters.Add(new MySqlParameter("@text", string.Format("%{0}%", SearchText)));
                command.Parameters.Add(new MySqlParameter("@grup", SearchSelectGroup));
                command.Parameters.Add(new MySqlParameter("@nation", SearchSelectNation));
                command.Parameters.Add(new MySqlParameter("@date_start", SearchDataStart.ToString("yyyy-MM-dd")));
                command.Parameters.Add(new MySqlParameter("@date_end", SearchDataEnd.ToString("yyyy-MM-dd")));

                await con.GetOpen();
                Users = new ObservableCollection<StudModel>();

                MySqlDataReader reader = await command.ExecuteReaderAsync();

                if (!reader.HasRows)
                {
                    await con.GetClose();
                    OnPropertyChanged("Users");
                    return false;
                }

                while (await reader.ReadAsync())
                {
                    Users.Add(new StudModel()
                    {
                        IDStud = (int)reader["id_stud"],
                        IDGrup = (int)reader["id_grup"],
                        ListItemSelectGrup = ListItemSelectGrupSelf,
                        FIOStud = (string)reader["FIO_stud"],
                        DRStud = Convert.ToDateTime(reader["dr_stud"]),
                        Adr = (string)reader["address_stud"],
                        FAdr = (string)reader["faddress_stud"],
                        Tel = (string)reader["tel_stud"],
                        IDNation = (int)reader["id_nation"],
                        ListItemSelectNation = ListItemSelectNationSelf,
                        Section = reader["section_stud"] == DBNull.Value ? null : (string)reader["section_stud"],
                        Img = reader["img_stud"] == DBNull.Value ? null : (string)reader["img_stud"],
                        Note = reader["note_stud"] == DBNull.Value ? null : (string)reader["note_stud"],
                    Delete = new DeleteCommand(DeleteData, (int)reader[0])
                    });

                    OnPropertyChanged("Users");
                }

                await con.GetClose();
                return true;
            }
            catch (Exception ex)
            {
                // Если возникает исключение, выводим его сообщение в консоль для отладки
                Console.WriteLine("Вызвано исключение: " + ex.Message);
                return false;
            }
        }




        private string SearchTypes(string existingSql)
        {
            try
            {
                List<string> conditions = new List<string>();

                // Добавляем условия поиска в зависимости от выбранных чекбоксов
                if (SearchName)
                    conditions.Add("`FIO_stud` LIKE @text");
                if (SearchAdr)
                    conditions.Add("`address_stud` LIKE @text");
                if (SearchFAdr)
                    conditions.Add("`faddress_stud` LIKE @text");
                if (SearchTel)
                    conditions.Add("`tel_stud` LIKE @text");
                if (SearchGrup)
                    conditions.Add("`id_grup` IN (SELECT `id_grup` FROM `grup` WHERE `name_grup` LIKE @text)");
                if (SearchNation)
                    conditions.Add("`id_nation` IN (SELECT `id_nation` FROM `nation` WHERE `name_nation` LIKE @text)");
                if (SearchSection)
                    conditions.Add("`section_stud` LIKE @text");
                if (SearchNote)
                    conditions.Add("`note_stud` LIKE @text");

                // Если ни один чекбокс не выбран, добавляем условия поиска для всех полей
                if (!SearchName && !SearchAdr && !SearchFAdr && !SearchTel && !SearchGrup && !SearchNation && !SearchSection && !SearchNote && !string.IsNullOrEmpty(SearchText))
                {
                    conditions.Add("`FIO_stud` LIKE @text " +
                        "OR `address_stud` LIKE @text " +
                        "OR `faddress_stud` LIKE @text " +
                        "OR `tel_stud` LIKE @text " +
                        "OR `id_grup` IN (SELECT `id_grup` FROM `grup` WHERE `name_grup` LIKE @text)" +
                        "OR `id_nation` IN (SELECT `id_nation` FROM `nation` WHERE `name_nation` LIKE @text)" +
                        "OR `section_stud` LIKE @text " +
                        "OR `note_stud` LIKE @text "
                        );
                }

                // Если есть добавленные условия, объединяем их с основным SQL запросом
                if (conditions.Any())
                {
                    string whereClause = string.Join(" OR ", conditions);
                    existingSql += " WHERE " + whereClause; // Заменяем AND на WHERE, так как это первое условие
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
                    IDNation = value.IDNation,
                    ListItemSelectGrup = ListItemSelectGrupSelf,
                    ListItemSelectNation = ListItemSelectNationSelf,
                    FIOStud = value.FIOStud,
                    DRStud = value.DRStud,
                    Adr = value.Adr,
                    FAdr = value.FAdr,
                    Tel = value.Tel,
                    Section = value.Section,
                    Note = value.Note,
                    Img = value.Img
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

            if (GroupInsertNotValidNation(NewStudSelf.IDNation))
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
                sql = "INSERT INTO `kart_stud` (id_grup, id_nation, FIO_stud, dr_stud, address_stud, faddress_stud, tel_stud, section_stud, note_stud, img_stud) " +
                "VALUES (@idg, @idn, @f, @d, @a, @fa, @t, @s, @note, @img);";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@idg", NewStudSelf.IDGrup));
            command.Parameters.Add(new MySqlParameter("@idn", NewStudSelf.IDNation));
            command.Parameters.Add(new MySqlParameter("@f", NewStudSelf.FIOStud));
            command.Parameters.Add(new MySqlParameter("@d", NewStudSelf.DRStud));
            command.Parameters.Add(new MySqlParameter("@a", NewStudSelf.Adr));
            command.Parameters.Add(new MySqlParameter("@fa", NewStudSelf.FAdr));
            command.Parameters.Add(new MySqlParameter("@t", NewStudSelf.Tel));
            command.Parameters.Add(new MySqlParameter("@s", NewStudSelf.Section));
            command.Parameters.Add(new MySqlParameter("@note", NewStudSelf.Note));
            command.Parameters.Add(new MySqlParameter("@img", NewStudSelf.Img));

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

            if (GroupInsertNotValidNation(EditStud.IDNation))
            {
                MessageBox.Show("Не выбрана национальность");
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
                sql = "UPDATE `kart_stud` SET `id_grup`=@idg, `id_nation`=@idn, `FIO_stud`=@f, `dr_stud`=@d, " +
                "`address_stud`=@a, `faddress_stud`=@fa, `tel_stud`=@t, `section_stud`=@s, `note_stud`=@note, `img_stud`=@img WHERE `id_stud`=@i;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@idg",EditStud.IDGrup));
            command.Parameters.Add(new MySqlParameter("@idn",EditStud.IDNation));
            command.Parameters.Add(new MySqlParameter("@f", EditStud.FIOStud));
            command.Parameters.Add(new MySqlParameter("@d", EditStud.DRStud));
            command.Parameters.Add(new MySqlParameter("@a", EditStud.Adr));
            command.Parameters.Add(new MySqlParameter("@fa", EditStud.FAdr));
            command.Parameters.Add(new MySqlParameter("@t", EditStud.Tel));
            command.Parameters.Add(new MySqlParameter("@s", EditStud.Section));
            command.Parameters.Add(new MySqlParameter("@note", EditStud.Note));
            command.Parameters.Add(new MySqlParameter("@img", EditStud.Img));
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

        private bool GroupInsertNotValidNation(int key)
        {
            if (key < 1)
                return true;

            foreach (ListItemSelectN nation in ListItemSelectNation)
            {
                if (nation.IDNation != key)
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

        public bool SearchGrupSelf;
        public bool SearchGrup
        {
            get => SearchGrupSelf;
            set
            {
                SearchGrupSelf = value;
                OnPropertyChanged("SearchGrup");
                LoadData();

            }
        }

        public bool SearchNationSelf;
        public bool SearchNation
        {
            get => SearchNationSelf;
            set
            {
                SearchNationSelf = value;
                OnPropertyChanged("SearchNation");
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

        public bool SearchFAdrSelf;
        public bool SearchFAdr
        {
            get => SearchFAdrSelf;
            set
            {
                SearchFAdrSelf = value;
                OnPropertyChanged("SearchFAdr");
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

        private bool SearchSectionSelf;
        public bool SearchSection
        {
            get => SearchSectionSelf;
            set
            {
                SearchSectionSelf = value;
                OnPropertyChanged("SearchSection");
                LoadData();

            }
        }

        private bool SearchNoteSelf;
        public bool SearchNote
        {
            get => SearchNoteSelf;
            set
            {
                SearchNoteSelf = value;
                OnPropertyChanged("SearchNote");
                LoadData();

            }
        }


        private int SearchSelectGroupSelf;
        public int SearchSelectGroup
        {
            get => SearchSelectGroupSelf;
            set { SearchSelectGroupSelf = value; LoadData(); OnPropertyChanged("SearchSelectGroup"); }
        }

        private int SearchSelectNationSelf;
        public int SearchSelectNation
        {
            get => SearchSelectNationSelf;
            set { SearchSelectNationSelf = value; LoadData(); OnPropertyChanged("SearchSelectNation"); }
        }



        private bool SearchDateSelf;
        public bool SearchDate
        {
            get => SearchDateSelf;
            set { SearchDateSelf = value; OnPropertyChanged("SearchDate"); }
        }

        //private DateTime searchDataStartSelf = DateTime.Now.AddMonths(-1); // Начальная дата
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
                OnPropertyChanged("SearchDataStart");
            }
        }

        //private DateTime searchDataEndSelf = DateTime.Now; // Конечная дата

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
                OnPropertyChanged("SearchDataEnd");
            }
        }

        private IExcelExport ExportPdfSelf;
        public IExcelExport ExportPdf => ExportPdfSelf ?? (ExportPdfSelf = new IExcelExport(ExportDataPdf));

        private IExcelExport ExportExcelSelf;
        public IExcelExport ExportExcel => ExportExcelSelf ?? (ExportExcelSelf = new IExcelExport(ExportDataExcel));

        private string GetGrupName(int grupID)
        {
            foreach (var grup in ListItemSelectGrup)
            {
                if (grup.IDGrup == grupID)
                {
                    return grup.NameGrup;
                }
            }
            return ""; 
        }

        private string GetNationName(int nationID)
        {
            foreach (var nation in ListItemSelectNation)
            {
                if (nation.IDNation == nationID)
                {
                    return nation.NameNation;
                }
            }
            return "";
        }


        private void ExportDataExcel()
        {
            if (Users == null || !Users.Any())
            {
                MessageBox.Show("Нет данных для экспорта", "Ошибка");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Создать Excel документ для таблицы \"Ученики\"?", "Подтверждение", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var excelApp = new Excel();
            var workbook = excelApp.Workbooks.Add();
            Worksheet worksheet = (Worksheet)excelApp.ActiveSheet;


            // Переименовываем лист
            worksheet.Name = "Таблица \"Ученики\"";

            // Заголовок таблицы
            worksheet.Cells[1, 1] = "Таблица \"Ученики\"";
            Range titleRange = worksheet.Range["A1", "B1"];
            titleRange.Merge();
            titleRange.Font.Bold = true;
            titleRange.Font.Size = 12;
            titleRange.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // Заголовки колонок
            worksheet.Cells[3, 1] = "ФИО ученика";
            worksheet.Cells[3, 2] = "Класс";
            worksheet.Cells[3, 3] = "Дата рождения";
            worksheet.Cells[3, 4] = "Адрес";
            worksheet.Cells[3, 5] = "Фактический адрес";
            worksheet.Cells[3, 6] = "Телефон";
            worksheet.Cells[3, 7] = "Национальность";
            worksheet.Cells[3, 8] = "Секция";
            worksheet.Cells[3, 9] = "Примечание";


            // Делаем заголовки колонок жирными
            for (int i = 1; i <= 9; i++)
            {
                Range headerCell = worksheet.Cells[3, i];
                headerCell.Font.Bold = true;
            }


            // Данные
            int row = 4; // Начинаем с 4-й строки, так как 3-я строка занята заголовками колонок
            foreach (var student in Users)
            {
                worksheet.Cells[row, 1] = student.FIOStud;
                worksheet.Cells[row, 2] = GetGrupName(student.IDGrup);
                worksheet.Cells[row, 3] = student.DRStud;
                worksheet.Cells[row, 4] = student.Adr;
                worksheet.Cells[row, 5] = student.FAdr;
                worksheet.Cells[row, 6] = student.Tel;
                worksheet.Cells[row, 7] = GetNationName(student.IDNation);
                worksheet.Cells[row, 8] = student.Section;
                worksheet.Cells[row, 9] = student.Note;

                row++;
            }

            // Автоматическое выравнивание столбцов
            worksheet.Columns.AutoFit();

            // Выровнять все ячейки по левому краю
            worksheet.Cells.HorizontalAlignment = XlHAlign.xlHAlignLeft;

            // Добавление границ для таблицы
            Range dataRange = worksheet.Range["A3", $"I{row - 1}"];
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

            MessageBoxResult result = MessageBox.Show("Создать PDF документ для таблицы \"Ученики\"?", "Подтверждение", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // Создаем новый документ
            Document document = new Document();
            Section section = document.AddSection();
            section.PageSetup.LeftMargin = 15;


            // Заголовок таблицы
            Paragraph title = section.AddParagraph("Таблица \"Ученики\"");
            title.Format.Font.Bold = true;
            title.Format.Font.Size = 14;
            title.Format.SpaceAfter = "1cm";
            title.Format.LeftIndent = "2cm";

            title.Format.Alignment = ParagraphAlignment.Center;

            // Таблица
            Table table = section.AddTable();
            table.Borders.Width = 0.75;

            // Определение столбцов
            Column column1 = table.AddColumn("2cm");
            Column column2 = table.AddColumn("2cm");
            Column column3 = table.AddColumn("1.75cm");
            Column column4 = table.AddColumn("3cm");
            Column column5 = table.AddColumn("3cm");
            Column column6 = table.AddColumn("2cm");
            Column column7 = table.AddColumn("2.5cm");
            Column column8 = table.AddColumn("2cm");
            Column column9 = table.AddColumn("2cm");


            // Заголовок таблицы
            Row headerRow = table.AddRow();
            headerRow.Cells[0].AddParagraph("ФИО ученика").Format.Font.Size = 8;
            headerRow.Cells[1].AddParagraph("Класс").Format.Font.Size = 8;
            headerRow.Cells[2].AddParagraph("Дата рождения").Format.Font.Size = 8;
            headerRow.Cells[3].AddParagraph("Адрес").Format.Font.Size = 8;
            headerRow.Cells[4].AddParagraph("Фактический адрес").Format.Font.Size = 8;
            headerRow.Cells[5].AddParagraph("Телефон").Format.Font.Size = 8;
            headerRow.Cells[6].AddParagraph("Национальность").Format.Font.Size = 8;
            headerRow.Cells[7].AddParagraph("Секция").Format.Font.Size = 8;
            headerRow.Cells[8].AddParagraph("Примечание").Format.Font.Size = 8;

            headerRow.Format.Font.Bold = true;

            // Заполнение таблицы данными
            foreach (var student in Users)
            {
                Row row = table.AddRow();
                row.Cells[0].AddParagraph(student.FIOStud).Format.Font.Size = 8;
                row.Cells[1].AddParagraph(GetGrupName(student.IDGrup)).Format.Font.Size = 8;
                row.Cells[2].AddParagraph(student.DRStud.ToString()).Format.Font.Size = 8;
                row.Cells[3].AddParagraph(student.Adr).Format.Font.Size = 8;
                row.Cells[4].AddParagraph(student.FAdr).Format.Font.Size = 8;
                row.Cells[5].AddParagraph(student.Tel).Format.Font.Size = 8;
                row.Cells[6].AddParagraph(GetNationName(student.IDNation)).Format.Font.Size = 8;
                row.Cells[7].AddParagraph(student.Section ?? "").Format.Font.Size = 8;
                row.Cells[8].AddParagraph(student.Note ?? "").Format.Font.Size = 8;


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