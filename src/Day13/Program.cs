using System.Text.RegularExpressions;

var regex = new Regex(@"^(Button (?<button>A|B): X\+(?<x>\d+), Y\+(?<y>\d+)|Prize: X=(?<x>\d+), Y=(?<y>\d+))$");
var machines = new List<(Button[] buttons, (long x, long y) prizePosition)>();

var offset = 10000000000000;
foreach (var lines in File.ReadAllLines("input.txt").Chunk(4))
{
    var buttons = new List<Button>();

    foreach (var line in lines)
    {
        if (regex.Match(line) is { Success: true, Groups: var groups })
        {
            if (groups["button"] is { Success: true, Value: { } buttonType })
            {
                buttons.Add(new Button(GetButtonPrice(buttonType),
                                       int.Parse(groups["x"].Value),
                                       int.Parse(groups["y"].Value)));
            }
            else
            {
                machines.Add((buttons.ToArray(),
                                 (
                                     long.Parse(groups["x"].Value) + offset,
                                     long.Parse(groups["y"].Value) + offset)));
            }
        }
    }
}

var cheapestPrices = machines.Select(m => FindCheapestPrice(m.buttons, m.prizePosition))
                             .Where(v => v.HasValue)
                             .Cast<long>()
                             .ToArray();

Console.WriteLine($"Prize count: {cheapestPrices.Count()}");
Console.WriteLine($"Cheapest price: {cheapestPrices.Sum()}");

long? FindCheapestPrice(IReadOnlyList<Button> buttons, (long x, long y) prizePosition)
{
    var A = buttons[0];
    var B = buttons[1];

    var b = (prizePosition.y * A.X - prizePosition.x * A.Y) / (double)(A.X * B.Y - B.X * A.Y);
    var a = (prizePosition.x - b * B.X) / A.X;

    if (!double.IsInteger(a) || !double.IsInteger(b))
        return null;

    var bestPrice = a * A.Price + b * B.Price;

    return (long)bestPrice;
}

int GetButtonPrice(string buttonType)
    => buttonType switch
    {
        "A" => 3,
        "B" => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(buttonType), buttonType, null)
    };

internal record Button(int Price, int X, int Y)
{
    public (long x, long y)? Press((long x, long y) target)
    {
        var newX = target.x - X;
        var newY = target.y - Y;

        if (newX < 0 || newY < 0)
            return null;
        return (newX, newY);
    }
}
