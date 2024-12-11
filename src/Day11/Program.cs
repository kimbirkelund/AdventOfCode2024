using System.Collections.Immutable;
using System.Diagnostics;

var cache = ImmutableDictionary<(long value, int remainingBlinks), long>.Empty;
long cacheHits = 0;

var input = File.ReadAllText("input.txt").Trim();


var initialStones =
    input.Split(" ")
         .Select(long.Parse)
         .ToList();

var blinkCount = 75;
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
Console.WriteLine($"Cache hits: {cacheHits}");
Console.WriteLine($"Total Stone Count: {totalStoneCount}");

long Blink(long value, int numberOfBlinks)
{
    if (cache.TryGetValue((value, numberOfBlinks), out var stoneCount))
    {
        Interlocked.Increment(ref cacheHits);
        return stoneCount;
    }

    long result;

    if (numberOfBlinks == 0)
        result = 1;
    else if (value is 0)
        result = Blink(1, numberOfBlinks - 1);
    else if (value.ToString() is { } strValue && strValue.Length % 2 == 0)
    {
        var leftValue = long.Parse(strValue[..(strValue.Length / 2)]);
        var rightValue = long.Parse(strValue[(strValue.Length / 2)..]);

        result = Blink(leftValue, numberOfBlinks - 1)
                 + Blink(rightValue, numberOfBlinks - 1);
    }
    else
        result = Blink(value * 2024, numberOfBlinks - 1);

    ImmutableInterlocked.TryAdd(ref cache, (value, numberOfBlinks), result);

    return result;
}
