// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;

var input = File.ReadAllText("input.txt");

var regex = new Regex(@"(?<do>do\(\))|(?<dont>don't\(\))|mul\((?<op1>\d+),(?<op2>\d+)\)");

var nextMultEnabled = true;
var result = 0;

foreach (Match match in regex.Matches(input))
{
    if (match.Groups["do"].Success)
        nextMultEnabled = true;
    else if (match.Groups["dont"].Success)
        nextMultEnabled = false;
    else
    {
        if (nextMultEnabled)
            result += int.Parse(match.Groups["op1"].Value) * int.Parse(match.Groups["op2"].Value);
    }
}

Console.WriteLine(result);
