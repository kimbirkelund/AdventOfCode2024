using System.Text.RegularExpressions;

var input = File.ReadAllLines("input.txt");

IOperator[] operators =
[
    new Addition(),
    new Multiply()
];

IEnumerable<IEnumerable<IOperator>> GetAllCombinationsOfOperators(int size)
{
    var combinations = new List<IEnumerable<IOperator>>();

    if (size == 1)
    {
        foreach (var op in operators)
            yield return [op];
    }
    else
    {
        foreach (var combination in GetAllCombinationsOfOperators(size - 1))
        {
            var localCombination = combination.ToList();
            foreach (var op in operators)
                yield return [op, ..localCombination];
        }
    }
}

bool IsSolvable(long result, IReadOnlyList<long> operands)
{
    var operatorCombinations = GetAllCombinationsOfOperators(operands.Count - 1);

    return operatorCombinations
        .Any(c =>
                 c.Zip(operands.Skip(1), (op, operand) => (op, operand))
                  .Aggregate(operands[0],
                             (accum, next) => next.op.Evaluate(accum, next.operand)) == result);
}

var equationParser = new Regex(@"(?<result>\d+):( (?<operand>\d+))+", RegexOptions.Compiled);

IReadOnlyList<(long result, IReadOnlyList<long> operands)> equations =
    input.Select(l =>
                 {
                     if (equationParser.Match(l) is not { Success: true, Groups: { } groups })
                         throw new Exception($"Line not equation: '{l}'");

                     return (
                         result: long.Parse(groups["result"].Value),
                         operands: (IReadOnlyList<long>)
                         [
                             ..groups["operand"].Captures
                                                .Select(c => long.Parse(c.Value))
                         ]);
                 })
         .ToList();

var solvableEquations =
    equations.Where(t => IsSolvable(t.result, t.operands))
             .ToList();

var sumOfResultsOfSolvableEquations = solvableEquations.Sum(e => e.result);

Console.WriteLine($"Sum of results of solvable equations: {sumOfResultsOfSolvableEquations}");

internal interface IOperator
{
    long Evaluate(long left, long right);
}

internal class Addition : IOperator
{
    public long Evaluate(long left, long right)
        => left + right;

    public override string ToString()
        => "+";
}

internal class Multiply : IOperator
{
    public long Evaluate(long left, long right)
        => left * right;

    public override string ToString()
        => "*";
}
