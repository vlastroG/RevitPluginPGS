using Autodesk.Revit.DB;
using MS.Commands.AR;
using MS.Commands.AR.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private string _InputHeightString { get; set; }

        /// <summary>
        /// Если FinWallsHeightType == FinWallsHeightType.ByInput, 
        /// то возвращается введенное пользователем положительное число.
        /// Если пользователь ввел 0 или отрицательное число, 
        /// будет выброшено исключение ArgumentException.
        /// Если FinWallsHeightType != FinWallsHeightType.ByInput, будет возвращен 0;
        /// </summary>
        public int InputHeight
        {
            get
            {
                if (FinWallsHeightType == FinWallsHeight.ByInput)
                {
                    int h = 0;
                    bool result = Int32.TryParse(_InputHeightString, out h);
                    if (result == false)
                    {
                        throw new ArgumentException($"Нельзя преобразовать в число '{_InputHeightString}'");
                    }
                    if (h <= 0)
                    {
                        throw new ArgumentException($"Высота стены должна быть положительным числом! " +
                            $"Введено:'{h}'");
                    }
                    return h;
                }
                else return 0;
            }
        }

        /// <summary>
        /// Выбор пользователем варианта высоты отделочной стены: 
        /// по помещению, по элементу или по заданному числу
        /// </summary>
        public FinWallsHeight FinWallsHeightType { get; private set; }

        /// <summary>
        /// Словарь типоразмеров стен по значению параметра PGS_НаименованиеОтделки
        /// </summary>
        public IReadOnlyDictionary<string, WallType> DictWallTypeByFinName
        {
            get { return _dtos.ToDictionary(dto => dto.FinishingName, dto => dto.WallType); }
        }

        /// <summary>
        /// Форма для назначения типоразмеров отделочных стен 
        /// по значению параметра PGS_НаименованиеОтделки у отделываемого элемента
        /// </summary>
        /// <param name="dtos"></param>
        /// <param name="wallTypes"></param>
        public FinishingCreation(List<WallTypeFinishingDto> dtos, List<WallType> wallTypes)
        {
            _dtos = dtos;
            _InputHeightString = "2500";
            InitializeComponent();
            FinWallsHeightType = FinWallsHeight.ByInput;
            FinishingDto.ItemsSource = _dtos;
            wallTypesComboBoxColumn.ItemsSource = wallTypes;
            textBoxHeight.Text = _InputHeightString;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            _InputHeightString = textBoxHeight.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }



        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void HElems_Checked(object sender, RoutedEventArgs e)
        {
            FinWallsHeightType = FinWallsHeight.ByElement;
        }

        private void HRooms_Checked(object sender, RoutedEventArgs e)
        {
            FinWallsHeightType = FinWallsHeight.ByRoom;
        }

        private void HInput_Checked(object sender, RoutedEventArgs e)
        {
            FinWallsHeightType = FinWallsHeight.ByInput;
        }

    }
}
