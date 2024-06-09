using Ivanova_UchitDn.Core;
using Ivanova_UchitDn.Model;
using MySqlConnector;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel.Application;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using MigraDoc.DocumentObjectModel.Tables;
using System.Diagnostics;
using System.Windows.Input;


namespace Ivanova_UchitDn.ViewModel
{
    public class UserData : INotifyPropertyChanged
    {
        private bool isUpdating = false;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string property)
        {
            if (property == null)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private ObservableCollection<User> UsersSelf;
        public IList<User> Users
        {
            get => UsersSelf;
            set
            {
                UsersSelf = value as ObservableCollection<User>;
                OnPropertyChanged("Users");
            }
        }

        private User NewUserSelf;
        public User NewUser
        {
            get => NewUserSelf;
            set
            {
                NewUserSelf = value;
                OnPropertyChanged("NewUser");
            }
        }

        private bool isAscending = true;
        public ICommand SortCommand { get; private set; }


        public UserData()
        {
            LoadData();

            SortCommand = new RelayCommand(SortUsers);

        }

        private void SortUsers()
        {
            if (isAscending)
            {
                Users = new ObservableCollection<User>(Users.OrderBy(u => u.Name));
            }
            else
            {
                Users = new ObservableCollection<User>(Users.OrderByDescending(u => u.Name));
            }
            isAscending = !isAscending;
            OnPropertyChanged(nameof(Users));
        }

        private async void LoadData()
        {
            if (isUpdating)
            {
                // Если уже идет процесс обновления данных, прерываем выполнение метода
                return;
            }
            isUpdating = true;


            NewUser = new User();
            EditUser = new User();
            await UserDataSelect();

            KCount = Users.Count;

            isUpdating = false;
        }

        private int _kCount;
        public int KCount
        {
            get { return _kCount; }
            set
            {
                _kCount = value;
                OnPropertyChanged(nameof(KCount));
            }
        }


        private async Task<bool> UserDataSelect()
        {
            Connector 
                con = new Connector();
            string 
                sql = string.Format("select * from `kurator` {0} limit {1}", SearchTypes(), 999);
            MySqlCommand 
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@text", string.Format("%{0}%", SearchText)));

            await con.GetOpen();
            UsersSelf = new ObservableCollection<User>();

            MySqlDataReader reader = await command.ExecuteReaderAsync();

            if (!reader.HasRows)
            {
                await con.GetClose();
                OnPropertyChanged("Users");
                return false;
            }

            while (await reader.ReadAsync())
            {
                await Task.Delay(1);
                UsersSelf.Add(new User()
                {
                    ID = (int)reader[0],
                    Name = (string)reader[1],
                    Login = (string)reader[2],
                    Parol = (string)reader[3],
                    Delete = new DeleteCommand(DeleteData, (int)reader[0])

                });

                OnPropertyChanged("Users");
            }

            await con.GetClose();
            OnPropertyChanged("Users");
            return true;
        }

     
        private string SearchTypes()
        {
            string sql = "";

            // Если ни один чекбокс не выбран, добавляем условия поиска для всех полей
            if (!SearchName && !SearchLog && !SearchParol)
            {
                sql = "`FIO_kurator` LIKE @text OR `login` LIKE @text OR `parol` LIKE @text";
            }
            else
            {
                List<string> conditions = new List<string>();

                // Добавляем условия поиска в зависимости от выбранных чекбоксов
                if (SearchName)
                    conditions.Add("`FIO_kurator` LIKE @text");
                if (SearchLog)
                    conditions.Add("`login` LIKE @text");
                if (SearchParol)
                    conditions.Add("`parol` LIKE @text");

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


        public User EditUserSelf { get; set; }
        public User EditUser
        {
            get => EditUserSelf;
            set
            {
                if (value == null)
                {
                    EditUserSelf = new User();
                    return;
                }

                EditUserSelf = new User()
                {
                    ID = value.ID,
                    Name = value.Name,
                    Login = value.Login,
                    Parol = value.Parol
                };


                OnPropertyChanged("EditUser");
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
            if (string.IsNullOrEmpty(NewUserSelf.Name))
            {
                MessageBox.Show("Не указано ФИО", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(NewUserSelf.Login))
            {
                MessageBox.Show("Не указан логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(NewUserSelf.Parol))
            {
                MessageBox.Show("Не указано пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            Connector
                 con = new Connector();
            string
                sql = "INSERT INTO `kurator` (FIO_kurator, login, parol) VALUES (@f, @l, @p);";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@f", NewUserSelf.Name));
            command.Parameters.Add(new MySqlParameter("@l", NewUserSelf.Login));
            command.Parameters.Add(new MySqlParameter("@p", NewUserSelf.Parol));

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
                sql = "DELETE FROM `kurator` WHERE `id_kurator` = @i;";
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
            if (string.IsNullOrEmpty(EditUser.Name))
            {
                MessageBox.Show("Не указано ФИО", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(EditUser.Login))
            {
                MessageBox.Show("Не указан логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(EditUser.Parol))
            {
                MessageBox.Show("Не указано пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (element is Grid g)
                g.Visibility = Visibility.Collapsed;

            Connector
                 con = new Connector();
            string
                sql = "UPDATE `kurator` SET `FIO_kurator`=@f, `login`=@l, `parol`=@p WHERE `id_kurator`=@i;";
            MySqlCommand
                command = new MySqlCommand(sql, con.GetCon());

            command.Parameters.Add(new MySqlParameter("@f", EditUser.Name));
            command.Parameters.Add(new MySqlParameter("@l", EditUser.Login));
            command.Parameters.Add(new MySqlParameter("@p", EditUser.Parol));
            command.Parameters.Add(new MySqlParameter("@i", EditUser.ID));

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

        public bool SearchLogSelf;
        public bool SearchLog
        {
            get => SearchLogSelf;
            set
            {
                SearchLogSelf = value;
                OnPropertyChanged("SearchLog");
                LoadData();
            }
        }

        public bool SearchParolSelf;
        public bool SearchParol
        {
            get => SearchParolSelf;
            set
            {
                SearchParolSelf = value;
                OnPropertyChanged("SearchParol");
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

        private IExcelExport ExportExcelSelf;
        public IExcelExport ExportExcel => ExportExcelSelf ?? (ExportExcelSelf = new IExcelExport(ExportDataExcel));
        private void ExportDataExcel()
        {
            if (Users == null || !Users.Any())
            {
                MessageBox.Show("Нет данных для экспорта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Создать Excel документ для таблицы \"Руководители\"?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var excelApp = new Excel();
            var workbook = excelApp.Workbooks.Add();
            Worksheet worksheet = (Worksheet)excelApp.ActiveSheet;

            // Переименовываем лист
            worksheet.Name = "Таблица \"Кураторы\"";


            // Заголовок таблицы
            worksheet.Cells[1, 1] = "Таблица \"Кураторы\"";
            Range titleRange = worksheet.Range["A1", "B1"];
            titleRange.Merge();
            titleRange.Font.Bold = true;
            titleRange.Font.Size = 12;
            titleRange.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // Заголовки колонок
            worksheet.Cells[3, 1] = "ID";
            worksheet.Cells[3, 2] = "ФИО руководителя";
            worksheet.Cells[3, 3] = "Логин";
            worksheet.Cells[3, 4] = "Пароль";

            // Делаем заголовки колонок жирными
            for (int i = 1; i <= 4; i++)
            {
                Range headerCell = worksheet.Cells[3, i];
                headerCell.Font.Bold = true;
            }

            // Данные
            int row = 4; // Начинаем с 4-й строки, так как 3-я строка занята заголовками колонок
            foreach (var user in Users)
            {
                worksheet.Cells[row, 1] = user.ID;
                worksheet.Cells[row, 2] = user.Name;
                worksheet.Cells[row, 3] = user.Login;
                worksheet.Cells[row, 4] = user.Parol;
                row++;
            }


            // Автоматическое выравнивание столбцов
            worksheet.Columns.AutoFit();

            // Выровнять все ячейки по левому краю
            worksheet.Cells.HorizontalAlignment = XlHAlign.xlHAlignLeft;

            // Добавление границ для таблицы
            Range dataRange = worksheet.Range["A3", $"D{row - 1}"];
            dataRange.Borders.LineStyle = XlLineStyle.xlContinuous;
            dataRange.Borders.Weight = XlBorderWeight.xlThin;


            // Показать Excel
            excelApp.Visible = true;
        }

        private IExcelExport ExportPdfSelf;
        public IExcelExport ExportPdf => ExportPdfSelf ?? (ExportPdfSelf = new IExcelExport(ExportDataPdf));
        private void ExportDataPdf()
        {
            if (Users == null || !Users.Any())
            {
                MessageBox.Show("Нет данных для экспорта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Создать PDF документ для таблицы \"Руководители\"?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // Создаем новый документ
            Document document = new Document();
            Section section = document.AddSection();
            section.PageSetup.LeftMargin = 75; //отступ слева для таблицы


            // Заголовок таблицы
            Paragraph title = section.AddParagraph("Таблица \"Классные руководители\"");
            title.Format.Font.Bold = true;
            title.Format.Font.Size = 14;
            title.Format.SpaceAfter = "1cm";
            title.Format.LeftIndent = "1cm";// остут слева для тайтла


            title.Format.Alignment = ParagraphAlignment.Center;

            // Таблица
            Table table = section.AddTable();
            table.Borders.Width = 0.75;
            table.Format.Alignment = ParagraphAlignment.Center;

            // Определение столбцов
            Column column1 = table.AddColumn("2cm");
            Column column2 = table.AddColumn("6cm");
            Column column3 = table.AddColumn("4cm");
            Column column4 = table.AddColumn("4cm");

            // Заголовок таблицы
            Row headerRow = table.AddRow();
            headerRow.Cells[0].AddParagraph("ID");
            headerRow.Cells[1].AddParagraph("ФИО руководителя");
            headerRow.Cells[2].AddParagraph("Логин");
            headerRow.Cells[3].AddParagraph("Пароль");

            headerRow.Format.Font.Bold = true;
            headerRow.Format.Alignment = ParagraphAlignment.Center;

            // Заполнение таблицы данными
            foreach (var user in Users)
            {
                Row row = table.AddRow();
                row.Cells[0].AddParagraph(user.ID.ToString());
                row.Cells[1].AddParagraph(user.Name);
                row.Cells[2].AddParagraph(user.Login);
                row.Cells[3].AddParagraph(user.Parol);
                row.Format.Alignment = ParagraphAlignment.Center;
            }

            // Рендеринг документа
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
            renderer.Document = document;
            renderer.RenderDocument();

            var filePath = "Таблица_руководитель.pdf";
            renderer.PdfDocument.Save(filePath);

            // Открытие созданного PDF файла
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }
    }
}