var input = File.ReadAllLines("input.txt");


Contents GetContents(int lineNumber, int columnNumber)
{
    if (lineNumber < 0 || lineNumber >= input.Length)
        return new Contents();
    var line = input[lineNumber];
    if (columnNumber < 0 || columnNumber >= line.Length)
        return Contents.Outside;

    return line[columnNumber] switch
    {
        '#' => Contents.Obstacle,
        '.' => Contents.Nothing,
        '<' => Contents.GuardMovingLeft,
        '^' => Contents.GuardMovingUp,
        '>' => Contents.GuardMovingRight,
        'v' => Contents.GuardMovingDown,
        var c => throw new ArgumentOutOfRangeException($"Unknown contents: [{c}].")
    };
}

var (guardPosition, guardDirection) =
    Enumerable.Range(0, input.Length)
              .SelectMany(l => Enumerable.Range(0, input[l].Length)
                                         .Select(c => (l, c)))
              .Select(c => (position: (line: c.l, column: c.c), contents: GetContents(c.l, c.c)))
              .First(t => t.contents is
                         Contents.GuardMovingLeft
                         or Contents.GuardMovingUp
                         or Contents.GuardMovingRight
                         or Contents.GuardMovingDown);
var visitedPositions = new HashSet<(int, int)>();
Console.WriteLine($"Initial Position: {guardPosition}, direction: {guardDirection}, visited positions: {visitedPositions.Count}");

while (guardDirection != Contents.Outside)
{
    visitedPositions.Add(guardPosition);

    (int line, int column) facingPosition = guardDirection switch
    {
        Contents.GuardMovingLeft => (guardPosition.line, guardPosition.column - 1),
        Contents.GuardMovingUp => (guardPosition.line - 1, guardPosition.column),
        Contents.GuardMovingRight => (guardPosition.line, guardPosition.column + 1),
        Contents.GuardMovingDown => (guardPosition.line + 1, guardPosition.column),
        var c => throw new ArgumentOutOfRangeException($"Unknown contents: [{c}].")
    };

    (guardPosition, guardDirection) = GetContents(facingPosition.line, facingPosition.column) switch
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

    Console.WriteLine($"Position: {guardPosition}, direction: {guardDirection}, visited positions: {visitedPositions.Count}");
}

Console.WriteLine($"Final Position: {guardPosition}, direction: {guardDirection}, visited positions: {visitedPositions.Count}");

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
