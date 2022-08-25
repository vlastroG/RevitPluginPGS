using System.Collections.Generic;
using System.Text;

namespace MS.Commands.AR.DTO
{
    public class MultiNameRoomsDto
    {
        private readonly List<NameWithNumbersDto> _namesWithNumbers = new List<NameWithNumbersDto>();

        public IReadOnlyCollection<NameWithNumbersDto> NamesWithNumbers { get { return _namesWithNumbers; } }

        public MultiNameRoomsDto(NameWithNumbersDto nameWithNumber)
        {
            _namesWithNumbers.Add(nameWithNumber);
        }

        public void AddNameWithNumber(string name, string number)
        {
            NameWithNumbersDto dto = new NameWithNumbersDto(name, number);

            if (_namesWithNumbers.Contains(dto))
            {
                _namesWithNumbers.Find(m => m.Equals(dto)).AddNumber(number);
            }
            else
            {
                _namesWithNumbers.Add(dto);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            _namesWithNumbers.Sort(delegate (NameWithNumbersDto name1, NameWithNumbersDto name2)
            {
                return name1.Name.CompareTo(name2.Name);
            });
            foreach (var nameWithNumbers in _namesWithNumbers)
            {
                sb.Append(nameWithNumbers.ToString());
                sb.Append(',');
                sb.Append(' ');
            }
            sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }
    }
}
