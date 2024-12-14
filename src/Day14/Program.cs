using System.Text.RegularExpressions;

var input = File.ReadAllLines("input.txt");

var robots = input.Select(Robot.Parse)
                  .ToList();
Console.WriteLine("Robots:");
Console.WriteLine(string.Join('\n', robots.Select(r => $"  {r}")));

PrintFloorMap();
foreach (var _ in Enumerable.Range(0, 100))
{
    foreach (var robot in robots)
        robot.MoveOnce();

    PrintFloorMap();
    Console.WriteLine();
}

var safetyScore = robots.Where(r => r.Quardrant != null)
                        .GroupBy(r => r.Quardrant)
                        .Select(g => g.Count())
                        .Aggregate((product, next) => product * next);
Console.WriteLine($"Safety score: {safetyScore}");

void PrintFloorMap()
{
    for (var y = 0; y < Robot.FloorMap.GetLength(1); y++)
    {
        for (var x = 0; x < Robot.FloorMap.GetLength(0); x++)
        {
            Console.Write(Robot.FloorMap[x, y] switch
            {
                0 => ".",
                var n => $"{n}"
            });
        }

        Console.WriteLine();
    }
}

public class Robot
{
    private const int AreaHeight = 103;
    private const int AreaMiddleX = AreaWidth / 2;
    private const int AreaMiddleY = AreaHeight / 2;
    private const int AreaWidth = 101;

    public static readonly int[,] FloorMap = new int[AreaWidth, AreaHeight];
    private static readonly Regex _robotRegex = new(@"^p=(?<x>\d+),(?<y>\d+) v=(?<vx>-?\d+),(?<vy>-?\d+)$");


    public int? Quardrant
        => (X, Y) switch
        {
            (AreaMiddleX, _) => null,
            (_, AreaMiddleY) => null,
            (< AreaMiddleX, < AreaMiddleY) => 1,
            (> AreaMiddleX, < AreaMiddleY) => 2,
            (< AreaMiddleX, > AreaMiddleY) => 3,
            (> AreaMiddleX, > AreaMiddleY) => 4
        };

    public int VelocityX { get; }
    public int VelocityY { get; }
    public int X { get; private set; }
    public int Y { get; private set; }

    public Robot(int x, int y, int velocityX, int velocityY)
    {
        VelocityX = velocityX;
        VelocityY = velocityY;
        X = x;
        Y = y;
        FloorMap[X, Y]++;
    }

    public void MoveOnce()
    {
        FloorMap[X, Y]--;

        X = (X + VelocityX + AreaWidth) % AreaWidth;
        Y = (Y + VelocityY + AreaHeight) % AreaHeight;

        FloorMap[X, Y]++;
    }

    public override string ToString()
        => $"p={X},{Y} v={VelocityX},{VelocityY} q={Quardrant}";

    public static Robot Parse(string input)
    {
        if (_robotRegex.Match(input) is not { Success: true, Groups: { } groups })
            throw new ArgumentOutOfRangeException(nameof(input), "Invalid robot input");

        var x = int.Parse(groups["x"].Value);
        var y = int.Parse(groups["y"].Value);
        var velocityX = int.Parse(groups["vx"].Value);
        var velocityY = int.Parse(groups["vy"].Value);
        return new Robot(x, y, velocityX, velocityY);
    }
}
