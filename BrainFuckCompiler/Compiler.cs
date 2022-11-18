using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BrainFuckCompiler;
internal class Compiler
{
    const int MaxElementSize = int.MaxValue;
    const int MinElementSize = 0;

    public Compiler(Stream input, Stream output, int arrayLength)
    {
        ArrayLength = arrayLength;
        _input = input;
        _output = output;
    }
    /// <summary>
    /// Represents Brainfuck array.
    /// </summary>
    private static int[] Array { get; set; }
    /// <summary>
    /// Length of Brainfuck array. 
    /// </summary>
    public int ArrayLength
    {
        get => arrayLength;
        private set => arrayLength = (value > 0) ? value : 3000;
    }
    
    /// <summary>
    /// Input compiler stream.
    /// </summary>
    private Stream _input;
    /// <summary>
    /// Output compiler stream.
    /// </summary>
    private Stream _output;
    private int currentElementIndex;
    private int arrayLength;
    /// <summary>
    /// Compiles source code uses initialized streams.
    /// </summary>
    /// <param name="sourceCode">Brainfuck source code.</param>
    public void Compile(string sourceCode)
    {
        TextIdentificate(sourceCode);

        Array = InitializeArray(ArrayLength);
        currentElementIndex = 0;

        InterpretBySym(sourceCode);
    }
    /// <summary>
    /// Interprets every symbols.
    /// </summary>
    /// <param name="symbols"></param>
    private void InterpretBySym(string symbols)
    {
        for (int symNum = 0; symNum < symbols.Length; symNum++)
            switch ((Commands)symbols[symNum])
            {
                case Commands.Next:
                    if (currentElementIndex < Array.Length - 1) currentElementIndex++;
                    break;
                case Commands.Previous:
                    if (currentElementIndex > 0) currentElementIndex--;
                    break;
                case Commands.Plus:
                    if (Array[currentElementIndex] < MaxElementSize) Array[currentElementIndex]++;
                    break;
                case Commands.Minus:
                    if (Array[currentElementIndex] > MinElementSize) Array[currentElementIndex]--;
                    break;
                case Commands.Out:
                    _output.WriteByte((byte)Array[currentElementIndex]);
                    break;
                case Commands.In:
                    Array[currentElementIndex] = _input.ReadByte();
                    break;
                case Commands.WhileStart:
                    if (Array[currentElementIndex] == 0)
                    {
                        int cycleEnds = 1;
                        while (cycleEnds > 0 && symNum < symbols.Length)
                        {
                            symNum++;
                            if (symbols[symNum] == (char)Commands.WhileEnd) cycleEnds--;
                            if (symbols[symNum] == (char)Commands.WhileStart) cycleEnds++;
                        }
                    }
                    break;
                case Commands.WhileEnd:
                    if (Array[currentElementIndex] > 0)
                    {
                        int cycleStarts = 1;
                        while (cycleStarts > 0 && symNum >= 0)
                        {
                            symNum--;
                            if (symbols[symNum] == (char)Commands.WhileEnd) cycleStarts++;
                            if (symbols[symNum] == (char)Commands.WhileStart) cycleStarts--;
                        }
                    }
                    break;
            }
    }
    /// <summary>
    /// Checks errors in code.
    /// </summary>
    /// <param name="code">Brainfuck source code.</param>
    /// <exception cref="CompilerException"></exception>
    private void TextIdentificate(string code)
    {
        (int cycleStart, int cycleEnd) = (0, 0);
        for (int i = 0; i < code.Length; i++)
        {
            switch ((Commands)code[i])
            {
                case Commands.WhileStart:
                    cycleStart++;
                    break;
                case Commands.WhileEnd:
                    cycleEnd++;
                    break;
            }
            if (cycleEnd > cycleStart) throw new CompilerException("Cycle was not opened before closeing.");
        }
        if (cycleEnd < cycleStart) throw new CompilerException("Cycle was not closed.");
    }
    /// <summary>
    /// Represents list of Brainfuck commands.
    /// </summary>
    private enum Commands
    {
        Next = '>',
        Previous = '<',
        Plus = '+',
        Minus = '-',
        Out = '.',
        In = ',',
        WhileStart = '[',
        WhileEnd = ']',
    }
    /// <summary>
    /// Initialize empty <see cref="Array{int}"/> of int with specified length.
    /// </summary>
    /// <param name="length">Length of returned array.</param>
    /// <returns></returns>
    private static int[] InitializeArray(int length)
    {
        var ar = new int[length];
        ar.Initialize();
        return ar;
    }
}
