using Autodesk.Revit.DB;
using MS.GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.MEP
{
    public class MEPinSpatialsViewModel : ViewModelBase
    {
        /// <summary>
        /// True - пространства из текущего файла
        /// False - помещения из связей АР
        /// </summary>
        private static bool _spatialsFromMEP = true;

        /// <summary>
        /// True - пространства из текущего файла
        /// False - помещения из связей АР
        /// </summary>
        public bool SpatialsFromMEP
        {
            get => _spatialsFromMEP;
            set => Set(ref _spatialsFromMEP, value);
        }

        /// <summary>
        /// Обрабатывать арматуру трубопроводов
        /// </summary>
        private static bool _addPipelineFittings;

        /// <summary>
        /// Обрабатывать арматуру трубопроводов
        /// </summary>
        public bool AddPipelineFittings
        {
            get => _addPipelineFittings;
            set => Set(ref _addPipelineFittings, value);
        }

        /// <summary>
        /// Обрабатывать оборудование
        /// </summary>
        private static bool _addEquipment;

        /// <summary>
        /// Обрабатывать оборудование
        /// </summary>
        public bool AddEquipment
        {
            get => _addEquipment;
            set => Set(ref _addEquipment, value);
        }

        /// <summary>
        /// Обрабатывать воздухораспределители
        /// </summary>
        private static bool _addDuctTerminals;

        /// <summary>
        /// Обрабатывать воздухораспределители
        /// </summary>
        public bool AddDuctTerminals
        {
            get => _addDuctTerminals;
            set => Set(ref _addDuctTerminals, value);
        }


        /// <summary>
        /// Возвращает коллекцию категорий элементов для обработки
        /// </summary>
        /// <returns>Коллекция встроенных категорий</returns>
        public ICollection<BuiltInCategory> GetCategories()
        {
            ICollection<BuiltInCategory> categories = new List<BuiltInCategory>();

            if (_addDuctTerminals)
            {
                categories.Add(BuiltInCategory.OST_DuctTerminal);
            }
            if (_addEquipment)
            {
                categories.Add(BuiltInCategory.OST_MechanicalEquipment);
            }
            if (_addPipelineFittings)
            {
                categories.Add(BuiltInCategory.OST_PipeAccessory);
            }

            return categories;
        }
    }
}
