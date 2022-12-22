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

namespace MS.GUI.Windows.MEP
{
    /// <summary>
    /// Interaction logic for DuctInstallationView.xaml
    /// </summary>
    public partial class DuctInstallationView : Window
    {
        public DuctInstallationView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = true;
            }
            catch (System.InvalidOperationException)
            {
                MessageBox.Show("Нельзя создать семейство установки после ошибки! " +
                    "Можете сохранить конфигурацию, закрыть окно конструктора, " +
                    "запустить конструктор заново и загрузить в него сохраненную конфигурацию, " +
                    "после чего снова попытайтесь создаьт семейство. " +
                    "Если ошибка повторится, попробуйте очистить список оборудования.",
                    "Ошибка!");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = false;
            }
            catch (System.InvalidOperationException)
            {
                MessageBox.Show("Нельзя создать семейство установки после ошибки! " +
                    "Можете сохранить конфигурацию, закрыть окно конструктора, " +
                    "запустить конструктор заново и загрузить в него сохраненную конфигурацию, " +
                    "после чего снова попытайтесь создаьт семейство. " +
                    "Если ошибка повторится, попробуйте очистить список оборудования.",
                    "Ошибка!");
            }
        }
    }
}
