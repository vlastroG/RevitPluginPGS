

using System.Globalization;
using System.Text.RegularExpressions;

Regex regex = new Regex(@"^[А-Я][а-я]+\s*[\+\-]*\d*\s*\([\+\-]*\d+[\.\,]+\d+\)$");
string test = "Па -1 (+5.123)";
string test1 = "Этаж +1 (+5.000)";
string test2 = "Этаж 1 (5,954)";
string test3 = "Этаж 1 (+.000)";
string test4 = "Этаж 13232 (+5.0)";
string test5 = "Этаж 1(+5.000)";
string test6 = "Этаж1 (+5.123)";
string test7 = "Этупр54ен54нж 1 (5.00)";
string test8 = "Кровля (+15.000)";
string test9 = "Этаж 1";
string test10 = "Этаж 1 (+125.000)";

string pattern = @"\d+[\.\,]+\d+";
string test_height = Regex.Match(test, pattern).Value;

double height = 0;
try
{
    height = Double.Parse(test_height, CultureInfo.InvariantCulture);
}
catch (FormatException)
{
    throw new FormatException(nameof(test_height));
}



Console.WriteLine(test + " - " + regex.IsMatch(test));
Console.WriteLine(test1 + " - " + regex.IsMatch(test1));
Console.WriteLine(test2 + " - " + regex.IsMatch(test2));
Console.WriteLine(test3 + " - " + regex.IsMatch(test3));
Console.WriteLine(test4 + " - " + regex.IsMatch(test4));
Console.WriteLine(test5 + " - " + regex.IsMatch(test5));
Console.WriteLine(test6 + " - " + regex.IsMatch(test6));
Console.WriteLine(test7 + " - " + regex.IsMatch(test7));
Console.WriteLine(test8 + " - " + regex.IsMatch(test8));
Console.WriteLine(test9 + " - " + regex.IsMatch(test9));
Console.WriteLine(test10 + " - " + regex.IsMatch(test10));
Console.WriteLine(test + " - " + test_height + " - " + height);
Console.ReadLine();
