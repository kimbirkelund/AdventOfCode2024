using System.Collections.Immutable;
using System.Diagnostics;

var input = File.ReadAllText("input.txt").Trim();

var initialStones =
    input.Split(" ")
         .Select(long.Parse)
         .ToList();

var blinkCount = 25;
var mutex = new object();

var swTotal = Stopwatch.StartNew();
var totalStoneCount =
    initialStones
        .AsParallel()
        .Select(v =>
                {
                    lock (mutex)
                        Console.WriteLine($"Blinking {v} {blinkCount} times...");

                    var sw = Stopwatch.StartNew();

                    var blink = Blink(v, blinkCount);

                    sw.Stop();
                    lock (mutex)
                        Console.WriteLine($"Blinking {v} {blinkCount} times took {sw}. {sw.Elapsed}");

                    return blink;
                })
        .Sum();
swTotal.Stop();
Console.WriteLine($"Blinking {blinkCount} times took {swTotal.Elapsed}.");

Console.WriteLine($"Total Stone Count: {totalStoneCount}");

var cache = ImmutableDictionary<(long value, int remainingBlinks), long>.Empty;

long Blink(long value, int numberOfBlinks)
{
    if (numberOfBlinks == 0)
        return 1;
    if (value is 0)
        return Blink(1, numberOfBlinks - 1);

    if (value.ToString() is { } strValue && strValue.Length % 2 == 0)
    {
        var leftValue = long.Parse(strValue[..(strValue.Length / 2)]);
        var rightValue = long.Parse(strValue[(strValue.Length / 2)..]);

        return Blink(leftValue, numberOfBlinks - 1)
               + Blink(rightValue, numberOfBlinks - 1);
    }

    return Blink(value * 2024, numberOfBlinks - 1);
}
