using System.Diagnostics;

var input = File.ReadAllText("input.txt").Trim();

var nextFileId = new FileId(0);

var disk =
    input.Select(v => int.Parse($"{v}"))
         .SelectMany((length, index) =>
                     {
                         if (index % 2 == 0)
                         {
                             var fileId = nextFileId;
                             nextFileId = nextFileId.Next();
                             return Enumerable.Range(0, length)
                                              .Select(_ => fileId);
                         }

                         return Enumerable.Range(0, length)
                                          .Select(_ => (FileId?)null);
                     })
         .ToArray();

string DiskToString()
    => string.Join("", disk.Select(d => $"{d?.Value.ToString() ?? "."}"));

Console.WriteLine($"Initial disk: {DiskToString()}");

void Compact()
{
    var frontIndex = 0;
    var backIndex = disk.Length - 1;

    while (frontIndex < backIndex)
    {
        while (disk[frontIndex] is { })
            frontIndex++;

        while (disk[backIndex] is null)
            backIndex--;

        if (frontIndex > backIndex)
            break;

        disk[frontIndex] = disk[backIndex];
        disk[backIndex] = null;
    }
}

Compact();
Console.WriteLine($"Compacted disk: {DiskToString()}");

long ComputeChecksum()
{
    long checksum = 0;
    for (var i = 0; i < disk.Length; i++)
    {
        if (disk[i] is { Value: var fileId })
            checksum += i * fileId;
    }

    return checksum;
}

var checksum = ComputeChecksum();
Console.WriteLine($"Checksum: {checksum}");

[DebuggerDisplay("{Value}")]
internal record FileId(int Value)
{
    public FileId Next()
        => new(Value + 1);

    public override string ToString()
        => $"{Value}";
}
