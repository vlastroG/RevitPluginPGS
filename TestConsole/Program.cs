using System.Text.RegularExpressions;

string fileName = @"123<>:poi+-9dfh\/|?*""adfh";

Console.WriteLine(Regex.Replace(fileName, @"[\\<>:/|?*""]", "_"));

