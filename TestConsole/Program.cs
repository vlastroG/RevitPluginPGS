string test = "к1 18";
var t = test.Split(',');
var filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Autodesk\Revit\Addins\2022\MS.dll");
bool exists = File.Exists(filepath);
var b = 9;