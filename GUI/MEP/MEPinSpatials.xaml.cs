using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MS.GUI.MEP
{
    /// <summary>
    /// Interaction logic for MEPinSpatials.xaml
    /// </summary>
    public partial class MEPinSpatials : Window
    {
        public bool SpatialsFromLinks { get; private set; }

        private List<BuiltInCategory> _Categories = new List<BuiltInCategory>();

        public IReadOnlyCollection<BuiltInCategory> Categories { get { return _Categories; } }

        public MEPinSpatials()
        {
            InitializeComponent();
        }

        private void UpdateCategories(bool? checkBoxIsChecked, BuiltInCategory category)
        {
            if (!checkBoxIsChecked ?? false)
            {
                _Categories.RemoveAll(c => c == category);
            }
            else
            {
                if (!_Categories.Contains(category))
                {
                    _Categories.Add(category);
                }
            }
        }

        private void PipelineFittings_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategories(PipelineFittings.IsChecked, BuiltInCategory.OST_PipeAccessory);
        }

        private void Equipment_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategories(Equipment.IsChecked, BuiltInCategory.OST_MechanicalEquipment);
        }

        private void DuctTerminal_Checked(object sender, RoutedEventArgs e)
        {
            UpdateCategories(DuctTerminal.IsChecked, BuiltInCategory.OST_DuctTerminal);
        }

        private void LinkAR_Checked(object sender, RoutedEventArgs e)
        {
            if (Convert.ToBoolean(LinkAR.IsChecked))
            {
                SpatialsFromLinks = true;
            }
        }

        private void Spaces_Checked(object sender, RoutedEventArgs e)
        {
            if (Convert.ToBoolean(Spaces.IsChecked))
            {
                SpatialsFromLinks = false;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            UpdateCategories(PipelineFittings.IsChecked, BuiltInCategory.OST_PipeAccessory);
            UpdateCategories(Equipment.IsChecked, BuiltInCategory.OST_MechanicalEquipment);
            UpdateCategories(DuctTerminal.IsChecked, BuiltInCategory.OST_DuctTerminal);
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Equipment_UnChecked(object sender, RoutedEventArgs e)
        {
            UpdateCategories(Equipment.IsChecked, BuiltInCategory.OST_MechanicalEquipment);
        }

        private void PipelineFittings_UnChecked(object sender, RoutedEventArgs e)
        {
            UpdateCategories(PipelineFittings.IsChecked, BuiltInCategory.OST_PipeAccessory);
        }

        private void DuctTerminal_UnChecked(object sender, RoutedEventArgs e)
        {
            UpdateCategories(DuctTerminal.IsChecked, BuiltInCategory.OST_DuctTerminal);
        }
    }
}
