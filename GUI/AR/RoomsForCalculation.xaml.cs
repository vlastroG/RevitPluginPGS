using MS.Commands.AR.Models;
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
    /// Interaction logic for RoomsForCalculation.xaml
    /// </summary>
    public partial class RoomsForCalculation : Window
    {
        private List<RoomDto> _rooms;


        public RoomsForCalculation(List<RoomDto> rooms)
        {
            _rooms = rooms;
            InitializeComponent();

            RoomDtosList.ItemsSource = _rooms;
        }

        private void RoomDtosList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {

        }
    }
}
