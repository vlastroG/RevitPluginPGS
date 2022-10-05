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

        private readonly List<CeilingType> _ceilingTypes;

        public CeilingType Ceiling
        {
            get
            {
                try
                {
                    return _ceilingTypes[comboBoxCeilingType.SelectedIndex];
                }
                catch (ArgumentOutOfRangeException)
                {
                    return null;
                }
            }
        }

        private string _inputWallsHeightString { get; set; }

        private string _inputCeilingsHeightString { get; set; }

        public bool CreateWalls { get; private set; }

        public bool CreateCeilings { get; private set; }

        /// <summary>
        /// Если FinWallsHeightType == FinWallsHeightType.ByInput, 
        /// то возвращается введенное пользователем положительное число.
        /// Если пользователь ввел 0 или отрицательное число, 
        /// будет выброшено исключение ArgumentException.
        /// Если FinWallsHeightType != FinWallsHeightType.ByInput, будет возвращен 0;
        /// </summary>
        public int InputWallsHeight
        {
            get
            {
                if (FinWallsHeightType == FinWallsHeight.ByInput)
                {
                    int h = 0;
                    bool result = Int32.TryParse(_inputWallsHeightString, out h);
                    if (result == false)
                    {
                        throw new ArgumentException($"Нельзя преобразовать в число '{_inputWallsHeightString}'");
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
        /// Высота потолка, заданная пользователем
        /// </summary>
        public int InputCeilingsHeight
        {
            get
            {
                int h = 0;
                bool result = Int32.TryParse(_inputCeilingsHeightString, out h);
                if (result == false)
                {
                    throw new ArgumentException($"Нельзя преобразовать в число '{_inputWallsHeightString}'");
                }
                if (h <= 0)
                {
                    throw new ArgumentException($"Высота стены должна быть положительным числом! " +
                        $"Введено:'{h}'");
                }
                return h;
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
        public FinishingCreation(List<WallTypeFinishingDto> dtos, List<WallType> wallTypes, List<CeilingType> ceilingTypes)
        {
            _dtos = dtos;
            _inputWallsHeightString = "2500";
            _inputCeilingsHeightString = "2500";
            _ceilingTypes = ceilingTypes;
            InitializeComponent();
            FinWallsHeightType = FinWallsHeight.ByInput;
            FinishingDto.ItemsSource = _dtos;
            wallTypesComboBoxColumn.ItemsSource = wallTypes;
            comboBoxCeilingType.ItemsSource = _ceilingTypes;
            comboBoxCeilingType.SelectedIndex = 0;
            textBoxHeight.Text = _inputWallsHeightString;
            textBoxCeilingHeight.Text = _inputCeilingsHeightString;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            _inputWallsHeightString = textBoxHeight.Text;
            _inputCeilingsHeightString = textBoxCeilingHeight.Text;
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

        private void checkBoxCreateWalls_Checked(object sender, RoutedEventArgs e)
        {
            CreateWalls = true;
        }
        private void checkBoxCreateWalls_Unchecked(object sender, RoutedEventArgs e)
        {
            CreateWalls = false;
        }

        private void checkBoxCreateCeiling_Checked(object sender, RoutedEventArgs e)
        {
            CreateCeilings = true;
        }

        private void checkBoxCreateCeiling_Unchecked(object sender, RoutedEventArgs e)
        {
            CreateCeilings = false;
        }

    }
}
