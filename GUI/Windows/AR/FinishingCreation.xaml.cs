using Autodesk.Revit.DB;
using MS.RevitCommands.AR;
using MS.RevitCommands.AR.DTO;
using MS.RevitCommands.AR.Enums;
using MS.GUI.ViewModels.AR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MS.GUI.AR
{
    /// <summary>
    /// Interaction logic for FinishingCreation.xaml
    /// </summary>
    public partial class FinishingCreation : Window
    {
        /// <summary>
        /// Форма для назначения настроек генерации отделки
        /// </summary>
        /// <param name="viewModel">ViewModel для для настроек генерации отделки</param>
        public FinishingCreation(in FinishingCreationViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }


        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
