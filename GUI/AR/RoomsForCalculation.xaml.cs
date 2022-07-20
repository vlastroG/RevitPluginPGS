using MS.Commands.AR.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MS.GUI.AR
{
    /// <summary>
    /// Interaction logic for OpeningsLintels.xaml
    /// </summary>
    public partial class OpeningsLintels : Window
    {
        private List<RoomDto> _rooms;

        public IReadOnlyCollection<RoomDto> Rooms
        {
            get { return _rooms; }
        }


        public OpeningsLintels(List<RoomDto> rooms)
        {
            _rooms = rooms;
            InitializeComponent();

            RoomDtosList.ItemsSource = _rooms;
        }

        private void RoomDtosList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {

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
