using System.Collections;
using System.Text;

var input = File.ReadAllLines("input.txt");

var occupants = new List<(int x, int y, char contents)>();
Robot? robot = null;
var robotMoves = new List<Direction>();

var maxX = -1;
var maxY = 0;
var readingMap = true;
foreach (var line in input)
{
    if (line is "")
    {
        readingMap = false;
        continue;
    }

    if (readingMap)
    {
        occupants.AddRange(line.Select((c, x) => (x, y: maxY, c)));
        maxY++;
        maxX = Math.Max(maxX, line.Length);
    }
    else
    {
        robotMoves.AddRange(line.Select(c => c switch
        {
            '<' => Direction.Left,
            '^' => Direction.Up,
            '>' => Direction.Right,
            'v' => Direction.Down,
            _ => throw new ArgumentOutOfRangeException(nameof(c), c, null)
        }));
    }
}

var map = new Map(maxX, maxY);
foreach (var (x, y, contents) in occupants)
{
    switch (contents)
    {
        case '#':
            _ = new Wall(map, x, y);
            break;
        case 'O':
            _ = new Box(map, x, y);
            break;
        case '@':
            if (robot is { })
                throw new Exception("Robot is already occupied");
            robot = new Robot(map, x, y);
            break;
    }
}

if (robot is null)
    throw new Exception("Robot is empty");

Console.WriteLine(map);

foreach (var robotMove in robotMoves)
    robot.Move(robotMove);

Console.WriteLine();
Console.WriteLine(map);

var sumOfAllGpsCoordinates = map.OfType<Box>().Sum(b => b.Gps);
Console.WriteLine($"Sum of all gps is {sumOfAllGpsCoordinates}");


internal enum Direction
{
    Left,
    Up,
    Right,
    Down
}

internal interface IOccupant
{
    int X { get; }
    int Y { get; }
    bool Move(Direction direction);
}

internal abstract class OccupantBase : IOccupant
{
    public int X { get; protected set; }
    public int Y { get; protected set; }

    protected Map Map { get; }

    protected OccupantBase(Map map, int x, int y)
    {
        X = x;
        Y = y;
        Map = map;
        map[x, y] = this;
    }

    public abstract bool Move(Direction direction);
}

internal class MovableOccupantBase(Map map, int x, int y)
    : OccupantBase(map, x, y)
{
    public override bool Move(Direction direction)
    {
        var (nextX, nextY) = direction switch
        {
            Direction.Left => (X - 1, Y),
            Direction.Up => (X, Y - 1),
            Direction.Right => (X + 1, Y),
            Direction.Down => (X, Y + 1),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };

        if (!Map.IsValidPosition(nextX, nextY))
            return false;

        if (Map[nextX, nextY] is not { } occupant || occupant.Move(direction))
        {
            Map[X, Y] = null;
            Map[nextX, nextY] = this;
            X = nextX;
            Y = nextY;
            return true;
        }

        return false;
    }
}

internal class Wall(Map map, int x, int y) : OccupantBase(map, x, y)
{
    public override bool Move(Direction direction)
        => false;

    public override string ToString()
        => "#";
}

internal class Box(Map map, int x, int y) : MovableOccupantBase(map, x, y)
{
    public int Gps => Y * 100 + X;

    public override string ToString()
        => "O";
}

internal class Robot(Map map, int x, int y) : MovableOccupantBase(map, x, y)
{
    public override string ToString()
        => "@";
}

internal class Map(int width, int height) : IEnumerable<IOccupant>
{
    private readonly IOccupant?[,] _map = new IOccupant?[width, height];

    public IOccupant? this[int x, int y]
    {
        get => _map[x, y];
        set => _map[x, y] = value;
    }

    public IEnumerator<IOccupant> GetEnumerator()
    {
        for (var y = 0; y < _map.GetLength(1); y++)

        for (var x = 0; x < _map.GetLength(0); x++)
        {
            if (this[x, y] is { } occupant)
                yield return occupant;
        }
    }

    public bool IsValidPosition(int x, int y)
        => x >= 0 && x < _map.GetLength(0)
                  && y >= 0 && y < _map.GetLength(1);

    public override string ToString()
    {
        var sb = new StringBuilder();

        for (var y = 0; y < _map.GetLength(1); y++)
        {
            for (var x = 0; x < _map.GetLength(0); x++)
                sb.Append($"{_map[x, y]?.ToString() ?? "."}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
