// See https://aka.ms/new-console-template for more information

var input = File.ReadAllLines("input.txt");

var result = 0;

char GetChar((int line, int character) c)
    => c.line >= 0
       && c.line < input.Length
       && c.character >= 0
       && c.character < input[c.line].Length
        ? input[c.line][c.character]
        : '_';

bool IsXmas((int line, int character) c1, (int line, int character) c2, (int line, int character) c3, (int line, int character) c4)
    => (GetChar(c1) == 'X'
        && GetChar(c2) == 'M'
        && GetChar(c3) == 'A'
        && GetChar(c4) == 'S')
       ||
       (GetChar(c4) == 'X'
        && GetChar(c3) == 'M'
        && GetChar(c2) == 'A'
        && GetChar(c1) == 'S');

void CountXmas((int line, int character) c1, (int line, int character) c2, (int line, int character) c3, (int line, int character) c4)
{
    if (IsXmas(c1, c2, c3, c4))
        result += 1;
}

var lineCount = input.Length;
var lineLength = input[0].Length;

for (var i = 0; i < lineCount; i++)
for (var j = 0; j < lineLength; j++)
{
    CountXmas((i, j), (i, j + 1), (i, j + 2), (i, j + 3));
    CountXmas((i, j), (i + 1, j), (i + 2, j), (i + 3, j));
    CountXmas((i, j), (i + 1, j + 1), (i + 2, j + 2), (i + 3, j + 3));
    CountXmas((i, j), (i - 1, j + 1), (i - 2, j + 2), (i - 3, j + 3));
}

Console.WriteLine(result);
