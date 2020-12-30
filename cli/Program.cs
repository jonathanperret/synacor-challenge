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
    public readonly ushort[] Memory = new ushort[32775];
    public int Cycles { get; private set; }
    public TextWriter Output { get; set; } = new StringWriter();
    public ushort PC { get; private set; } = 0;
    public readonly Stack<ushort> Stack = new Stack<ushort>();

    public void Run()
    {
        while (true)
        {
            Cycles++;
            ushort opcode = Memory[PC];
            ushort a = Memory[PC + 1];
            ushort b = Memory[PC + 2];
            ushort c = Memory[PC + 3];
            // W($"PC={PC} op={opcode}");
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
                    Stack.Push(GetValue(a));
                    break;
                case 3: // pop a — remove the top element from the stack and write it into <a>; empty stack = error
                    PC += 2;
                    break;
                case 4: // eq a b c — set <a> to 1 if <b> is equal to <c>; set it to 0 otherwise
                    PC += 4;
                    break;
                case 5: // gt a b c — set <a> to 1 if <b> is greater than <c>; set it to 0 otherwise
                    PC += 4;
                    break;
                case 6: // jmp a — jump to <a>
                    PC = GetValue(a);
                    break;
                case 7: // jt a b — if <a> is nonzero, jump to <b>
                    PC += 3;
                    break;
                case 8: // jf a b — if <a> is zero, jump to <b>
                    PC += 3;
                    break;
                case 9: // add a b c — assign into <a> the sum of <b> and <c> (modulo 32768)
                    PC += 4;
                    break;
                case 10: // mult a b c — store into <a> the product of <b> and <c> (modulo 32768)
                    PC += 4;
                    break;
                case 11: // mod a b c — store into <a> the remainder of <b> divided by <c>
                    PC += 4;
                    break;
                case 12: // and a b c — stores into <a> the bitwise and of <b> and <c>
                    PC += 4;
                    break;
                case 13: // or a b c — stores into <a> the bitwise or of <b> and <c>
                    PC += 4;
                    break;
                case 14: // not a b — stores 15-bit bitwise inverse of <b> in <a>
                    PC += 3;
                    break;
                case 15: // rmem a b — read memory at address <b> and write it to <a>
                    PC += 3;
                    break;
                case 16: // wmem a b — write the value from <b> into memory at address <a>
                    PC += 3;
                    break;
                case 17: // call a — write the address of the next instruction to the stack and jump to <a>
                    PC += 2;
                    break;
                case 18: // ret — remove the top element from the stack and jump to it; empty stack = halt
                    PC += 1;
                    break;
                case 19: // out a — write the character represented by ascii code <a> to the terminal
                    Output.Write((char)GetValue(a));
                    PC += 2;
                    break;
                case 20: // in a — read a character from the terminal and write its ascii code to <a>; it can be assumed that once input starts, it will continue until a newline is encountered; this means that you can safely read whole lines from the keyboard and trust that they will be fully read
                    PC += 2;
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
    public static int Part1(string[] lines)
    {
        var groups = lines.Split("");

        return lines.Length;
    }

    static void Main(string[] args)
    {
        var bytes = File.ReadAllBytes(args[0]);
        var vm = new VM();
        for (int i = 0; i < bytes.Length - 1; i += 2)
        {
            ushort val = (ushort)(bytes[i] + (bytes[i + 1] << 8));
            vm.Memory[i / 2] = val;
        }
        vm.Output = Console.Out;
        vm.Run();
    }
}
