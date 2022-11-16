using Autodesk.Revit.DB;
using MS.GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.General
{
    public class SelectorViewModel : ViewModelBase
    {
        /// <summary>
        /// Путь к документу, в котором выбираются элементы
        /// </summary>
        private static string _docPath = String.Empty;

        /// <summary>
        /// Путь к документу, в котором выбираются элементы
        /// </summary>
        public string DocPath
        {
            get => _docPath;
            private set => Set(ref _docPath, value);
        }

        /// <summary>
        /// Название категории по умолчанию
        /// </summary>
        private static string _categoryDefault = "Помещения";

        /// <summary>
        /// Категории для выбора
        /// </summary>
        public static ObservableCollection<Category> Categories { get; private set; }
            = new ObservableCollection<Category>();

        /// <summary>
        /// Категория, заданная при выборе пользователем
        /// </summary>
        private static Category _selectedCategory;

        /// <summary>
        /// Категория, заданная при выборе пользователем
        /// </summary>
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                Set(ref _selectedCategory, value);
                _categoryDefault = SelectedCategory.Name;
            }
        }

        public SelectorViewModel() { }

        /// <summary>
        /// Настройка выбора категории
        /// </summary>
        /// <param name="categories">Список категорий, 
        /// доступных для выбора в текущем проекте</param>
        /// <param name="docPath">Путь к документу, в котором выбираются элементы</param>
        public SelectorViewModel(in IEnumerable<Category> categories, string docPath)
        {
            Categories = new ObservableCollection<Category>(categories);
            _selectedCategory
                = Categories.FirstOrDefault(c => c.Name.Equals(_categoryDefault));
            DocPath = docPath;
        }
    }
}
