using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.General.Models
{
    /// <summary>
    /// Обертка для класса вида
    /// </summary>
    public class ViewWrapper
    {
        /// <summary>
        /// Видовой экран
        /// </summary>
        private readonly View _view;


        /// <summary>
        /// Видовой экран
        /// </summary>
        public View View { get { return _view; } }

        /// <summary>
        /// Номер листа, на котором размещен вид
        /// </summary>
        public string SheetNumber
        {
            get
            {
                return _view.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NUMBER).AsValueString() ?? string.Empty;
            }
        }

        /// <summary>
        /// Конструктор обертки вида
        /// </summary>
        /// <param name="view">Вид, для которого создается обертка</param>
        public ViewWrapper(View view)
        {
            _view = view;
        }
    }
}
