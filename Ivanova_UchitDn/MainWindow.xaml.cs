using Ivanova_UchitDn.Core;
using Ivanova_UchitDn.View_Page;
using Ivanova_UchitDn.ViewModel;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void Pred_btn(object sender, RoutedEventArgs e)
        {
            MyFrame.NavigationService.Navigate(new PredmetPage());
        }

        private void Stud_btn(object sender, RoutedEventArgs e)
        {
            MyFrame.NavigationService.Navigate(new StudPage());
        }

        private void Rod_btn(object sender, RoutedEventArgs e)
        {
            MyFrame.NavigationService.Navigate(new RoditeliPage());
        }

        private void Usrev_btn(object sender, RoutedEventArgs e)
        {
            MyFrame.NavigationService.Navigate(new UspPage());
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
