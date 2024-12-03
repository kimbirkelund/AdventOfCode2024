// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;

var input = File.ReadAllText("input.txt");

var regex = new Regex(@"mul\((?<op1>\d+),(?<op2>\d+)\)");

var result = regex.Matches(input)
                  .Select(m => int.Parse(m.Groups["op1"].Value) * int.Parse(m.Groups["op2"].Value))
                  .Sum();

Console.WriteLine(result);
