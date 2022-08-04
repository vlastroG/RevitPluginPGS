
using static MS.Utilites.UserInput;
int test = 0;
try
{
    test = GetIntFromUser("Ввод числа", "Введите целое число");
}
catch (System.OperationCanceledException)
{
    Console.WriteLine("Operation Cancelled");
}

var test2 = test * 2;

