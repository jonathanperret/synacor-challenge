using System;
using System.IO;
using System.Linq;
using Sprache;
using System.Numerics;
using MoreLinq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using static Util;

public class VM
{
    public readonly ushort[] Memory = new ushort[32776];
    public int Cycles { get; private set; }
    public TextWriter Output { get; set; } = new StringWriter();
    public TextReader Input { get; set; } = new StringReader("");
    public ushort PC { get; private set; } = 0;
    public readonly Stack<ushort> Stack = new Stack<ushort>();

    public string currentLine;

    public TextWriter LogFile = new StringWriter();

    private byte[] GetMemoryBytes()
    {
        byte[] membytes = new byte[Memory.Length * 2];
        Buffer.BlockCopy(Memory, 0, membytes, 0, membytes.Length);
        return membytes;
    }

    private void Log(string s)
    {
        LogFile.WriteLine(s);
        // LogFile.Flush();
    }

    public void Run()
    {
        while (true)
        {
            Cycles++;
            ushort opcode = Memory[PC];
            ushort a = Memory[PC + 1];
            ushort b = Memory[PC + 2];
            ushort c = Memory[PC + 3];
            Log($"PC={PC} op={opcode} a={a} b={b} c={c}");
            switch (opcode)
            {
                case 0: // halt — stop execution and terminate the program
                    PC += 1;
                    return;
                case 1: // set a b — set register <a> to the value of <b>
                    PC += 3;
                    Memory[a] = GetValue(b);
                    break;
                case 2: // push a — push <a> onto the stack
                    PC += 2;
                    Log($"pushing {GetValue(a)}");
                    Stack.Push(GetValue(a));
                    break;
                case 3: // pop a — remove the top element from the stack and write it into <a>; empty stack = error
                    PC += 2;
                    Log($"popping");
                    Memory[a] = Stack.Pop();
                    break;
                case 4: // eq a b c — set <a> to 1 if <b> is equal to <c>; set it to 0 otherwise
                    PC += 4;
                    Memory[a] = (GetValue(b) == GetValue(c)) ? 1 : 0;
                    break;
                case 5: // gt a b c — set <a> to 1 if <b> is greater than <c>; set it to 0 otherwise
                    PC += 4;
                    Memory[a] = (GetValue(b) > GetValue(c)) ? 1 : 0;
                    break;
                case 6: // jmp a — jump to <a>
                    PC = GetValue(a);
                    break;
                case 7: // jt a b — if <a> is nonzero, jump to <b>
                    PC += 3;
                    if (GetValue(a) != 0) PC = GetValue(b);
                    break;
                case 8: // jf a b — if <a> is zero, jump to <b>
                    PC += 3;
                    if (GetValue(a) == 0) PC = GetValue(b);
                    break;
                case 9: // add a b c — assign into <a> the sum of <b> and <c> (modulo 32768)
                    PC += 4;
                    Memory[a] = (ushort)((GetValue(b) + GetValue(c)) & 0x7fff);
                    break;
                case 10: // mult a b c — store into <a> the product of <b> and <c> (modulo 32768)
                    PC += 4;
                    Memory[a] = (ushort)((GetValue(b) * GetValue(c)) & 0x7fff);
                    break;
                case 11: // mod a b c — store into <a> the remainder of <b> divided by <c>
                    PC += 4;
                    Memory[a] = (ushort)((GetValue(b) % GetValue(c)) & 0x7fff);
                    break;
                case 12: // and a b c — stores into <a> the bitwise and of <b> and <c>
                    PC += 4;
                    Memory[a] = (ushort)((GetValue(b) & GetValue(c)) & 0x7fff);
                    break;
                case 13: // or a b c — stores into <a> the bitwise or of <b> and <c>
                    PC += 4;
                    Memory[a] = (ushort)((GetValue(b) | GetValue(c)) & 0x7fff);
                    break;
                case 14: // not a b — stores 15-bit bitwise inverse of <b> in <a>
                    PC += 3;
                    Memory[a] = (ushort)((~GetValue(b)) & 0x7fff);
                    break;
                case 15: // rmem a b — read memory at address <b> and write it to <a>
                    PC += 3;
                    Memory[a] = Memory[GetValue(b)];
                    break;
                case 16: // wmem a b — write the value from <b> into memory at address <a>
                    PC += 3;
                    LogFile.WriteLine($"W {GetValue(a)} {GetValue(b)} {(char)(GetValue(b))}");
                    LogFile.Flush();
                    Memory[GetValue(a)] = GetValue(b);
                    break;
                case 17: // call a — write the address of the next instruction to the stack and jump to <a>
                    PC += 2;
                    Log($"pushing {PC}");
                    Stack.Push(PC);
                    PC = GetValue(a);
                    break;
                case 18: // ret — remove the top element from the stack and jump to it; empty stack = halt
                    PC += 1;
                    if (Stack.Count == 0) return;
                    Log($"popping");
                    PC = Stack.Pop();
                    break;
                case 19: // out a — write the character represented by ascii code <a> to the terminal
                    Output.Write((char)GetValue(a));
                    PC += 2;
                    break;
                case 20: // in a — read a character from the terminal and write its ascii code to <a>; it can be assumed that once input starts, it will continue until a newline is encountered; this means that you can safely read whole lines from the keyboard and trust that they will be fully read
                    PC += 2;
                    if (string.IsNullOrEmpty(currentLine))
                    {
                        bool again = true;
                        while (again)
                        {
                            again = false;
                            currentLine = Input.ReadLine();
                            W(currentLine);

                            switch (currentLine.Split(" ")[0])
                            {
                                case "!halt":
                                    return;
                                case "!dump":
                                    again = true;
                                    W("dumping memory…");
                                    File.WriteAllBytes("mem.bin", GetMemoryBytes());
                                    break;
                                case "!save":
                                    again = true;
                                    W("saving state…");
                                    using (BinaryWriter writer = new BinaryWriter(File.OpenWrite("state.bin")))
                                    {
                                        writer.Write(PC);
                                        writer.Write(Stack.Count);
                                        foreach (var x in Stack.ToArray().Reverse())
                                        {
                                            writer.Write(x);
                                        }
                                        writer.Write(GetMemoryBytes());
                                    }
                                    break;
                                case "!load":
                                    again = true;
                                    W("loading state…");
                                    using (BinaryReader reader = new BinaryReader(File.OpenRead("state.bin")))
                                    {
                                        PC = reader.ReadUInt16();
                                        Stack.Clear();
                                        int stackCount = reader.ReadInt32();
                                        for (int i = 0; i < stackCount; i++)
                                        {
                                            Stack.Push(reader.ReadUInt16());
                                        }
                                        Buffer.BlockCopy(reader.ReadBytes(Memory.Length * 2), 0, Memory, 0, Memory.Length * 2);
                                    }
                                    break;
                                case "!set":
                                    {
                                        again = true;
                                        var addr = ushort.Parse(currentLine.Split(" ")[1]);
                                        var x = ushort.Parse(currentLine.Split(" ")[2]);
                                        W($"setting {addr} to {x}");
                                        Memory[addr] = x;
                                        break;
                                    }
                                default:
                                    currentLine += "\n";
                                    break;
                            }
                        }
                    }

                    Memory[a] = (ushort)currentLine[0];
                    currentLine = currentLine[1..];
                    break;
                case 21: // noop — no operation
                    PC += 1;
                    break;
            }
        }
    }

