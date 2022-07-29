using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using MS.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MS.Commands.AR.Models
{
    /// <summary>
    /// Квартира
    /// </summary>
    internal class Apartment
    {
        /// <summary>
        /// Список всех помещений квартиры
        /// </summary>
        private readonly List<Room> _rooms = new List<Room>();

        /// <summary>
        /// Коэффициент для перевода квадратных футов в квадратные метры
        /// </summary>
        private readonly double _footSquare = 0.3048 * 0.3048;


        /// <summary>
        /// Переопределенный конструктор квартиры (Number = default). Не использовать.
        /// </summary>
        public Apartment()
        {
            Number = "default";
        }

        /// <summary>
        /// Конструктор квартиры с заданным номером
        /// </summary>
        /// <param name="number">Номер квартиры</param>
        public Apartment(string number)
        {
            Number = number;
        }


        /// <summary>
        /// Номер квартиры
        /// </summary>
        public string Number { get; private set; }

        /// <summary>
        /// Список всех помещений квартиры
        /// </summary>
        public IReadOnlyList<Room> Rooms { get { return _rooms; } }


        /// <summary>
        /// Список всех жилых комнат в квартире
        /// </summary>
        /// <returns>Список жилых комнат в квартире.</returns>
        public IReadOnlyList<Room> GetLivingRooms()
        {
            return Rooms.Where(r => r.get_Parameter(SharedParams.ADSK_TypeOfRoom).AsInteger() == 1).ToList().AsReadOnly();
        }

        /// <summary>
        /// Добавить комнату в квартиру
        /// </summary>
        /// <param name="room">Комната для добавления</param>
        /// <returns>True, если помещение добавлено или false, если нет.</returns>
        public bool AddRoom(Room room)
        {
            if (room == null || room.get_Parameter(SharedParams.ADSK_NumberOfApartment).AsString() != this.Number)
            {
                throw new ArgumentException(room.get_Parameter(BuiltInParameter.ID_PARAM).ToString());
            }
            else
            {
                _rooms.Add(room);
                return true;
            }
        }

        /// <summary>
        /// Вычисляет жилую площидь квартиры с заданой точностью (для квадратных метров).
        /// </summary>
        /// <param name="round_decimals">Количество знаков после запятой</param>
        /// <returns>Жилая площадь квартиры в футах для назначения помещениям.</returns>
        public double GetAreaLiving(int round_decimals)
        {
            double area = 0;
            var rooms = this.GetLivingRooms();
            foreach (var room in rooms)
            {
                area += Math.Round(room.get_Parameter(BuiltInParameter.ROOM_AREA)
                                       .AsDouble() * _footSquare, round_decimals, MidpointRounding.AwayFromZero) / _footSquare;
            }
            return area;
        }

        /// <summary>
        /// Вычисляет отапливаемую площадь квартиры с заданной точностью (для квадратных метров).
        /// </summary>
        /// <param name="round_decimals">Количество знаков после запятой</param>
        /// <returns>Отапливаемая площадь квартиры в футах для назначения помещениям.</returns>
        public double GetAreaHeated(int round_decimals)
        {
            double area = 0;
            var rooms = _rooms.Where(r => r.get_Parameter(SharedParams.ADSK_TypeOfRoom).AsInteger() < 3);
            foreach (var room in rooms)
            {
                area += Math.Round(room.get_Parameter(BuiltInParameter.ROOM_AREA)
                                       .AsDouble() * _footSquare, round_decimals, MidpointRounding.AwayFromZero) / _footSquare;
            }
            return area;
        }

        /// <summary>
        /// Вычисляет полную приведенную площадь квартиры с заданной точностью (для квадратных метров).
        /// </summary>
        /// <param name="nameof_param_roomtype">Название параметра "тип комнаты"</param>
        /// <param name="nameof_param_coeff">Название параметра "коэффициент площади"</param>
        /// <param name="round_decimals">Количество знаков после запятой</param>
        /// <returns>Приведенная площадь квартиры в футах для назначения помещениям.</returns>
        public double GetAreaTotalCoeff(int round_decimals)
        {
            double area = 0;
            var rooms = _rooms;
            foreach (var room in rooms)
            {
                if (room.get_Parameter(SharedParams.ADSK_TypeOfRoom).AsInteger() >= 3 && room.get_Parameter(SharedParams.ADSK_TypeOfRoom).AsInteger() <= 5)
                {
                    area += Math.Round(
                                       Math.Round(room.get_Parameter(BuiltInParameter.ROOM_AREA)
                                                      .AsDouble() * _footSquare, round_decimals, MidpointRounding.AwayFromZero)
                                                  * room.get_Parameter(SharedParams.ADSK_CoeffOfArea).AsDouble(),
                                       round_decimals, MidpointRounding.AwayFromZero) / _footSquare;
                }
                else
                {
                    area += Math.Round(room.get_Parameter(BuiltInParameter.ROOM_AREA)
                                           .AsDouble() * _footSquare, round_decimals, MidpointRounding.AwayFromZero) / _footSquare;
                }
            }
            return area;
        }

        /// <summary>
        /// Переопределенный метод возвращает true, если номера квартир одинаковые
        /// </summary>
        /// <param name="obj">Квартира для сравнения</param>
        /// <returns>true, если номера одинаковые, иначе false</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Apartment);
        }

        /// <summary>
        /// Реализация переопределенного метода Equals
        /// </summary>
        /// <param name="other">Вторая квартира</param>
        /// <returns>true, если номера одинаковые, иначе false</returns>
        private bool Equals(Apartment other)
        {
            return other != null &&
                Number == other.Number;
        }

        /// <summary>
        /// Переопределенный метод
        /// </summary>
        /// <returns>Хэш-код по номеру квартиры и количеству помещений</returns>
        public override int GetHashCode()
        {
            return new { Number, _rooms.Count }.GetHashCode();
        }
    }
}
