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

var map = new Map(maxX * 2, maxY);
foreach (var (x, y, contents) in occupants)
{
    switch (contents)
    {
        case '#':
            _ = new Wall(map, x * 2, y);
            _ = new Wall(map, x * 2 + 1, y);
            break;
        case 'O':
            _ = new WideBox(map, x * 2, y);
            break;
        case '@':
            if (robot is { })
                throw new Exception("Robot is already occupied");
            robot = new Robot(map, x * 2, y);
            break;
    }
}

if (robot is null)
    throw new Exception("Robot is empty");

Console.WriteLine("Initial state:");
Console.WriteLine(map);
Console.WriteLine();

foreach (var robotMove in robotMoves)
{
    if (robot.CanMove(robotMove))
    {
        try
        {
            robot.Move(robotMove);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"Move {robotMove}: ");
            Console.WriteLine(map);
            return -1;
        }
    }

    // Console.WriteLine($"Move {robotMove}:");
    // Console.WriteLine(map);
    // Console.WriteLine();
}

Console.WriteLine();
Console.WriteLine(map);

var sumOfAllGpsCoordinates = map.OfType<IHasGpsCoordinates>().Sum(b => b.Gps);
Console.WriteLine($"Sum of all gps is {sumOfAllGpsCoordinates}");
return 0;

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
    bool CanMove(Direction direction);
    void Move(Direction direction);
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

    public abstract bool CanMove(Direction direction);
    public abstract void Move(Direction direction);
}

internal class MovableOccupantBase(Map map, int x, int y)
    : OccupantBase(map, x, y)
{
    protected virtual bool PerformCanMoveCheckOnMove => true;

    public override bool CanMove(Direction direction)
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

        if (Map[nextX, nextY] is not { } occupant || occupant.CanMove(direction))
        {
            return true;
        }

        return false;
    }

    public override void Move(Direction direction)
    {
        if (PerformCanMoveCheckOnMove && !CanMove(direction))
            throw new InvalidOperationException("Can't move in that direction");

        var (nextX, nextY) = direction switch
        {
            Direction.Left => (X - 1, Y),
            Direction.Up => (X, Y - 1),
            Direction.Right => (X + 1, Y),
            Direction.Down => (X, Y + 1),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };

        Map[nextX, nextY]?.Move(direction);
        Map[X, Y] = null;
        Map[nextX, nextY] = this;
        X = nextX;
        Y = nextY;
    }
}

internal class Wall(Map map, int x, int y) : OccupantBase(map, x, y)
{
    public override bool CanMove(Direction direction)
        => false;

    public override void Move(Direction direction) { }

    public override string ToString()
        => "#";
}

internal interface IHasGpsCoordinates : IOccupant
{
    int Gps => ComputeGpsCoordinates(X, Y);

    static int ComputeGpsCoordinates(int x, int y)
        => y * 100 + x;
}

internal class Box(Map map, int x, int y) : MovableOccupantBase(map, x, y), IHasGpsCoordinates
{
    public override string ToString()
        => "O";
}

internal class WideBox : MovableOccupantBase, IHasGpsCoordinates
{
    private readonly bool _leftPart;
    private readonly WideBox _other;

    public int Gps => _leftPart ? IHasGpsCoordinates.ComputeGpsCoordinates(X, Y) : 0;

    protected override bool PerformCanMoveCheckOnMove => _leftPart;

    private WideBox(Map map, int x, int y, WideBox other)
        : base(map, x, y)
    {
        _other = other;
    }

    public WideBox(Map map, int x, int y)
        : base(map, x, y)
    {
        _leftPart = true;
        _other = new WideBox(map, x + 1, y, this);
    }

    public override bool CanMove(Direction direction)
    {
        if (!_leftPart)
            return _other.CanMove(direction);

        return direction switch
        {
            Direction.Left => BaseCanMove(direction),
            Direction.Right => _other.X != X + 1 || _other.BaseCanMove(direction),
            _ => BaseCanMove(direction) && _other.BaseCanMove(direction)
        };
    }

    public override void Move(Direction direction)
    {
        if (!_leftPart)
        {
            _other.Move(direction);
            return;
        }

        switch (direction)
        {
            case Direction.Right:
                _other.BaseMove(direction);
                BaseMove(direction);
                break;
            default:
                BaseMove(direction);
                _other.BaseMove(direction);
                break;
        }
    }

    public override string ToString()
        => _leftPart ? "[" : "]";

    private bool BaseCanMove(Direction direction)
        => base.CanMove(direction);

    private void BaseMove(Direction direction)
        => base.Move(direction);
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
