using Ivanova_UchitDn.Model;
using System.Windows;

namespace Ivanova_UchitDn.RoditeliToStudent
{
    /// <summary>
    /// Логика взаимодействия для RoditelWindow.xaml
    /// </summary>
    public partial class RoditelWindow : Window
    {
        public RoditelWindow(StudModel student)
        {
            InitializeComponent();

            // Устанавливаем DataContext для окна, чтобы оно могло получить информацию о студенте
            DataContext = new RoditelWindowViewModel(student);
        }
        public class RoditelWindowViewModel
        {
            public StudModel Student { get; set; }

            public RoditelWindowViewModel(StudModel student)
            {
                Student = student;
            }
        }
    }
}
