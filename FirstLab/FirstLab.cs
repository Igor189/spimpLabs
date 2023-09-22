// See https://aka.ms/new-console-template for more information

var numbers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
var partCount = 3;

var result = numbers
    .Select((number, i) => new { Index = i, Value = number })
    .GroupBy(number => number.Index / partCount)
    .Select(partList => partList.Sum(number => number.Value));

foreach (var num in result)
{
    Console.WriteLine(num);
}