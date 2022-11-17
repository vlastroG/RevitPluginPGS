using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using MS.GUI.ViewModels.Base;
using MS.Utilites.Extensions;


namespace MS.GUI.KR
{
    /// <summary>
    /// Interaction logic for StairReinforcementView.xaml
    /// </summary>
    public partial class StairReinforcementView : Window
    {
        public StairReinforcementView()
        {
            InitializeComponent();
        }


        private void ValidationTextBoxCover(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[1-7][0-9]$");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ValidationTextBoxStep(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[1-9][0-9]{2}$");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
