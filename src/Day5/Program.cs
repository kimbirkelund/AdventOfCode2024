﻿var input = File.ReadAllLines("input.txt");

var orders = input.TakeWhile(v => v != "")
                  .Select(v => v.Split("|"))
                  .Select(v => (before: v[0], after: v[1]))
                  .ToLookup(t => t.before, t => t.after);

Console.WriteLine($"Orders ({orders.Count()}):\n" + string.Join("\n", orders.Select(g => $"  {g.Key}: {string.Join(", ", g)}")));

var updates = input.SkipWhile(v => v != "")
                   .Where(v => v != "")
                   .Select(l => l.Split(",").ToList())
                   .ToList();
Console.WriteLine($"Updates: {updates.Count}");


bool IsCorrectlyOrdered(IReadOnlyList<string> update, int index)
{
    // Check no numbers before index are supposed to come after index
    return Enumerable.Range(0, index)
                     .All(i => !orders[update[index]].Contains(update.ElementAt(i)));
}

var correctlyOrderedUpdates =
    updates.Where(o => o.Select((_, i) => IsCorrectlyOrdered(o, i))
                        .All(v => v))
           .ToList();

Console.WriteLine($"Correctly ordered updates: {correctlyOrderedUpdates.Count}");

var sumOfMiddlePagesOfCorrectlyOrderedUpdates =
    correctlyOrderedUpdates.Select(o => o[o.Count / 2])
                           .Select(int.Parse)
                           .Sum();
Console.WriteLine($"Sum of middle pages of correctly ordered updates: {sumOfMiddlePagesOfCorrectlyOrderedUpdates}");

var incorrectlyOrderedUpdates = updates.Except(correctlyOrderedUpdates).ToList();

Console.WriteLine($"Incorrectly ordered updates: {incorrectlyOrderedUpdates.Count}");

IReadOnlyList<string> FixIncorrectlyOrdered(IReadOnlyList<string> update)
{
    var fixedOrder =
        update.OrderByDescending(v => update.Except([v])
                                            .Count(ov => orders[v].Contains(ov)))
              .ToList();
    return fixedOrder;
}

var fixedOrderUpdates =
    incorrectlyOrderedUpdates.Select(FixIncorrectlyOrdered)
                             .ToList();

Console.WriteLine($"Fixed updates: {fixedOrderUpdates.Count}");

var sumOfMiddlePagesOfFixedUpdates =
    fixedOrderUpdates.Select(o => o[o.Count / 2])
                     .Select(int.Parse)
                     .Sum();
Console.WriteLine($"Sum of middle pages of fixed ordered updates: {sumOfMiddlePagesOfFixedUpdates}");
