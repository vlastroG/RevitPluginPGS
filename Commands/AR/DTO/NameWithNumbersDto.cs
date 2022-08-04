using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.DTO
{
    public class NameWithNumbersDto
    {
        public string Name { get; private set; }

        private readonly List<string> _numbers = new List<string>();

        public List<string> Numbers { get { return _numbers; } }

        public NameWithNumbersDto(string name, string number)
        {
            Name = name;
            _numbers.Add(number);
        }

        public void AddNumber(string number)
        {
            if (!_numbers.Contains(number))
            {
                _numbers.Add(number);
            }
        }

        public override string ToString()
        {
            _numbers.Sort();
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            sb.Append(' ');
            sb.Append('(');
            foreach (var number in _numbers)
            {
                sb.Append(number);
                sb.Append(',');
                sb.Append(' ');
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append(')');
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is NameWithNumbersDto)
            {
                return Name == (obj as NameWithNumbersDto).Name;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
