﻿using Ivanova_UchitDn.ViewModel;
using System;
using System.Collections.Generic;
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

namespace Ivanova_UchitDn.View_Page
{
    /// <summary>
    /// Логика взаимодействия для RoditeliPage.xaml
    /// </summary>
    public partial class RoditeliPage : Page
    {
        public RoditeliPage()
        {
            InitializeComponent();
            GridData.DataContext = new RodData();
        }

        private void AddGrup(object sender, RoutedEventArgs e)
        {
            ShowInsertData.Visibility = Visibility.Visible;

        }

        private void CloseEdit(object sender, MouseButtonEventArgs e)
        {
            ShowEditData.Visibility = Visibility.Collapsed;

        }

        private void CloseInsert(object sender, MouseButtonEventArgs e)
        {
            ShowInsertData.Visibility = Visibility.Collapsed;

        }

        private void OpenInsert(object sender, RoutedEventArgs e)
        {
            ShowInsertData.Visibility = Visibility.Visible;

        }

        private void OpenEdit(object sender, RoutedEventArgs e)
        {
            ShowEditData.Visibility = Visibility.Visible;

        }
    }
}
