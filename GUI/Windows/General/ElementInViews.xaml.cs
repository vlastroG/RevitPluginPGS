﻿using MS.GUI.ViewModels.General;
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

namespace MS.GUI.General
{
    /// <summary>
    /// Interaction logic for ElementInViews.xaml
    /// </summary>
    public partial class ElementInViews : Window
    {
        public ElementInViews(ElementInViewsViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ToList_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as ElementInViewsViewModel).GoToSheet = true;
            DialogResult = true;
        }
    }
}
