using Autodesk.Revit.DB;
using MS.Commands.General.Models;
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
        public ReadOnlyObservableCollection<ViewWrapper> Views { get; private set; }

        private bool _goToSheet = false;

        public bool GoToSheet
        {
            get => _goToSheet;
            set => Set(ref _goToSheet, value);
        }

        /// <summary>
        /// Выбранный вид из списка
        /// </summary>
        private ViewWrapper _selectedView;

        /// <summary>
        /// Выбранный вид из списка
        /// </summary>
        public ViewWrapper SelectedView
        {
            get => _selectedView;
            set => Set(ref _selectedView, value);
        }


        public ElementInViewsViewModel(IEnumerable<View> views)
        {
            var viewWrappers = views.Select(v => new ViewWrapper(v));
            var collection = new ObservableCollection<ViewWrapper>(viewWrappers);
            Views = new ReadOnlyObservableCollection<ViewWrapper>(collection);
        }
    }
}
