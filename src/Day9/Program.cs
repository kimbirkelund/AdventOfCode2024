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

    (int startIndex, int length)? FindNextFile(FileId fileIdToFind)
    {
        while (disk[backIndex] != fileIdToFind && backIndex > frontIndex)
            backIndex--;
        if (backIndex <= frontIndex)
            return null;

        var length = 0;
        while (disk[backIndex] == fileIdToFind && backIndex > frontIndex)
        {
            backIndex--;
            length++;
        }

        return (backIndex + 1, length);
    }

    int? FindMatchingFreeSpace(int requiredLength)
    {
        while (disk[frontIndex] is { } && frontIndex <= backIndex)
            frontIndex++;
        if (frontIndex > backIndex)
            return null;

        var index = frontIndex;
        while (true)
        {
            while (disk[index] is { } && index <= backIndex)
                index++;
            if (index > backIndex)
                return null;

            var length = 0;
            while (disk[index] is null && index <= backIndex)
            {
                index++;
                length++;

                if (length >= requiredLength)
                    return index - length;
            }
        }
    }

    var nextFileIdToMove = nextFileId.Previous();
    while (frontIndex < backIndex)
    {
        if (FindNextFile(nextFileIdToMove) is not (var (fileStartIndex, fileLength)))
            return;

        if (FindMatchingFreeSpace(fileLength) is { } emptyStartIndex)
        {
            for (var i = 0; i < fileLength; i++)
            {
                disk[emptyStartIndex + i] = nextFileIdToMove;
                disk[fileStartIndex + i] = null;
            }
        }

        nextFileIdToMove = nextFileIdToMove.Previous();
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
internal record FileId(long Value)
{
    public FileId Next()
        => new(Value + 1);

    public FileId Previous()
        => new(Value - 1);

    public override string ToString()
        => $"{Value}";
}
