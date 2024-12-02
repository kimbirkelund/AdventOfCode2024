// See https://aka.ms/new-console-template for more information

var reports = File.ReadAllLines("input.txt")
                  .Select(line => line.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                                      .Select(int.Parse)
                                      .ToList());

bool IsReportSafe(IReadOnlyList<int> report)
{
    var isReportSafe = report.Zip(report.Skip(1), (x, y) => (x - y) switch
                             {
                                 >= 1 and <= 3 => 1,
                                 <= -1 and >= -3 => -1,
                                 _ => 0
                             })
                             .Sum() is { } risingOrDecreasingTerms && Math.Abs(risingOrDecreasingTerms) == report.Count - 1;
    Console.WriteLine($"   {string.Join(", ", report)}: {isReportSafe}");
    return isReportSafe;
}

var safeReports = reports
    .Count(r =>
           {
               Console.WriteLine($"{string.Join(", ", r)}:");
               return IsReportSafe(r)
                      || Enumerable.Range(0, r.Count)
                                   .Select(i => (int[]) [..r.Take(i), ..r.Skip(i + 1)])
                                   .Any(IsReportSafe);
           });

Console.WriteLine($"SafeReports: {safeReports}");
