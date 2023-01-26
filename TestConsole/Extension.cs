using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    internal static class Extension
    {
        public static string ToString<TKey, TValue>(this Dictionary<TKey, TValue> dict) 
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('{');
            foreach (var item in dict)
            {
                sb.Append(item.Key);
                sb.Append(": ");
                sb.Append(item.Value);
                sb.Append(", ");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
