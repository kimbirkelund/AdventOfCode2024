using System.Collections.Immutable;
using System.Text.RegularExpressions;

var input = File.ReadAllLines("input.txt");

var progs = Prog.Parse(input);

foreach (var prog in progs)
{
    var initialA = prog.InitialMachine.A;
    Machine machine;
    do
    {
        Console.WriteLine($"Initial A: {initialA}");
        machine = prog.InitialMachine with { A = initialA };
        Console.WriteLine(machine.ToString());
        while (machine.InstructionCounter < prog.Instructions.Count)
        {
            Console.WriteLine($"Instruction: {prog.Instructions[machine.InstructionCounter]} {prog.Instructions[machine.InstructionCounter].Description}");

            machine = prog.Instructions[machine.InstructionCounter].Execute(machine);

            Console.WriteLine(machine);

            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }

        Console.WriteLine("Result:");
        Console.WriteLine(machine);

        Console.WriteLine("Expectations:");
        foreach (var expectation in prog.Expectations)
            Console.WriteLine($"  {expectation}: {expectation.IsSatisfied(machine)}");

        Console.WriteLine("---");

        initialA = (machine.Output.Count - prog.Instructions.Count * 2) switch
        {
            > 0 => initialA - 1,
            0 => initialA + 1,
            var diff and < 0 => initialA + Math.Abs(diff) * 434343
        };
        if (initialA <= 0)
            return;
    } while (prog.Expectations.Any(e => !e.IsSatisfied(machine)));
}

public class Prog(IReadOnlyList<Instruction> instructions, Machine initialMachine, IReadOnlyCollection<IExpectation> expectations)
{
    private static readonly Regex _regexExpectation = new(@"^Expect register (?<register>A|B|C): (?<value>\d+)|Expect output: (?<output>\d+)(,(?<output>\d+))+$");
    private static readonly Regex _regexProgram = new(@"^Program: (?<instruction>\d+)(,(?<instruction>\d+))+$");
    private static readonly Regex _regexRegister = new(@"^Register (?<register>A|B|C): (?<value>\d+)$");
    public IReadOnlyCollection<IExpectation> Expectations { get; } = expectations;
    public Machine InitialMachine { get; } = initialMachine;


    public IReadOnlyList<Instruction> Instructions { get; } = instructions;


    public override string ToString()
    {
        var s = $"""
                 Register A: {InitialMachine.A}
                 Register B: {InitialMachine.B}
                 Register C: {InitialMachine.C}

                 Program: {string.Join(",", Instructions)}
                 """;

        if (Expectations.Any())
            s += "\n\n" + string.Join("\n", Expectations);

        return s;
    }

    public static IEnumerable<Prog> Parse(IEnumerable<string> lines)
    {
        IReadOnlyList<Instruction> instructions = ImmutableList<Instruction>.Empty;
        var registers = new Machine();
        var expectations = ImmutableList<IExpectation>.Empty;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("---"))
            {
                if (instructions.Any())
                    yield return new Prog(instructions, registers, expectations);

                instructions = ImmutableList<Instruction>.Empty;
                registers = new Machine();
                expectations = ImmutableList<IExpectation>.Empty;
            }
            else if (ParseRegister(line, registers, out var updatedRegisters))
                registers = updatedRegisters;
            else if (ParseInstructions(line, out var updatedInstructions))
                instructions = updatedInstructions;
            else if (ParseExpectation(line, out var expectation))
                expectations = expectations.Add(expectation);
            else
                throw new Exception($"Unexpected line: {line}");
        }

        if (instructions.Any())
            yield return new Prog(instructions, registers, expectations);


        static bool ParseRegister(string line, Machine registers, out Machine updatedRegisters)
        {
            if (_regexRegister.Match(line) is not { Success: true, Groups: var groups })
            {
                updatedRegisters = registers;
                return false;
            }

            var register = groups["register"].Value;
            var value = int.Parse(groups["value"].Value);

            updatedRegisters = registers.SetRegister(register, value);
            return true;
        }

        static bool ParseInstructions(string s, out IReadOnlyList<Instruction> updatedInstructions)
        {
            if (_regexProgram.Match(s) is not { Success: true, Groups: var groups })
            {
                updatedInstructions = ImmutableList<Instruction>.Empty;
                return false;
            }

            updatedInstructions = groups["instruction"].Captures.Select(capture => capture.Value)
                                                       .Select(int.Parse)
                                                       .Chunk(2)
                                                       .Select(p => Instruction.Parse(p[0], p[1]))
                                                       .ToImmutableList();
            return true;
        }

        static bool ParseExpectation(string line, out IExpectation expectation)
        {
            if (_regexExpectation.Match(line) is not { Success: true, Groups: var groups })
            {
                expectation = null!;
                return false;
            }

            if (groups["register"].Success)
            {
                expectation = new RegisterExpectation(groups["register"].Value, int.Parse(groups["value"].Value));
            }
            else
            {
                expectation = new OutputExpectation(groups["output"].Captures.Select(capture => capture.Value)
                                                                    .Select(int.Parse)
                                                                    .ToImmutableList());
            }

            return true;
        }
    }
}

public class RegisterExpectation(string register, int expectedValue) : IExpectation
{
    public bool IsSatisfied(Machine machine)
    {
        return register switch
        {
            "A" => machine.A,
            "B" => machine.B,
            "C" => machine.C,
            _ => throw new ArgumentOutOfRangeException(nameof(register), register, null)
        } == expectedValue;
    }

