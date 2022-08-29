using Autodesk.Revit.DB;
using MS.Commands.AR.DTO;
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
    /// Interaction logic for FinishingCreation.xaml
    /// </summary>
    public partial class FinishingCreation : Window
    {
        private List<WallTypeFinishingDto> _dtos;

        public IReadOnlyCollection<WallTypeFinishingDto> Dtos
        {
            get { return _dtos; }
        }

        public FinishingCreation(List<WallTypeFinishingDto> dtos, List<WallType> wallTypes)
        {
            _dtos = dtos;
            InitializeComponent();
            FinishingDto.ItemsSource = _dtos;
            wallTypesComboBoxColumn.ItemsSource = wallTypes;
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
