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
    public class ElementInViewsViewModel : ViewModelBase
    {
        /// <summary>
        /// Список видов, на которых виден элемент
        /// </summary>
        public ReadOnlyObservableCollection<View> Views { get; private set; }

        /// <summary>
        /// Выбранный вид из списка
        /// </summary>
        private View _selectedView;

        /// <summary>
        /// Выбранный вид из списка
        /// </summary>
        public View SelectedView
        {
            get => _selectedView;
            set => Set(ref _selectedView, value);
        }


        public ElementInViewsViewModel(IEnumerable<View> views)
        {
            var collection = new ObservableCollection<View>(views);
            Views = new ReadOnlyObservableCollection<View>(collection);
        }
    }
}
