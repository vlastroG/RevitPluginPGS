using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for LintelsManagerView.xaml
    /// </summary>
    public partial class LintelsManagerView : Window
    {
        public LintelsManagerView()
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

        private void GoTo3D_clicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
