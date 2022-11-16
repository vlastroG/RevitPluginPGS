using Autodesk.Revit.DB;
using MS.GUI.ViewModels.General;
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

namespace MS.GUI.General
{
    /// <summary>
    /// Interaction logic for CategoryInput.xaml
    /// </summary>
    public partial class CategoryInput : Window
    {
        /// <summary>
        /// Конструктор WPF формы для выбора категории
        /// </summary>
        public CategoryInput()
        {
            InitializeComponent();
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

    public static class MyCommands
    {
        public static readonly RoutedUICommand Submit = new RoutedUICommand
            (
                "Submit",
                "Submit",
                typeof(MyCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.Enter)
                }
            );

        public static readonly RoutedUICommand Cancel = new RoutedUICommand
            (
                "Cancel",
                "Cancel",
                typeof(MyCommands),
                new InputGestureCollection()
                {
                    new KeyGesture (Key.Escape)
                }
            );
    }
}
