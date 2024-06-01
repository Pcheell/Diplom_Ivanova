using Ivanova_UchitDn.View_Page;
using Ivanova_UchitDn.ViewModel;
using System.Windows;
using System.Windows.Controls;


namespace Ivanova_UchitDn
{
    /// <summary>
    /// Логика взаимодействия для MainWindowKurator.xaml
    /// </summary>
    public partial class MainWindowKurator : Window
    {
        private int userId; // Переменная для хранения ID пользователя

        public MainWindowKurator(int userId)
        {
            InitializeComponent();
            MyFrame.NavigationService.Navigate(new StudPage(userId));

            this.userId = userId; // Сохраняем ID пользователя
            var studData = new StudData(userId);
            var rodData = new RodData(userId);
        }
      

        private void Stud_btn(object sender, RoutedEventArgs e)
        {
            MyFrame.NavigationService.Navigate(new StudPage(userId));
        }

        private void Rod_btn(object sender, RoutedEventArgs e)
        {
            MyFrame.NavigationService.Navigate(new RoditeliPage(userId));
        }

        private void Nat_btn(object sender, RoutedEventArgs e)
        {
            MyFrame.NavigationService.Navigate(new NationPage());

        }

        private void LoginPageOpen(object sender, RoutedEventArgs e)
        {
            if (this.Owner is VhodPage v)
            {
                v.LoginTxt.Text = "";
                v.ParolTxt.Text = "";
            }

            this.Owner.Show();
            this.Close();
        }

    }
}