    private ushort GetValue(ushort b)
    {
        if (b > 32767)
        {
            return Memory[b];
        }
        else
        {
            return b;
        }
    }
}

public static class Program
{
    static ushort r7 = 1;

    private static Dictionary<(ushort, ushort), ushort> memo = new();

    private static ushort ack(ushort x, ushort y)
    {
        if (memo.TryGetValue((x, y), out var cached))
        {
            return cached;
        }
        ushort result;
        if (x == 0)
        {
            result = (ushort)((y + 1) & 0x7fff);
        }
        else if (y == 0)
        {
            result = ack((ushort)(x - 1), r7);
        }
        else
        {
            result = (ushort)(ack((ushort)(x - 1), ack(x, (ushort)(y - 1))));
        }
        memo[(x, y)] = result;
        return result;
    }

    static ushort ackResult = 0;

    private static void DoIt()
    {
        ackResult = ack(4, 1);
    }

    private static void Find()
    {
        for (ushort i = 1; i < 32768; i++)
        {
            Console.WriteLine($"{i}");

            r7 = i;
            memo.Clear();
            ackResult = ack(4, 1);

            Console.WriteLine($"-> {ackResult}");
            if (ackResult == 6) return;
        }
    }

    private static string[][] matrix = {
        new[]{"22", "-", "9",  "*"},
        new[]{"+",  "4", "-",  "18"},
        new[]{"4",  "*", "11", "*"},
        new[]{"*",  "8", "-",  "1"},
    };

