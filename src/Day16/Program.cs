using System.Collections.Immutable;
using System.Text;

var input = File.ReadAllLines("input.txt");

var inputMaze = new Maze(input);
Console.WriteLine(inputMaze);

int FindCheapestPath(Maze maze)
{
    var seen = ImmutableHashSet<(int, int, Direction)>.Empty;
    var workingSet = new PriorityQueue<(int x, int y, Direction direction, int price), int>();
    workingSet.Enqueue((maze.Start.x, maze.Start.y, Direction.East, 0), 0);

    var bestAt = ImmutableDictionary<(int x, int y, Direction direction), int>.Empty;

    Console.Clear();
    var steps = 0;
    while (workingSet.Count > 0)
    {
        steps++;
        var (x, y, direction, price) = workingSet.Dequeue();

        Console.SetCursorPosition(0, 0);
        Console.Write(maze.ToString(workingSet.UnorderedItems.Select(t => (t.Element.x, t.Element.y)).ToImmutableHashSet()));
        // Console.WriteLine();
        Console.Write($"Steps: {steps}, current: {x},{y}, {direction}, {price}, working set size: {workingSet.Count}           ");
        // Console.WriteLine("Working set:");
        // foreach (var p in workingSet.Take(60))
        //     Console.WriteLine($"  {p.x},{p.y}, {p.direction}, {p.price}         ");
        // Console.WriteLine("Press enter to continue...");
        // Console.ReadLine();

        if (seen.Contains((x, y, direction)))
            continue;
        ImmutableInterlocked.Update(ref seen, s => s.Add((x, y, direction)));

        switch (maze[x, y])
        {
            case Tile.End:
                Console.WriteLine();
                return price;

            case Tile.Wall:
                continue;

            default:
                var (nextX, nextY, nextDirectionClockwise, nextDirectionCounterClockwise) = direction switch
                {
                    Direction.West => (x - 1, y, Direction.North, Direction.South),
                    Direction.North => (x, y - 1, Direction.East, Direction.West),
                    Direction.East => (x + 1, y, Direction.South, Direction.North),
                    Direction.South => (x, y + 1, Direction.West, Direction.East),
                    _ => throw new ArgumentOutOfRangeException()
                };

                workingSet.Enqueue((nextX, nextY, direction, price + 1), price + 1);
                workingSet.Enqueue((x, y, nextDirectionClockwise, price + 1000), price + 1000);
                workingSet.Enqueue((x, y, nextDirectionCounterClockwise, price + 1000), price + 1000);

                break;
        }
    }

    throw new Exception("Cannot find path");
}

var cheapestPath = FindCheapestPath(inputMaze);
Console.WriteLine($"Cheapest path: {cheapestPath}");

internal enum Direction
{
    West,
    North,
    East,
    South
}

internal enum Tile
{
    Wall,
    Empty,
    Start,
    End
}

internal class Maze(int width, int height)
{
    private readonly Tile[,] _maze = new Tile[width, height];

    public (int x, int y) End { get; }
    public (int x, int y) Start { get; }

    public Tile this[int x, int y]
    {
        get => _maze[x, y];
        set => _maze[x, y] = value;
    }

    public Maze(string[] input)
        : this(input[0].Length, input.Length)
    {
        for (var y = 0; y < input.Length; y++)
        for (var x = 0; x < input[y].Length; x++)
        {
            var tile = input[y][x] switch
            {
                '#' => Tile.Wall,
                '.' => Tile.Empty,
                'S' => Tile.Start,
                'E' => Tile.End,
                _ => throw new ArgumentOutOfRangeException()
            };
            this[x, y] = tile;

            if (tile == Tile.Start)
                Start = (x, y);
            else if (tile == Tile.End)
                End = (x, y);
        }
    }

    public override string ToString()
        => ToString([]);

    public string ToString(ImmutableHashSet<(int x, int y)> reindeerPositions)
    {
        var sb = new StringBuilder();

        for (var y = 0; y < _maze.GetLength(1); y++)
        {
            for (var x = 0; x < _maze.GetLength(0); x++)
                sb.Append($"{TileToString(x, y, _maze[x, y])}");
            sb.AppendLine();
        }

        return sb.ToString();

        string TileToString(int x, int y, Tile tile)
            => tile switch
            {
                Tile.Wall => "#",
                Tile.Empty => reindeerPositions.Contains((x, y)) ? "R" : ".",
                Tile.Start => "S",
                Tile.End => "E",
                _ => throw new ArgumentOutOfRangeException(nameof(tile), tile, null)
            };
    }
}
