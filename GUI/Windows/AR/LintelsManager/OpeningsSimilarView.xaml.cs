using MS.GUI.ViewModels.AR.LintelsManager;
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

namespace MS.GUI.Windows.AR.LintelsManager
{
    /// <summary>
    /// Interaction logic for OpeningsSimilarView.xaml
    /// </summary>
    public partial class OpeningsSimilarView : Window
    {
        public OpeningsSimilarView()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void EditInstances_Clicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
