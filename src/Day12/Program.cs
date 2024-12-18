﻿using System.Collections;
using System.Diagnostics;

var input = new Input();

var regions = new List<Region>();

foreach (var (line, column, type) in input)
{
    var plot = new Plot(input, line, column, type);
    if (!regions.Any(r => r.Contains(plot)))
        regions.Add(new Region(input, plot));
}

Console.WriteLine(string.Join("\n", regions));
Console.WriteLine($"Expected total area: {input.Count()}");
Console.WriteLine($"Total area: {regions.Sum(r => r.Area)}");
Console.WriteLine($"Total price: {regions.Sum(r => r.Price)}");
Console.WriteLine($"Total bulk price: {regions.Sum(r => r.BulkPrice)}");

internal class Input : IEnumerable<(int line, int column, char type)>
{
    private static readonly string[] _input = File.ReadAllLines("input.txt");

    public char? this[int line, int column]
    {
        get
        {
            if (line < 0 || line >= _input.Length)
                return null;
            if (column < 0 || column >= _input[line].Length)
                return null;
            return _input[line][column];
        }
    }

    public IEnumerator<(int line, int column, char type)> GetEnumerator()
    {
        for (var i = 0; i < _input.Length; i++)
        {
            for (var j = 0; j < _input[i].Length; j++)
                yield return (i, j, _input[i][j]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

[DebuggerDisplay("({Line}, {Column}, {Type})")]
internal record Plot(Input Input, int Line, int Column, char Type)
{
    public Plot? Down => At(Line + 1, Column);
    public Plot? Left => At(Line, Column - 1);
    public Plot? Right => At(Line, Column + 1);
    public Plot? Up => At(Line - 1, Column);
    private Input Input { get; } = Input;

    private Plot? At(int line, int column)
    {
        return Input[line, column] is { } type
            ? new Plot(Input, line, column, type)
            : null;
    }
}

[DebuggerDisplay("({Type}, {Plots.Count})")]
internal class Region
{
    public int Area => Plots.Count;
    public int BulkPrice => Area * Sides;
    public int Perimeter { get; }
    public IReadOnlyCollection<Plot> Plots { get; }
    public int Price => Area * Perimeter;
    public int Sides { get; }
    public char Type { get; }

    public Region(Input input, Plot topLeftMostPlot)
    {
        Type = topLeftMostPlot.Type;

        var plots = new List<Plot>();
        Expand(plots, topLeftMostPlot);
        Plots = [..plots.Distinct()];
        Perimeter = Plots.Sum(ComputePerimeter);
        Sides = CountSides();
    }

    public bool Contains(Plot plot)
        => Plots.Contains(plot);

    public override string ToString()
    {
        return $"A region of {Type} plants of area {Area} and perimeter {Perimeter} costing {Price}.";
    }

    private int ComputePerimeter(Plot plot)
    {
        return (plot.Left?.Type == plot.Type ? 0 : 1)
               + (plot.Up?.Type == plot.Type ? 0 : 1)
               + (plot.Down?.Type == plot.Type ? 0 : 1)
               + (plot.Right?.Type == plot.Type ? 0 : 1);
    }

    private int CountSides()
    {
        HashSet<(Plot, Side)> sides = new();

        foreach (var plot in Plots)
        {
            if (plot.Left?.Type != Type)
            {
                var current = plot;
                while (true)
                {
                    if (current.Up?.Type == Type)
                    {
                        if (current.Up.Left?.Type == Type)
                        {
                            sides.Add((current, Side.Left));
                            break;
                        }

                        current = current.Up;
                    }
                    else
                    {
                        sides.Add((current, Side.Left));
                        break;
                    }
                }
            }

            if (plot.Up?.Type != Type)
            {
                var current = plot;
                while (true)
                {
                    if (current.Left?.Type == Type)
                    {
                        if (current.Left.Up?.Type == Type)
                        {
                            sides.Add((current, Side.Top));
                            break;
                        }

                        current = current.Left;
                    }
                    else
                    {
                        sides.Add((current, Side.Top));
                        break;
                    }
                }
            }

            if (plot.Right?.Type != Type)
            {
                var current = plot;
                while (true)
                {
                    if (current.Up?.Type == Type)
                    {
                        if (current.Up.Right?.Type == Type)
                        {
                            sides.Add((current, Side.Right));
                            break;
                        }

                        current = current.Up;
                    }
                    else
                    {
                        sides.Add((current, Side.Right));
                        break;
                    }
                }
            }

            if (plot.Down?.Type != Type)
            {
                var current = plot;
                while (true)
                {
                    if (current.Left?.Type == Type)
                    {
                        if (current.Left.Down?.Type == Type)
                        {
                            sides.Add((current, Side.Bottom));
                            break;
                        }

                        current = current.Left;
                    }
                    else
                    {
                        sides.Add((current, Side.Bottom));
                        break;
                    }
                }
            }
        }

        return sides.Count;
    }

    private void Expand(List<Plot> plots, Plot? plot)
    {
        if (plot is null || plots.Contains(plot))
            return;

        if (plot.Type != Type)
            return;

        plots.Add(plot);

        Expand(plots, plot.Left);
        Expand(plots, plot.Up);
        Expand(plots, plot.Right);
        Expand(plots, plot.Down);
    }

    private enum Side
    {
        Left,
        Top,
        Right,
        Bottom
    }
}