    static Dictionary<(int, int, int), string> vaultMemo = new();

    static readonly (int, int, string)[] deltas = new[] { (-1, 0, "w"), (1, 0, "e"), (0, -1, "s"), (0, 1, "n") };

    private static void VaultWalk(int x, int y, int weight, string op, string path)
    {
        // W($"Walking into ({x},{y}) with weight {weight} op {op} path {path.Length}:{path.Substring(0, Math.Min(path.Length, 30))}");

        if (x == 0 && y == 0 && path.Length > 0)
        {
            //W("can't return to start");
            return;
        }

        if (path.Length > 40)
        {
            // W($"path too long, bored");
            return;
        }

        int tile = int.Parse(matrix[y][x]);

        int newWeight = op switch
        {
            "*" => weight * tile,
            "-" => weight - tile,
            "+" => weight + tile,
            _ => throw new Exception($"bad op {op}")
        };

        // W($"new weight is {weight} {op} {tile} = {newWeight}");


        if (newWeight < 0 || newWeight > 300) return;

        if (vaultMemo.TryGetValue((x, y, newWeight), out var bestPath))
        {
            if (bestPath.Length < path.Length)
            {
                // already know how to reach this weight here with a shortest path
                // W($"aborting path to {(x, y, newWeight)}");
                return;
            }
        }

        vaultMemo[(x, y, newWeight)] = path;
        // W($"new best path for {(x, y, newWeight)} is {path.Length}");

        if (x == 3 && y == 3)
        {
            if (newWeight == 30)
            {
                vaultMemo[(x, y, newWeight)] = path;
                W($"solution path is {path}");
            }
            else
            {
                // W("can't go to door without 30");
            }
            return;
        }

        foreach (var (dxop, dyop, dirop) in deltas)
        {
            int xop = x + dxop, yop = y + dyop;
            if (xop < 0 || xop > 3 || yop < 0 || yop > 3) continue;
            string newOp = matrix[yop][xop];
            // W($"via {(xop, yop)}");
            foreach (var (dx, dy, dir) in deltas)
            {
                int newx = xop + dx, newy = yop + dy;
                if (newx < 0 || newx > 3 || newy < 0 || newy > 3) continue;
                VaultWalk(newx, newy, newWeight, newOp, path + dirop + dir);
            }
        }
    }

    static void Vault()
    {
        VaultWalk(0, 0, 1, "*", "");
        string path = vaultMemo[(3, 3, 30)];
        W($"best path to (3,3,30) is {path}");
        W(path.Select(c => c switch
        {
            'n' => "north",
            'e' => "east",
            'w' => "west",
            's' => "south",
            _ => ""
        }).ToDelimitedString("\n"));
    }

    static void Main(string[] args)
    {
        RunVM(args);
        // Find();
        // Vault();
        // Mirror();
    }

    private static void Mirror()
    {
        var code = "boibTqlMIVul".Reverse().Select(c => c switch
        {
            'd' => 'b',
            'b' => 'd',
            'p' => 'q',
            'q' => 'p',
            _ => c
        }).ToDelimitedString("");
        W($"{code}");
    }

    private static void RunVM(string[] args)
    {
        var bytes = File.ReadAllBytes(args.Length > 0 ? args[0] : "../challenge.bin");
        var vm = new VM();
        for (int i = 0; i < bytes.Length - 1; i += 2)
        {
            ushort val = (ushort)(bytes[i] + (bytes[i + 1] << 8));
            vm.Memory[i / 2] = val;
        }
        vm.Output = Console.Out;
        vm.Input = Console.In;
        vm.LogFile = new StreamWriter(File.Open("log.txt", FileMode.Create));
        vm.Run();
        vm.LogFile.Flush();
    }
}
