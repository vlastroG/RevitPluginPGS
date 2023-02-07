using MS.RevitCommands.AR.DTO;
using MS.GUI.ViewModels.AR;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MS.GUI.AR
{
    /// <summary>
    /// Окно для выбора помещений для расчета площадей проемов
    /// </summary>
    public partial class RoomsForCalculation : Window
    {
        public RoomsForCalculation(OpeningsAreaViewModel viewModel)
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
    }
}
