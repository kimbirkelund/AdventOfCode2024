using System.Collections.Immutable;
using System.Text;

var input = File.ReadAllLines("input.txt");

var inputMaze = new Maze(input);
Console.WriteLine(inputMaze);

(int price, ImmutableList<ImmutableList<(int x, int y)>> routes) FindAllCheapestPaths(Maze maze)
{
    var seen = ImmutableHashSet<(int, int, Direction)>.Empty;
    var workingSet = new PriorityQueue<(int x, int y, Direction direction, int price, ImmutableList<(int x, int y)> previous), int>();
    workingSet.Enqueue((maze.Start.x, maze.Start.y, Direction.East, 0, []), 0);
    workingSet.Enqueue((maze.Start.x, maze.Start.y, Direction.North, 1000, []), 1000);
    workingSet.Enqueue((maze.Start.x, maze.Start.y, Direction.South, 1000, []), 1000);

    var bestAtEndSoFar = (price: int.MaxValue, routes: ImmutableList<ImmutableList<(int x, int y)>>.Empty);

    var bestAt = ImmutableDictionary<(int x, int y, Direction direction), int>.Empty;

    Console.Clear();
    var steps = 0;
    while (workingSet.Count > 0)
    {
        steps++;
        var (x, y, direction, price, previous) = workingSet.Dequeue();

        Console.SetCursorPosition(0, 0);
        Console.Write(maze.ToString(workingSet.UnorderedItems.Select(t => (t.Element.x, t.Element.y)).ToImmutableHashSet()));
        // Console.WriteLine();
        Console.WriteLine($"Steps: {steps}, current: {x},{y}, {direction}, {price}");
        Console.WriteLine($"Working set size: {workingSet.Count}           ");
        Console.WriteLine($"Best so far: {bestAtEndSoFar.routes.Count} routes at {bestAtEndSoFar.price}");
        // Console.WriteLine("Working set:");
        // foreach (var p in workingSet.Take(60))
        //     Console.WriteLine($"  {p.x},{p.y}, {p.direction}, {p.price}         ");
        // Console.WriteLine("Press enter to continue...");
        // Console.ReadLine();

        if (price > bestAtEndSoFar.price)
            continue;

        if (ImmutableInterlocked.AddOrUpdate(ref bestAt,
                                             (x, y, direction),
                                             price,
                                             (_, c) => Math.Min(c, price)) < price)
            continue;

        switch (maze[x, y])
        {
            case Tile.End:
                if (price == bestAtEndSoFar.price)
                    bestAtEndSoFar = (bestAtEndSoFar.price, routes: bestAtEndSoFar.routes.Add(previous.Add((x, y))));
                else if (price < bestAtEndSoFar.price)
                    bestAtEndSoFar = (price, [previous.Add((x, y))]);

                Console.WriteLine();
                break;

            case Tile.Start:
            case Tile.Empty:
                var (nextX, nextY, nextDirectionClockwise, nextDirectionCounterClockwise) = direction switch
                {
                    Direction.West => (x - 1, y, Direction.North, Direction.South),
                    Direction.North => (x, y - 1, Direction.East, Direction.West),
                    Direction.East => (x + 1, y, Direction.South, Direction.North),
                    Direction.South => (x, y + 1, Direction.West, Direction.East),
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (previous.Contains((nextX, nextY)))
                    continue;
                if (maze[nextX, nextY] is Tile.Wall)
                    continue;

                previous = previous.Add((x, y));
                workingSet.Enqueue((nextX, nextY, direction, price + 1, previous), price + 1);
                workingSet.Enqueue((nextX, nextY, nextDirectionClockwise, price + 1 + 1000, previous), price + 1000);
                workingSet.Enqueue((nextX, nextY, nextDirectionCounterClockwise, price + 1 + 1000, previous), price + 1000);

                break;
        }
    }

    return bestAtEndSoFar;
}

var (cheapestPath, bestRoutes) = FindAllCheapestPaths(inputMaze);
Console.WriteLine($"Cheapest path: {cheapestPath}");
Console.WriteLine("All best routes:");
Console.WriteLine(string.Join("\n", bestRoutes.Select(r => $"  {string.Join(", ", r)}")));
Console.WriteLine($"Unique tiles in all best routes: {bestRoutes.SelectMany(r => r).Distinct().Count()}");

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

    public (int x, int y) Start { get; }

    public Tile this[int x, int y]
    {
        get => _maze[x, y];
        private init => _maze[x, y] = value;
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
