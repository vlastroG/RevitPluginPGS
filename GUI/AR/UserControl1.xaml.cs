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

namespace MS.GUI.AR
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class InputRoomsArea : Window
    {
        public InputRoomsArea()
        {
            InitializeComponent();
        }

        public bool AllProjCalc { get; private set; }

        public string RevitTemplate { get; private set; }

        public int AreaRound { get; private set; }

        /*--------------------template_choice-----------------------------*/

        private void tempPGS_Checked(object sender, RoutedEventArgs e)
        {
            RevitTemplate = "pgs";
        }

        private void tempADSK_Checked(object sender, RoutedEventArgs e)
        {
            RevitTemplate = "adsk";
        }

        /*--------------------calc_scale_choice---------------------------*/

        private void allProj_Checked(object sender, RoutedEventArgs e)
        {
            AllProjCalc = true;
        }

        private void allView_Checked(object sender, RoutedEventArgs e)
        {
            AllProjCalc = false;
        }

        /*--------------------round_choice-------------------------------*/

        private void twoDecimal_Checked(object sender, RoutedEventArgs e)
        {
            AreaRound = 2;
        }

        private void threeDecimal_Checked(object sender, RoutedEventArgs e)
        {
            AreaRound = 3;
        }

        private void Execute_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
