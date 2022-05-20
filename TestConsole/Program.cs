

using System.Text.RegularExpressions;

Regex regex = new Regex(@"\(.*\d+\.\d+\)$");
string test = "Этаж 1 (+5.000)";


Console.WriteLine(regex.IsMatch(test));
Console.ReadLine();
