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

bool IsMas((int line, int character) c1, (int line, int character) c2, (int line, int character) c3)
    => (GetChar(c1) == 'M'
        && GetChar(c2) == 'A'
        && GetChar(c3) == 'S')
       ||
       (GetChar(c3) == 'M'
        && GetChar(c2) == 'A'
        && GetChar(c1) == 'S');

bool IsXMas(int line, int character)
    => IsMas((line, character), (line + 1, character + 1), (line + 2, character + 2))
       && IsMas((line, character + 2), (line + 1, character + 1), (line + 2, character));

void CountXMas(int line, int character)
{
    if (IsXMas(line, character))
        result += 1;
}

var lineCount = input.Length;
var lineLength = input[0].Length;

for (var i = 0; i < lineCount; i++)
for (var j = 0; j < lineLength; j++)
{
    CountXMas(i, j);
}

Console.WriteLine(result);
