using Ivanova_UchitDn.View_Page;
using System.Windows;


namespace Ivanova_UchitDn
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Kur_btn(object sender, RoutedEventArgs e)
        {
            MyFrame.NavigationService.Navigate(new Kurator());
        }


        private void Grup_btn(object sender, RoutedEventArgs e)
        {
            MyFrame.NavigationService.Navigate(new GrupPage());
        }

        private void Stud_btn(object sender, RoutedEventArgs e)
        {
            MyFrame.NavigationService.Navigate(new StudPage());
        }

        private void Rod_btn(object sender, RoutedEventArgs e)
        {
            MyFrame.NavigationService.Navigate(new RoditeliPage());
        }

        private void Test_btn(object sender, RoutedEventArgs e)
        {
            MyFrame.NavigationService.Navigate(new TestDesign());
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
