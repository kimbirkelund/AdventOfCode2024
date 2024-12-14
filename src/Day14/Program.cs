using System.Text.RegularExpressions;

var input = File.ReadAllLines("input.txt");

var robots = input.Select(Robot.Parse)
                  .ToList();
Console.WriteLine("Robots:");
Console.WriteLine(string.Join('\n', robots.Select(r => $"  {r}")));

var seconds = 0;
var speed = 0;
var stepOne = 0;


var waitHandle = new AutoResetEvent(initialState: false);

Task.Run(() =>
         {
             while (true)
             {
                 var key = Console.ReadKey();

                 if (key.Key == ConsoleKey.UpArrow)
                     speed += key.Modifiers.HasFlag(ConsoleModifiers.Control) ? 10 : 1;
                 else if (key.Key == ConsoleKey.DownArrow)
                     speed -= key.Modifiers.HasFlag(ConsoleModifiers.Control) ? 10 : 1;
                 else if (key.Key == ConsoleKey.Spacebar)
                     speed = 0;
                 else if (key.Key == ConsoleKey.LeftArrow)
                 {
                     speed = 0;
                     stepOne = -1;
                 }
                 else if (key.Key == ConsoleKey.RightArrow)
                 {
                     speed = 0;
                     stepOne = 1;
                 }

                 waitHandle.Set();
             }
         });

Console.Clear();

while (true)
{
    var actualSpeed = speed + stepOne;
    if (actualSpeed != 0)
    {
        foreach (var robot in robots)
        {
            if (actualSpeed < 0)
                robot.MoveBackward();
            else
                robot.MoveForward();
        }

        seconds += actualSpeed < 0 ? -1 : 1;
    }

    stepOne = 0;

    Console.SetCursorPosition(0, 0);
    Console.WriteLine($"Seconds: {seconds}, speed: {speed}".PadRight(Robot.AreaWidth, ' '));
    PrintFloorMap();
    Console.WriteLine();

    waitHandle.WaitOne(speed == 0 ? Timeout.Infinite : 1000 / Math.Abs(speed));
}

var safetyScore = robots.Where(r => r.Quadrant != null)
                        .GroupBy(r => r.Quadrant)
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
    public const int AreaMiddleX = AreaWidth / 2;
    private const int AreaMiddleY = AreaHeight / 2;
    public const int AreaWidth = 101;
    public static readonly int[,] FloorMap = new int[AreaWidth, AreaHeight];
    private static readonly Regex _robotRegex = new(@"^p=(?<x>\d+),(?<y>\d+) v=(?<vx>-?\d+),(?<vy>-?\d+)$");


    public int? Quadrant
        => (X, Y) switch
        {
            (AreaMiddleX, _) => null,
            (_, AreaMiddleY) => null,
            (< AreaMiddleX, < AreaMiddleY) => 1,
            (> AreaMiddleX, < AreaMiddleY) => 2,
            (< AreaMiddleX, > AreaMiddleY) => 3,
            (> AreaMiddleX, > AreaMiddleY) => 4
        };

    public int X { get; set; }
    public int Y { get; set; }

    private int VelocityX { get; }
    private int VelocityY { get; }

    private Robot(int x, int y, int velocityX, int velocityY)
    {
        VelocityX = velocityX;
        VelocityY = velocityY;
        X = x;
        Y = y;
        FloorMap[X, Y]++;
    }

    public void MoveBackward()
    {
        FloorMap[X, Y]--;

        X = (X - VelocityX + AreaWidth) % AreaWidth;
        Y = (Y - VelocityY + AreaHeight) % AreaHeight;

        FloorMap[X, Y]++;
    }

    public void MoveForward()
    {
        FloorMap[X, Y]--;

        X = (X + VelocityX + AreaWidth) % AreaWidth;
        Y = (Y + VelocityY + AreaHeight) % AreaHeight;

        FloorMap[X, Y]++;
    }

    public override string ToString()
        => $"p={X},{Y} v={VelocityX},{VelocityY} q={Quadrant}";

    public static int FloorAt(int centerX, int centerY)
    {
        if (centerX < 0 || centerX >= AreaWidth)
            return -1;
        if (centerY < 0 || centerY >= AreaHeight)
            return -1;

        return FloorMap[centerX, centerY];
    }

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
