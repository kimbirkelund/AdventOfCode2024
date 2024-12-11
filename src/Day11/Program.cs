using System.Collections;

var input = File.ReadAllText("input.txt").Trim();

var leftMostStone =
    input.Split(" ")
         .Select(long.Parse).Reverse()
         .Aggregate((Stone?)null,
                    (a, b) => new Stone(b, a))
    ?? throw new Exception("Input is empty.");

Blink(25);

PrintStones(leftMostStone);

var totalStoneCount = leftMostStone.Count();

Console.WriteLine($"Total Stone Count: {totalStoneCount}");

void PrintStones(Stone stone)
{
    Console.Write($"{stone.Value}");
    var current = stone.Right;
    while (current is { Value: var value, Right: var right })
    {
        Console.Write($" {value}");
        current = right;
    }

    Console.WriteLine();
}

void Blink(int count)
{
    for (var i = 0; i < count; i++)
    {
        leftMostStone = Stone.EvolveAll(leftMostStone);
    }
}

internal class Stone(long value, Stone? right) : IEnumerable<Stone>
{
    public long Value { get; } = value;
    public Stone? Right { get; private set; } = right;

    public IEnumerator<Stone> GetEnumerator()
    {
        var current = this;
        while (current is not null)
        {
            yield return current;
            current = current.Right;
        }
    }

    private Stone EvolveSingle()
    {
        if (Value is 0)
            return new Stone(1, Right);

        if (Value.ToString() is { } strValue && strValue.Length % 2 == 0)
        {
            var leftValue = long.Parse(strValue[..(strValue.Length / 2)]);
            var rightValue = long.Parse(strValue[(strValue.Length / 2)..]);
            return new Stone(leftValue, new Stone(rightValue, Right));
        }

        return new Stone(Value * 2024, Right);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static Stone EvolveAll(Stone leftMost)
    {
        var stack = new Stack<Stone>(leftMost);

        while (true)
        {
            var current = stack.Pop();
            current = current.EvolveSingle();

            if (stack.TryPeek(out var left))
                left.Right = current;
            else
                return current;
        }
    }
}
