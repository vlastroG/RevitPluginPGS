using System.Collections.Generic;
using System.Text;

namespace MS.RevitCommands.AR.DTO
{
    public class LintelMultiMarkDto
    {
        /// <summary>
        /// Список марок с количеством для перемычки
        /// </summary>
        private readonly List<MarkWithCountDto> _marks = new List<MarkWithCountDto>();

        /// <summary>
        /// Коллекция марок с количеством для данной перемычки
        /// </summary>
        public IReadOnlyCollection<MarkWithCountDto> Marks
        {
            get
            {
                return _marks;
            }
        }

        public LintelMultiMarkDto(MarkWithCountDto markDto)
        {
            _marks.Add(markDto);
        }

        /// <summary>
        /// Добавить марку в список марок перемычки
        /// </summary>
        /// <param name="markDto">Марка с количеством</param>
        public void AddMark(MarkWithCountDto markDto)
        {
            if (markDto != null)
            {
                if (_marks.Contains(markDto))
                {
                    _marks.Find(m => m.Equals(markDto)).MarkCount++;
                }
                else
                {
                    _marks.Add(markDto);
                }
            }
        }

        /// <summary>
        /// Строковое представление списка марок перемычки с количеством элементов каждой марки
        /// </summary>
        /// <returns>
        /// Строковые представления <see cref="MarkWithCountDto">Марок с количеством</see> через запятую с пробелом.
        /// </returns>
        public override string ToString()
        {
            _marks.Sort(delegate (MarkWithCountDto mark1, MarkWithCountDto mark2)
            {
                return mark1.Mark.CompareTo(mark2.Mark);
            });
            StringBuilder sb = new StringBuilder();
            foreach (MarkWithCountDto mark in _marks)
            {
                sb.Append(mark.ToString());
                sb.Append(',');
                sb.Append(' ');
            }
            sb.Remove(sb.Length - 2, 2);

            return sb.ToString();
        }
    }
}
