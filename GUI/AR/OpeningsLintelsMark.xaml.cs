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


        /// <summary>
        /// Конструктор формы маркировки перемычек в проемах
        /// </summary>
        /// <param name="openingsDto">Список проемов</param>
        /// <param name="endToEndNumbering">Если маркировка сквозная - true, поэтажно - false</param>
        public OpeningsLintelsMark(List<OpeningDto> openingsDto, bool endToEndNumbering)
        {
            _openings = openingsDto.Distinct().ToList();
            InitializeComponent();

            // Если сквозная маркировка
            if (endToEndNumbering)
            {
                OpeningDtosList.ItemsSource = _openings;
            }
            // Если поэтажная маркировка
            else
            {
                ListCollectionView collection = new ListCollectionView(_openings);
                collection.GroupDescriptions.Add(new PropertyGroupDescription("Level"));

                OpeningDtosList.ItemsSource = collection;
            }

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
