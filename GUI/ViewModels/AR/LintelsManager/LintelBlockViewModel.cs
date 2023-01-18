using MS.GUI.ViewModels.Base;
using MS.RevitCommands.AR.Models;
using MS.RevitCommands.AR.Models.Lintels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.AR.LintelsManager
{
    /// <summary>
    /// Модель представления окна для редактирования свойств перемычки из бетонных брусков
    /// </summary>
    public class LintelBlockViewModel : ViewModelBase, ILintelCreator
    {
        /// <summary>
        /// Конструктор модели представления окна для редактирования свойств перемычки из бетонных брусков по умолчанию
        /// </summary>
        public LintelBlockViewModel()
        {
        }

        /// <summary>
        /// Конструктор модели представления окна для редактирования свойств заданной перемычки из бетонных брусков
        /// </summary>
        public LintelBlockViewModel(BlockLintel blockLintel)
        {
            _blockType_1 = blockLintel.BlockType_1;
            _blockType_2 = blockLintel.BlockType_2;
            _blockType_3 = blockLintel.BlockType_3;
            _blockType_4 = blockLintel.BlockType_4;
            _blockType_5 = blockLintel.BlockType_5;
            _blockType_6 = blockLintel.BlockType_6;
            _angleSupport = blockLintel.AngleSupport;
            _windowQuarter = blockLintel.WindowQuarter;
            _insulationThickness = blockLintel.InsulationThickness;
            _firstBlockWithQuarter = blockLintel.FirstBlockWithQuarter;
        }


        private string _blockType_1 = "2ПБ 26-4";

        /// <summary>
        /// Тип 1-го блока
        /// </summary>
        public string BlockType_1 { get => _blockType_1; set => Set(ref _blockType_1, value); }


        private string _blockType_2;

        /// <summary>
        /// Тип 2-го блока
        /// </summary>
        public string BlockType_2 { get => _blockType_2; set => Set(ref _blockType_2, value); }


        private string _blockType_3;

        /// <summary>
        /// Тип 3-го блока
        /// </summary>
        public string BlockType_3 { get => _blockType_3; set => Set(ref _blockType_3, value); }


        private string _blockType_4;

        /// <summary>
        /// Тип 4-го блока
        /// </summary>
        public string BlockType_4 { get => _blockType_4; set => Set(ref _blockType_4, value); }


        private string _blockType_5;

        /// <summary>
        /// Тип 5-го блока
        /// </summary>
        public string BlockType_5 { get => _blockType_5; set => Set(ref _blockType_5, value); }


        private string _blockType_6;

        /// <summary>
        /// Тип 6-го блока
        /// </summary>
        public string BlockType_6 { get => _blockType_6; set => Set(ref _blockType_6, value); }


        private double _windowQuarter;
        /// <summary>
        /// Размер четверти
        /// </summary>
        public double WindowQuarter { get => _windowQuarter; set => Set(ref _windowQuarter, value); }


        private double _insulationThickness;
        /// <summary>
        /// Толщина утеплителя
        /// </summary>
        public double InsulationThickness { get => _insulationThickness; set => Set(ref _insulationThickness, value); }


        private bool _firstBlockWithQuarter;
        /// <summary>
        /// 1-й элемент перемычки с четвертью
        /// </summary>
        public bool FirstBlockWithQuarter { get => _firstBlockWithQuarter; set => Set(ref _firstBlockWithQuarter, value); }


        private double _angleSupport;
        /// <summary>
        /// Уголок_Опирание
        /// </summary>
        public double AngleSupport { get => _angleSupport; set => Set(ref _angleSupport, value); }



        public Lintel GetLintel(Guid guid)
        {
            return new BlockLintel(guid)
            {
                BlockType_1 = _blockType_1,
                BlockType_2 = _blockType_2,
                BlockType_3 = _blockType_3,
                BlockType_4 = _blockType_4,
                BlockType_5 = _blockType_5,
                BlockType_6 = _blockType_6,
                AngleSupport = _angleSupport,
                WindowQuarter = _windowQuarter,
                InsulationThickness = _insulationThickness,
                FirstBlockWithQuarter = _firstBlockWithQuarter
            };
        }
    }
}
