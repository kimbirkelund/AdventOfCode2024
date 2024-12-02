// See https://aka.ms/new-console-template for more information

var reports = File.ReadAllLines("input.txt")
                  .Select(line => line.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                                      .Select(int.Parse)
                                      .ToList());

var safeReports = reports
                  .Where(report =>
                         {
                             return report.Zip(report.Skip(1), (x, y) => (x - y) switch
                                          {
                                              >= 1 and <= 3 => 1,
                                              <= -1 and >= -3 => -1,
                                              _ => 0
                                          })
                                          .Sum() is { } risingOrDecreasingTerms
                                    && Math.Abs(risingOrDecreasingTerms) == report.Count - 1;
                         })
                  .Count();

Console.WriteLine($"SafeReports: {safeReports}");
