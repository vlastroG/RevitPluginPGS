using System;
using System.Text;

namespace MS.Commands.AR.DTO
{
    public class MarkWithCountDto
    {
        /// <summary>
        /// Марка
        /// </summary>
        public string Mark { get; private set; }

        /// <summary>
        /// Количество марок
        /// </summary>
        public int MarkCount { get; set; }

        public MarkWithCountDto(string Mark)
        {
            this.Mark = Mark;
            MarkCount = 1;
        }

        /// <summary>
        /// Строковое представление марки с количеством
        /// </summary>
        /// <returns>"{Mark} ({MarkCount})"</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Mark);
            sb.Append(" (");
            sb.Append(MarkCount);
            sb.Append(")");
            return sb.ToString();
        }

        /// <summary>
        /// Равенство объектов
        /// </summary>
        /// <param name="obj">Объект для сравнения</param>
        /// <returns>Возвращает <see cref="Boolean.TrueString">True</see>, 
        /// если оба объекта являются типами класса <see cref="MarkWithCountDto"></see>
        /// и значения их параметров <see cref="MarkWithCountDto.Mark">Mark</see> равны,
        /// иначе <see cref="Boolean.FalseString">False</see>.</returns>
        public override bool Equals(object obj)
        {
            if (obj != null && obj is MarkWithCountDto)
            {
                return Mark == (obj as MarkWithCountDto).Mark;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Получение хэш кода
        /// </summary>
        /// <returns><c>Mark.GetHashCode()</c></returns>
        public override int GetHashCode()
        {
            return Mark.GetHashCode();
        }
    }
}