    public override string ToString()
        => $"Expect register {register}: {expectedValue}";
}

public class OutputExpectation(IReadOnlyList<int> expectedOutput) : IExpectation
{
    public bool IsSatisfied(Machine machine)
        => machine.Output.SequenceEqual(expectedOutput);

    public override string ToString()
        => $"Expect output: {string.Join(",", expectedOutput)}";
}

public interface IExpectation
{
    bool IsSatisfied(Machine machine);
}

public record Machine
{
    public long A { get; init; }
    public long B { get; init; }
    public long C { get; init; }
    public int InstructionCounter { get; init; }
    public ImmutableList<int> Output { get; init; } = ImmutableList<int>.Empty;

    public Machine AppendOutput(int value)
        => this with { Output = Output.Add(value) };

    public long EvaluateComboOperand(int operand)
    {
        return operand switch
        {
            >= 0 and <= 3 => operand,
            4 => A,
            5 => B,
            6 => C,
            _ => throw new InvalidOperationException($"Unknown combo operand: {operand}")
        };
    }

    public Machine SetRegister(string register, int value)
        => register switch
        {
            "A" => this with { A = value },
            "B" => this with { B = value },
            "C" => this with { C = value },
            _ => throw new InvalidOperationException($"Unknown register: {register}")
        };

    public override string ToString()
        => $"""
            Register A: {A}
            Register B: {B}
            Register C: {C}
            Ouput: {string.Join(",", Output)}
            """;
}

public abstract class Instruction(int operand)
{
    public abstract string Description { get; }
    public abstract int Opcode { get; }
    public int Operand { get; } = operand;

    public abstract Machine Execute(Machine machine);

    public override string ToString()
        => $"{Opcode},{Operand}";

    public static Instruction Parse(int opcode, int operand)
        => opcode switch
        {
            0 => new AdvInstruction(operand),
            1 => new BxlInstruction(operand),
            2 => new BstInstruction(operand),
            3 => new JnzInstruction(operand),
            4 => new BxcInstruction(operand),
            5 => new OutInstruction(operand),
            6 => new BdvInstruction(operand),
            7 => new CdvInstruction(operand),
            _ => throw new ArgumentOutOfRangeException(nameof(opcode), opcode, null)
        };
}

public abstract class NonJumpInstructionBase(int operand) : Instruction(operand)
{
    public override Machine Execute(Machine machine)
        => machine with { InstructionCounter = machine.InstructionCounter + 1 /* only 1 because our instructions are not indexed with operands */ };
}

public class AdvInstruction(int operand) : NonJumpInstructionBase(operand)
{
    public override string Description => $"A = A / 2^combo({Operand})";
    public override int Opcode => 0;

    public override Machine Execute(Machine machine)
        => base.Execute(machine with
                        {
                            A = (int)(machine.A / Math.Pow(2, machine.EvaluateComboOperand(Operand)))
                        });
}

public class BxlInstruction(int operand) : NonJumpInstructionBase(operand)
{
    public override string Description => $"B = B xor {Operand}";
    public override int Opcode => 1;

    public override Machine Execute(Machine machine)
        => base.Execute(machine with
                        {
                            B = machine.B ^ Operand
                        });
}

public class BstInstruction(int operand) : NonJumpInstructionBase(operand)
{
    public override string Description => $"B = combo({Operand}) % 8";
    public override int Opcode => 2;

    public override Machine Execute(Machine machine)
    {
        return base.Execute(machine with
                            {
                                B = machine.EvaluateComboOperand(Operand) % 8
                            });
    }
}

public class JnzInstruction(int operand) : Instruction(operand)
{
    public override string Description => "A == 0 ? IC + 1 : IC = Operand";
    public override int Opcode => 3;

    public override Machine Execute(Machine machine)
        => machine.A switch
        {
            0 => machine with { InstructionCounter = machine.InstructionCounter + 1 /* only 1 because our instructions are not indexed with operands */ },
            _ => machine with { InstructionCounter = Operand / 2 /* divide by two because our instructions are not indexed with operands */ }
        };
}

public class BxcInstruction(int operand) : NonJumpInstructionBase(operand)
{
    public override string Description => "B = B xor C";
    public override int Opcode => 4;

    public override Machine Execute(Machine machine)
        => base.Execute(machine with
                        {
                            B = machine.B ^ machine.C
                        });
}

public class OutInstruction(int operand) : NonJumpInstructionBase(operand)
{
    public override string Description => $"OUT combo({Operand}) % 8";
    public override int Opcode => 5;

    public override Machine Execute(Machine machine)
        => base.Execute(machine.AppendOutput((int)(machine.EvaluateComboOperand(Operand) % 8)));
}

public class BdvInstruction(int operand) : NonJumpInstructionBase(operand)
{
    public override string Description => $"B = A / 2^combo({Operand})";
    public override int Opcode => 6;

    public override Machine Execute(Machine machine)
        => base.Execute(machine with
                        {
                            B = (int)(machine.A / Math.Pow(2, machine.EvaluateComboOperand(Operand)))
                        });
}

public class CdvInstruction(int operand) : NonJumpInstructionBase(operand)
{
    public override string Description => $"C = A / 2^combo({Operand})";
    public override int Opcode => 7;

    public override Machine Execute(Machine machine)
        => base.Execute(machine with
                        {
                            C = (int)(machine.A / Math.Pow(2, machine.EvaluateComboOperand(Operand)))
                        });
}
