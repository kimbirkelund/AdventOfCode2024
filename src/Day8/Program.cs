var input = File.ReadAllLines("input.txt");

var allAntennas =
    Enumerable.Range(0, input.Length)
              .SelectMany(i => Enumerable.Range(0, input[i].Length)
                                         .Select(j => new Coordinate(i, j)))
              .Select(c => (coord: c, value: input[c.Line][c.Column]))
              .Where(t => t.value != '.')
              .ToLookup(t => t.value, t => t.coord);

static IEnumerable<( Coordinate first, Coordinate second)> GetAllPairs(IEnumerable<Coordinate> antennas)
{
    var localAntennas = antennas as Coordinate[] ?? antennas.ToArray();
    foreach (var first in localAntennas)
    {
        foreach (var second in localAntennas.Where(s => s != first))
        {
            if (first < second)
                yield return (first, second);
            else
                yield return (second, first);
        }
    }
}

var mininum = new Coordinate(0, 0);
var maxinum = new Coordinate(input.Length - 1, input[^1].Length - 1);

bool InBounds(Coordinate c)
{
    return c.Line >= 0
           && c.Line < input.Length
           && c.Column >= 0
           && c.Column < input[^1].Length;
}

IEnumerable<(char frequency, Coordinate coord, Coordinate antenna1, Coordinate antenna2)> GetAllAntiNodes(ILookup<char, Coordinate> antennas)
{
    foreach (var g in antennas)
    {
        Console.WriteLine($"{g.Key}: {string.Join(", ", g)}");

        var allPairs = GetAllPairs(g).Distinct().ToList();
        Console.WriteLine($"  pairs: {string.Join(", ", allPairs)}");

        foreach (var (first, second) in allPairs)
        {
            var dist = first - second;

            var firstAntiNode = first + dist;
            var secondAntiNode = second - dist;

            if (InBounds(firstAntiNode))
                yield return (g.Key, firstAntiNode, first, second);

            if (InBounds(secondAntiNode))
                yield return (g.Key, secondAntiNode, first, second);
        }
    }
}

var allAntiNodes = GetAllAntiNodes(allAntennas).ToList();

Console.WriteLine($"Anti nodes, {allAntiNodes.Count} in total:");
Console.WriteLine(string.Join("\n", allAntiNodes.Select(n => $"  {n}")));

var allUniqueAntiNodes =
    allAntiNodes.DistinctBy(t => t.coord)
                .Select(t => t.coord)
                .Order()
                .ToList();
Console.WriteLine($"Anti nodes, {allUniqueAntiNodes.Count} in total:");
Console.WriteLine($"  {string.Join(", ", allUniqueAntiNodes)}");

internal readonly record struct Coordinate(int Line, int Column) : IComparable<Coordinate>
{
    public int CompareTo(Coordinate other)
    {
        var lineComparison = Line.CompareTo(other.Line);
        if (lineComparison != 0)
            return lineComparison;
        return Column.CompareTo(other.Column);
    }

    public override string ToString()
    {
        return $"({Line}, {Column})";
    }

    public static Coordinate Abs(Coordinate second)
        => new(Math.Abs(second.Line), Math.Abs(second.Column));

    public static Coordinate operator +(Coordinate a, Coordinate b)
        => new(a.Line + b.Line, a.Column + b.Column);

    public static bool operator >(Coordinate left, Coordinate right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Coordinate left, Coordinate right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static bool operator <(Coordinate left, Coordinate right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Coordinate left, Coordinate right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static Coordinate operator -(Coordinate a, Coordinate b)
        => new(a.Line - b.Line, a.Column - b.Column);
}
