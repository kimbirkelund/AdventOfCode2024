using System.Diagnostics;

var input = File.ReadAllLines("input.txt");

Contents GetContents(int lineNumber, int columnNumber, bool treatGuardPositionAsNothing = true)
{
    if (lineNumber < 0 || lineNumber >= input.Length)
        return Contents.Outside;

    var line = input[lineNumber];
    if (columnNumber < 0 || columnNumber >= line.Length)
        return Contents.Outside;

    return line[columnNumber] switch
    {
        '#' => Contents.Obstacle,
        '.' => Contents.Nothing,
        '<' => treatGuardPositionAsNothing ? Contents.Nothing : Contents.GuardMovingLeft,
        '^' => treatGuardPositionAsNothing ? Contents.Nothing : Contents.GuardMovingUp,
        '>' => treatGuardPositionAsNothing ? Contents.Nothing : Contents.GuardMovingRight,
        'v' => treatGuardPositionAsNothing ? Contents.Nothing : Contents.GuardMovingDown,
        var c => throw new ArgumentOutOfRangeException($"Unknown contents: [{c}].")
    };
}

var (initialGuardPosition, initialGuardDirection) =
    Enumerable.Range(0, input.Length)
              .SelectMany(l => Enumerable.Range(0, input[l].Length)
                                         .Select(c => (l, c)))
              .Select(c => (position: (line: c.l, column: c.c), contents: GetContents(c.l, c.c, false)))
              .First(t => t.contents is
                         Contents.GuardMovingLeft
                         or Contents.GuardMovingUp
                         or Contents.GuardMovingRight
                         or Contents.GuardMovingDown);


Solution Solve((int line, int column) newObstaclePosition)
{
    Console.WriteLine($"New obstacle at {newObstaclePosition} makes guard...");
    var (guardPosition, guardDirection) = (initialGuardPosition, initialGuardDirection);

    var visitedPositions = new HashSet<(int, int)>();
    var iterationsSinceNewPosition = 0;
    // Console.WriteLine($"Initial Position: {guardPosition}, direction: {guardDirection}, visited positions: {visitedPositions.Count}");

    while (guardDirection != Contents.Outside)
    {
        if (visitedPositions.Add(guardPosition))
            iterationsSinceNewPosition = 0;
        else
            iterationsSinceNewPosition++;

        (int line, int column) facingPosition = guardDirection switch
        {
            Contents.GuardMovingLeft => (guardPosition.line, guardPosition.column - 1),
            Contents.GuardMovingUp => (guardPosition.line - 1, guardPosition.column),
            Contents.GuardMovingRight => (guardPosition.line, guardPosition.column + 1),
            Contents.GuardMovingDown => (guardPosition.line + 1, guardPosition.column),
            var c => throw new ArgumentOutOfRangeException($"Unknown contents: [{c}].")
        };

        (guardPosition, guardDirection) = (
                facingPosition == newObstaclePosition
                    ? Contents.Obstacle
                    : GetContents(facingPosition.line, facingPosition.column)) switch
            {
                Contents.Nothing => (facingPosition, guardDirection),
                Contents.Obstacle => (guardPosition, guardDirection switch
                {
                    Contents.GuardMovingLeft => Contents.GuardMovingUp,
                    Contents.GuardMovingUp => Contents.GuardMovingRight,
                    Contents.GuardMovingRight => Contents.GuardMovingDown,
                    Contents.GuardMovingDown => Contents.GuardMovingLeft,
                    var c => throw new ArgumentOutOfRangeException($"Unknown contents: [{c}].")
                }),
                Contents.Outside => ((-1, -1), Contents.Outside),
                var c => throw new ArgumentOutOfRangeException($"Unknown contents: [{c}].")
            };

        // Console.WriteLine($"Position: {guardPosition}, direction: {guardDirection}, visited positions: {visitedPositions.Count}, iterations since new position: {iterationsSinceNewPosition}");

        if (iterationsSinceNewPosition > visitedPositions.Count * 2)
        {
            Console.WriteLine($"New obstacle at {newObstaclePosition} makes guard loop");
            return Solution.Loops;
        }
    }

    // Console.WriteLine($"Final Position: {guardPosition}, direction: {guardDirection}, visited positions: {visitedPositions.Count}");

    Console.WriteLine($"New obstacle at {newObstaclePosition} makes guard leave");
    return Solution.Leaves;
}

var stopwatch = Stopwatch.StartNew();
var newObstaclePositionsThatMakeGuardLoop =
    Enumerable
        .Range(0, input.Length)
        .AsParallel()
        .SelectMany(l => Enumerable.Range(0, input[l].Length)
                                   .Select(c => (l, c)))
        .Select(c => (position: (line: c.l, column: c.c), contents: GetContents(c.l, c.c, false)))
        .Where(t => t.contents is Contents.Nothing)
        .Select(t => t.position)
        .Select(Solve)
        .Count(s => s is Solution.Loops);
stopwatch.Stop();
Console.WriteLine($"Solution took {stopwatch.Elapsed}");

Console.WriteLine($"New obstacle positions that make guard loop: {newObstaclePositionsThatMakeGuardLoop}");


internal enum Contents
{
    Nothing,
    Obstacle,
    GuardMovingLeft,
    GuardMovingUp,
    GuardMovingRight,
    GuardMovingDown,
    Outside
}

internal enum Solution
{
    Leaves,
    Loops
}
