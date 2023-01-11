using Autodesk.Revit.DB;
using MS.GUI.ViewModels.Base;
using MS.RevitCommands.AR.Models;
using MS.RevitCommands.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.AR.DTO
{
    /// <summary>
    /// Dto для хранения данных из Revit модели о проеме и его перемычке внутри диалогового окна менеджера перемычек
    /// с последующим парсингом отредактированного Dto в изменения в модели Revit
    /// </summary>
    public class OpeningDto : ViewModelBase, IIdentifiable
    {
        /// <summary>
        /// Перемычка
        /// </summary>
        private Lintel _lintel;

        /// <summary>
        /// Идентификатор
        /// </summary>
        private Guid _guid;

        /// <summary>
        /// Id размещенного в проекте Revit экземпляра семейства перемычки
        /// </summary>
        private readonly int _existLintelId = -1;

        /// <summary>
        /// Удалять ли уже размещенный экземпляр семейства перемычки
        /// </summary>
        private bool _existLintelDeleted = false;

        /// <summary>
        /// Id стены, в которой размещен проем
        /// </summary>
        private readonly int _hostWallId;

        /// <summary>
        /// Id проема
        /// </summary>
        private readonly int _openingId;

        /// <summary>
        /// Конструктор проема
        /// </summary>
        /// <param name="guid">Guid проема</param>
        /// <param name="width">Ширина проема</param>
        /// <param name="height">Высота проема</param>
        /// <param name="wallThick">Толщина стены</param>
        /// <param name="wallHeightOverOpening">Высота стены над проемом</param>
        /// <param name="distanceToRightEnd">Расстояние от правой стороны проема до торца стены в мм</param>
        /// <param name="distanceToLeftEnd">Расстояние от левой стороны проема до торца стены в мм</param>
        /// <param name="wallMaterial">Название материала сердцевины стены</param>
        /// <param name="level">Название уровня, на котором размещен проем</param>
        /// <param name="hostWallId">Id стены, в которой размещен проем</param>
        /// <param name="openingId">Id проема</param>
        /// <param name="location">Точка расположения проема</param>
        /// <param name="lintel">Перемычка проема</param>
        public OpeningDto(
            Guid guid,
            double width,
            double height,
            double wallThick,
            double wallHeightOverOpening,
            double distanceToRightEnd,
            double distanceToLeftEnd,
            string wallMaterial,
            string level,
            int hostWallId,
            int openingId,
            XYZ location,
            Lintel lintel = null)
        {
            Width = width;
            Height = height;
            WallThick = wallThick;
            WallHeightOverOpening = wallHeightOverOpening;
            DistanceToRightEnd = distanceToRightEnd;
            DistanceToLeftEnd = distanceToLeftEnd;
            WallMaterial = wallMaterial;
            Level = level;
            _guid = guid;
            Location = location;
            _lintel = lintel;
            if (!(lintel is null))
            {
                _existLintelId = lintel.ExistLintelId;
            }
            _hostWallId = hostWallId;
            _openingId = openingId;
        }

        /// <summary>
        /// Id стены, в которой размещен проем
        /// </summary>
        public int HostWallId => _hostWallId;

        /// <summary>
        /// Id проема
        /// </summary>
        public int OpeningId => _openingId;

        /// <summary>
        /// Точка расположения элемента, образующего проем
        /// </summary>
        public XYZ Location { get; }

        /// <summary>
        /// Уровень, на котором расположени проем
        /// </summary>
        public string Level { get; }

        /// <summary>
        /// Марка перемычки
        /// </summary>
        [Description("PGS_МаркаПеремычки")]
        public string Mark { get; set; } = string.Empty;

        /// <summary>
        /// Ширина проема в мм
        /// </summary>
        [Description("Ширина проема")]
        public double Width { get; }

        /// <summary>
        /// Высота проема вмм
        /// </summary>
        [Description("Отметка перемычки")]
        public double Height { get; }

        /// <summary>
        /// Отметка от уровня
        /// </summary>
        [Description("Отметка от уровня")]
        public double LevelOffset => 0;

        /// <summary>
        /// Толщина стены в мм
        /// </summary>
        public double WallThick { get; }

        /// <summary>
        /// Высота участка стены над проемом в мм
        /// </summary>
        public double WallHeightOverOpening { get; }

        /// <summary>
        /// Расстояние от правой стороны проема до торца стены в мм
        /// </summary>
        public double DistanceToRightEnd { get; }

        /// <summary>
        /// Расстояние от левой стороны проема до торца стены в мм
        /// </summary>
        public double DistanceToLeftEnd { get; }

        /// <summary>
        /// Строковое представление расстояния от проема до торца стены справа
        /// </summary>
        public string DistanceConditionToRightEnd => DistanceToRightEnd >= 250 ? "≥250" : "<250";

        /// <summary>
        /// Строковое представление расстояния от проема до торца стены слева
        /// </summary>
        public string DistanceConditionToLeftEnd => DistanceToLeftEnd >= 250 ? "≥250" : "<250";

        /// <summary>
        /// Название материала сердцевины стены
        /// </summary>
        public string WallMaterial { get; }

        /// <summary>
        /// Перемычка
        /// </summary>
        public Lintel Lintel
        {
            get => _lintel;
            set
            {
                if ((value is null) && (_existLintelId != -1) && (!_existLintelDeleted))
                {
                    _existLintelDeleted = true;
                }
                Set(ref _lintel, value);
            }
        }

        /// <summary>
        /// Id размещенного в проекте Revit экземпляра семейства перемычки
        /// </summary>
        public int ExistLintelId => _existLintelId;

        /// <summary>
        /// Удалять ли уже размещенный экземпляр семейства перемычки
        /// </summary>
        public bool ExistLintelDeleted => _existLintelDeleted;

        public Guid Guid => _guid;

        /// <summary>
        /// Проемы равны, если все их параметры равны
        /// </summary>
        /// <param name="obj">Объект для сравнения</param>
        /// <returns>True, если проемы имеют одинаковое значение параметров, иначе False</returns>
        public override bool Equals(object obj)
        {
            if ((obj is null) || !(obj is OpeningDto))
            {
                return false;
            }
            OpeningDto dto = obj as OpeningDto;
            return Guid.Equals(dto.Guid);
        }

        /// <summary>
        /// Возвращает HashCode проема
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Width.GetHashCode() +
                    WallThick.GetHashCode() +
                    WallHeightOverOpening.GetHashCode() +
                    DistanceToRightEnd.GetHashCode() +
                    DistanceToLeftEnd.GetHashCode() +
                    WallMaterial.GetHashCode();
        }

        /// <summary>
        /// Возвращает описание проема с перемычкой для записи в лог
        /// </summary>
        /// <returns>Сообщение для логгирования</returns>
        public string ToLongString()
        {
            StringBuilder sb = new StringBuilder();
            if (Lintel is null) return $"Id: {OpeningId}";

            var parValues = Lintel.GetParametersValues();
            foreach (var item in parValues)
            {
                sb.Append(item.Key);
                sb.Append(": ");
                sb.Append(item.Value);
                sb.Append(", ");
            }
            sb.Remove(sb.Length - 2, 2);
            return $"Id: {OpeningId}, Перемычка: {Lintel?.Mark}, {sb}";
        }


        /// <summary>
        /// Возвращает словарь названий параметров перемычки и их значений
        /// </summary>
        /// <returns>Словарь значений атрибутов Description свойств класса и значений этих свойств</returns>
        public virtual Dictionary<string, dynamic> GetParametersValues()
        {
            Dictionary<string, dynamic> parameters = new Dictionary<string, dynamic>();

            PropertyInfo[] properties = GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var description = ((DescriptionAttribute)property
                    .GetCustomAttribute(typeof(DescriptionAttribute)))?.Description;
                if (!(description is null))
                {
                    var value = property.GetValue(this);
                    if (!(value is null))
                    {
                        parameters.Add(description, value);
                    }
                }
            }

            return parameters;
        }
    }
}
