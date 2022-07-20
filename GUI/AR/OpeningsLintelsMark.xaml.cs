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
    /// Interaction logic for OpeningsLintelsMark.xaml
    /// </summary>
    public partial class OpeningsLintelsMark : Window
    {
        private List<OpeningDto> _openings;


        public IReadOnlyCollection<OpeningDto> Openings
        {
            get { return _openings; }
        }

        public OpeningsLintelsMark(List<OpeningDto> openingsDto)
        {
            _openings = openingsDto;
            InitializeComponent();

            OpeningDtosList.ItemsSource = _openings;
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
