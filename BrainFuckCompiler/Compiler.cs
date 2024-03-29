using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BrainFuckCompiler;
public class OutputInfo
{
    public string Name { get; init; }
    public FileInfo OutputFile { get; init; }

    public static OutputInfo FromFile(FileInfo outputFile)
    {
        return new OutputInfo
        {
            OutputFile = outputFile,
            Name = outputFile.Name
        };
    }
}
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


    /* private ILGenerator GetGenerator(OutputInfo info)
    {
        

        return methodBuilder.GetIlGenerator();
    } */

    /// <summary>
    /// Compiles source code uses initialized streams.
    /// </summary>
    /// <param name="sourceCode">Brainfuck source code.</param>
    public void Compile(Stream sourceCode, OutputInfo outputInfo)
    {
        var assemblyName = new AssemblyName(outputInfo.Name);
        var domain = System.Threading.Thread.GetDomain();
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(outputInfo.OutputFile.FullName);
        var typeBuilder = moduleBuilder.DefineType("Program", TypeAttributes.Public | TypeAttributes.Class);
        var methodBuilder = typeBuilder.DefineMethod("Main", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(object), new Type[] { });

        Emit(sourceCode, methodBuilder.GetILGenerator());

        // var type = typeBuilder.CreateType();
        assemblyBuilder.Save(outputInfo.OutputFile);

    }
    public void Emit(Stream sourceCode, ILGenerator il)
    {
        var parser = new Parser();

        /* foreach (var command in parser.Parse(sourceCode))
        {

            switch (command)
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
                        while (cycleEnds > 0 && i < commands.Length)
                        {
                            i++;
                            if (commands[i] == Commands.WhileEnd) cycleEnds--;
                            if (commands[i] == Commands.WhileStart) cycleEnds++;
                        }
                    }
                    break;
                case Commands.WhileEnd:
                    if (Array[currentElementIndex] > 0)
                    {
                        int cycleStarts = 1;
                        while (cycleStarts > 0 && i >= 0)
                        {
                            i--;
                            if (commands[i] == Commands.WhileEnd) cycleStarts++;
                            if (commands[i] == Commands.WhileStart) cycleStarts--;
                        }
                    }
                    break;
            }

        } */
        il.Emit(OpCodes.Ldc_I4_S, 2);
        il.Emit(OpCodes.Call, typeof(Console).GetMethod(
            "Write", new Type[] { typeof(int) }));
        il.Emit(OpCodes.Ret);
    }
    /// <summary>
    /// Interprets every symbols.
    /// </summary>
    /// <param name="sourceCode"></param>
    public void Interpret(Stream sourceCode)
    {
        //TextValidate(sourceCode);

        Array = InitializeArray(ArrayLength);
        currentElementIndex = 0;

        var parser = new Parser();

        var commands = parser.Parse(sourceCode).ToArray();
        for (uint i = 0; i < commands.Length; i++)
        {
            var symbol = commands[i];
            switch (symbol)
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
                        while (cycleEnds > 0 && i < commands.Length)
                        {
                            i++;
                            if (commands[i] == Commands.WhileEnd) cycleEnds--;
                            if (commands[i] == Commands.WhileStart) cycleEnds++;
                        }
                    }
                    break;
                case Commands.WhileEnd:
                    if (Array[currentElementIndex] > 0)
                    {
                        int cycleStarts = 1;
                        while (cycleStarts > 0 && i >= 0)
                        {
                            i--;
                            if (commands[i] == Commands.WhileEnd) cycleStarts++;
                            if (commands[i] == Commands.WhileStart) cycleStarts--;
                        }
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Checks errors in code.
    /// </summary>
    /// <param name="code">Brainfuck source code.</param>
    /// <exception cref="CompilerException"></exception>
    private void TextValidate(string code)
    {
        var stack = new Stack<char>();

        for (int i = 0; i < code.Length; i++)
        {
            switch (code[i])
            {
                case (char)Commands.WhileStart:
                    stack.Push(code[i]);
                    break;
                case (char)Commands.WhileEnd:
                    if (stack.Count == 0)
                        throw new CompilerException("Cycle was not opened before closing.");
                    stack.Pop();
                    break;
            }
        }
        if (stack.Count > 0)
            throw new CompilerException("Cycle was not closed.");
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
