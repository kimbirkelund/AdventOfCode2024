using System.Collections.Immutable;
using System.Text.RegularExpressions;

var regex = new Regex(@"^(Button (?<button>A|B): X\+(?<x>\d+), Y\+(?<y>\d+)|Prize: X=(?<x>\d+), Y=(?<y>\d+))$");
var machines = new List<(Button[] buttons, (int x, int y) prizePosition)>();
foreach (var lines in File.ReadAllLines("input.txt").Chunk(4))
{
    var buttons = new List<Button>();

    foreach (var line in lines)
    {
        if (regex.Match(line) is { Success: true, Groups: var groups })
        {
            if (groups["button"] is { Success: true, Value: { } buttonType })
            {
                buttons.Add(new Button(GetButtonPrice(buttonType), int.Parse(groups["x"].Value), int.Parse(groups["y"].Value)));
            }
            else
            {
                machines.Add((buttons.ToArray(), (int.Parse(groups["x"].Value), int.Parse(groups["y"].Value))));
            }
        }
    }
}

var cheapestPrices = machines.Select(m => FindCheapestPrice(m.buttons, m.prizePosition))
                             .Where(v => v.HasValue)
                             .Cast<int>()
                             .ToArray();

Console.WriteLine($"Prize count: {cheapestPrices.Count()}");
Console.WriteLine($"Cheapest price: {cheapestPrices.Sum()}");

int? FindCheapestPrice(IReadOnlyCollection<Button> buttons, (int x, int y) prizePosition)
{
    var cache = ImmutableDictionary<(int, int), int?>.Empty;

    return FindCheapestPriceImpl(prizePosition, ImmutableDictionary<Button, int>.Empty);

    int? FindCheapestPriceImpl((int x, int y) distanceToPrize, ImmutableDictionary<Button, int> presses)
    {
        if (cache.TryGetValue(distanceToPrize, out var cheapestPrice))
            return cheapestPrice;

        cheapestPrice = buttons.Select(b =>
                                       {
                                           if (presses.TryGetValue(b, out var pressCount) && pressCount >= 100)
                                               return null;
                                           var localPresses = presses.SetItem(b, pressCount + 1);
                                           return b.Press(distanceToPrize) switch
                                           {
                                               (0, 0) => b.Price,
                                               { } newDistanceToPrize => b.Price + FindCheapestPriceImpl(newDistanceToPrize, localPresses),
                                               _ => null
                                           };
                                       })
                               .Min();

        ImmutableInterlocked.TryAdd(ref cache, distanceToPrize, cheapestPrice);
        return cheapestPrice;
    }
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
    public (int x, int y)? Press((int x, int y) target)
    {
        var newX = target.x - X;
        var newY = target.y - Y;

        if (newX < 0 || newY < 0)
            return null;
        return (newX, newY);
    }
}
