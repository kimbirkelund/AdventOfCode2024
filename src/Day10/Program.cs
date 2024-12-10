var input = File.ReadAllLines("input.txt")
                .Select(line => line.Select(c => c == '.' ? -1 : int.Parse($"{c}")).ToArray())
                .ToArray();
Coordinate.Maximum = new Coordinate(input.Length - 1, input[0].Length - 1);

int At(Coordinate coord)
    => input[coord.Line][coord.Column];

int? MaybeAt(Coordinate? coord)
    => coord is { } c ? At(c) : null;

IEnumerable<Coordinate> EnumerateCoordinates(Func<Coordinate, Coordinate?> nextGetter, Coordinate? firstCoordinate = null)
{
    for (Coordinate? current = firstCoordinate ?? Coordinate.Minimum; current != null; current = nextGetter(current!.Value))
        yield return current!.Value;
}

var trailHeads =
    EnumerateCoordinates(c => c.Next())
        .Where(c => At(c) == 0)
        .ToList();

Coordinate[][] FindTrails(Coordinate from, int nextHeight)
{
    if (nextHeight == 10)
        return [[from]];

    List<Coordinate[]> trails = [];

    void AddTrails(Coordinate? next)
    {
        if (next is { } n && At(n) == nextHeight)
        {
            trails.AddRange(FindTrails(n, nextHeight + 1)
                                .Select(t => (Coordinate[]) [from, ..t]));
        }
    }

    AddTrails(from.Left());
    AddTrails(from.Up());
    AddTrails(from.Right());
    AddTrails(from.Down());

    return trails.ToArray();
}

Console.WriteLine($"Trail heads: {string.Join(", ", trailHeads)}");

var trailsByHead =
    trailHeads.Select(h => (head: h, trails: FindTrails(h, 1).ToArray()))
              .ToArray();

foreach (var (trailHead, trails) in trailsByHead)
{
    Console.WriteLine($"{trailHead} (score: {trails.Select(t => t.Last()).Distinct().Count()}, rating: {trails.Length}):");
    Console.WriteLine(string.Join("\n", trails.Select(t => $"  {string.Join(", ", t.Select(p => $"({p.Line}, {p.Column}, {At(p)})"))}")));
}

var sumOfScores = trailsByHead.Select(t => t.trails.Select(t => t.Last()).Distinct().Count()).Sum();
Console.WriteLine($"Sum of scores: {sumOfScores}");

var sumOfRatings = trailsByHead.Select(t => t.trails.Length).Sum();
Console.WriteLine($"Sum of ratings: {sumOfRatings}");

internal readonly record struct Coordinate(int Line, int Column) : IComparable<Coordinate>
{
    public static Coordinate Maximum = new(0, 0);
    public static Coordinate Minimum = new(0, 0);

    public int CompareTo(Coordinate other)
    {
        var lineComparison = Line.CompareTo(other.Line);
        if (lineComparison != 0)
            return lineComparison;
        return Column.CompareTo(other.Column);
    }

    public Coordinate? Down()
    {
        var down = new Coordinate(Line + 1, Column);
        if (down.Line > Maximum.Line)
            return null;

        return down
            ;
    }

    public Coordinate? Left()
    {
        var left = new Coordinate(Line, Column - 1);
        if (left.Column < Minimum.Column)
            return null;

        return left;
    }

    public Coordinate? Next()
    {
        var next = new Coordinate(Line, Column + 1);
        if (next.Column > Maximum.Column)
            next = new Coordinate(Line + 1, Minimum.Column);

        if (next.Line > Maximum.Line)
            return null;

        return next;
    }

    public Coordinate? Previous()
    {
        var previous = new Coordinate(Line, Column - 1);
        if (previous.Column < Minimum.Column)
            previous = new Coordinate(Line - 1, Maximum.Column);

        if (previous.Line < Minimum.Line)
            return null;

        return previous;
    }

    public Coordinate? Right()
    {
        var right = new Coordinate(Line, Column + 1);
        if (right.Column > Maximum.Column)
            return null;

        return right;
    }

    public override string ToString()
    {
        return $"({Line}, {Column})";
    }

    public Coordinate? Up()
    {
        var up = new Coordinate(Line - 1, Column);
        if (up.Line < Minimum.Line)
            return null;

        return up;
    }

    public static Coordinate Bearing(Coordinate a, Coordinate b)
    {
        var bearing = b - a;

        double divisor = Math.Min(Math.Abs(bearing.Line), Math.Abs(bearing.Column));

        while (divisor > 1)
        {
            var newLine = bearing.Line / divisor;
            var newColumn = bearing.Column / divisor;

            if (double.IsInteger(newLine) && double.IsInteger(newColumn))
            {
                bearing = new Coordinate((int)newLine, (int)newColumn);
                divisor = Math.Min(Math.Abs(bearing.Line), Math.Abs(bearing.Column));
            }
            else
                divisor--;
        }

        return bearing;
    }

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
