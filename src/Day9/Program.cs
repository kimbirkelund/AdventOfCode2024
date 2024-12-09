using System.Text;

var input = File.ReadAllText("input.txt").Trim();

IBlock lastBlock = new StartBlock();

var nextFileId = new FileId(0);
foreach (var (length, index) in input.Select(v => int.Parse($"{v}")).Select((length, index) => (value: length, index)))
{
    if (index % 2 == 0)
    {
        var fileId = nextFileId;
        nextFileId = nextFileId.Next();

        for (var i = 0; i < length; i++)
            lastBlock = new FileBlock(lastBlock, fileId);
    }
    else
    {
        for (var i = 0; i < length; i++)
            lastBlock = new EmptyBlock(lastBlock);
    }
}

string PrintFileMap(IBlock block)
{
    var workingSet = new Stack<string>();
    var current = block;
    var sb = new StringBuilder();

    while (true)
    {
        switch (current)
        {
            case EmptyBlock(var previous):
                workingSet.Push(".");
                current = previous;
                break;
            case FileBlock (var previous, var fileId):
                workingSet.Push(fileId.Value.ToString());
                current = previous;
                break;
            case StartBlock startBlock:
                return string.Join("", workingSet);
            default:
                throw new ArgumentOutOfRangeException(nameof(current));
        }
    }

    return "";
}

Console.WriteLine($"Initial file map: {PrintFileMap(lastBlock)}");

IBlock Compact(IBlock initialBlock)
{
    var workingSet = new Stack<IBlock>();
    workingSet.Push(initialBlock);

    List<FileBlock> fileIdsToCompact = new();

    while (true)
    {
        switch (workingSet.Peek())
        {
            case EmptyBlock(var previous):
                workingSet.Push(previous);
                break;
            case FileBlock(var previous, var _) fileBlock:
                fileIdsToCompact.Add(fileBlock);
                workingSet.Push(previous);
                break;
            case StartBlock startBlock:
                IBlock compactedBlock = startBlock;

                while (workingSet.Any())
                {
                    var current = workingSet.Pop();
                    switch (current)
                    {
                        case StartBlock:
                            break;
                        case FileBlock { Previous: var previous, FileId: var fileId } fileBlock:
                        {
                            if (fileIdsToCompact.Remove(fileBlock))
                                compactedBlock = new FileBlock(compactedBlock, fileId);

                            break;
                        }
                        case EmptyBlock { Previous: var previous }:
                        {
                            if (fileIdsToCompact.Count == 0)
                                break;

                            var fileId = fileIdsToCompact[0].FileId;
                            fileIdsToCompact.RemoveAt(0);
                            compactedBlock = new FileBlock(compactedBlock, fileId);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException(nameof(current), current, null);
                    }
                }

                return compactedBlock;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

var compactedLastBlock = Compact(lastBlock);
Console.WriteLine($"Compacted file map: {PrintFileMap(compactedLastBlock)}");

long ComputeChecksum(IBlock initialBlock)
{
    var workingSet = new Stack<IBlock>();
    workingSet.Push(initialBlock);

    while (true)
    {
        switch (workingSet.Peek())
        {
            case EmptyBlock (var previous):
                workingSet.Push(previous);
                break;
            case FileBlock (var previous, var _):
                workingSet.Push(previous);
                break;
            case StartBlock startBlock:
                long checksum = 0;
                var nextIndex = 0;

                while (workingSet.Any())
                {
                    var current = workingSet.Pop();
                    switch (current)
                    {
                        case EmptyBlock:
                            nextIndex += 1;
                            break;
                        case FileBlock(var _, var fileId):
                        {
                            checksum = checksum + nextIndex * fileId.Value;
                            nextIndex = nextIndex + 1;
                            break;
                        }
                        case StartBlock:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(current));
                    }
                }

                return checksum;

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

var checksum = ComputeChecksum(compactedLastBlock);
Console.WriteLine($"Checksum: {checksum}");

internal record FileId(int Value)
{
    public FileId Next()
        => new(Value + 1);

    public override string ToString()
        => $"{Value}";
}

internal interface IBlock;

internal record StartBlock : IBlock;

internal record EmptyBlock(IBlock Previous) : IBlock;

internal record FileBlock(IBlock Previous, FileId FileId) : IBlock
{
    public override string ToString()
        => FileId?.Value.ToString() ?? ".";
}
