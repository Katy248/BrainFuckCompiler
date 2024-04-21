using System.Reflection;
using System.Reflection.Emit;
using Lokad.ILPack;

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
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        var typeBuilder = moduleBuilder.DefineType("Program", TypeAttributes.AutoClass | TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed);
        var methodBuilder = typeBuilder.DefineMethod("Main", MethodAttributes.Static | MethodAttributes.Public, typeof(int), new Type[] { });

        Emit(sourceCode, methodBuilder.GetILGenerator());
        typeBuilder.CreateTypeInfo();
        
        var generator = new AssemblyGenerator();
        var bytes= generator.GenerateAssemblyBytes(assemblyBuilder, [Assembly.Load(new AssemblyName("System"))]);
    }
    public static void Emit(Stream sourceCode, ILGenerator il)
    {
        var loops = new Stack<int>();
        il.EmitWriteLine(".locals init ([0] int32 index, [1] int32[] array)");
        il.Emit(OpCodes.Ldc_I4_1, 3000);
        il.Emit(OpCodes.Newarr, typeof(int));
        il.Emit(OpCodes.Stloc_1);

        var parser = new Parser();

        foreach (var command in parser.Parse(sourceCode))
        {

            switch (command)
            {
                case Commands.Next:
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Add);
                    il.Emit(OpCodes.Stloc_0);
                    break;
                case Commands.Previous:
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Sub);
                    il.Emit(OpCodes.Stloc_0);
                    break;
                case Commands.Plus:
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Ldelema);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Add);
                    il.Emit(OpCodes.Stelem_I4);
                    break;
                case Commands.Minus:
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Ldelema);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Sub);
                    il.Emit(OpCodes.Stelem_I4);
                    break;
                case Commands.Out:
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Ldelema);
                    il.EmitWriteLine("call void [System.Console]System.Console::Write(int32)");
                    break;
                case Commands.In:
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldloc_0);
                    il.EmitWriteLine("call int32 [System.Console]System.Console::Read()");
                    il.Emit(OpCodes.Stelem_I4);
                    break;
                case Commands.WhileStart:
                    loops.Push(loops.Count);

                    il.EmitWriteLine($"START_LOOP_{loops.Peek()}:");
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Ldelema);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Beq_S, $"END_LOOP_{loops.Peek()}");
                    break;
                case Commands.WhileEnd:
                    il.Emit(OpCodes.Br_S, $"START_LOOP_{loops.Peek()}");
                    il.EmitWriteLine($"END_LOOP_{loops.Pop()}:");

                    break;
            }


        }
        /*
        il.Emit(OpCodes.Ldc_I4_S, 2);
        il.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", new Type[] { typeof(int) }));
        */
        il.Emit(OpCodes.Ldc_I4_0);
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
                    if (currentElementIndex < Array.Length - 1)
                        currentElementIndex++;
                    break;
                case Commands.Previous:
                    if (currentElementIndex > 0)
                        currentElementIndex--;
                    break;
                case Commands.Plus:
                    if (Array[currentElementIndex] < MaxElementSize)
                        Array[currentElementIndex]++;
                    break;
                case Commands.Minus:
                    if (Array[currentElementIndex] > MinElementSize)
                        Array[currentElementIndex]--;
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
                            if (commands[i] == Commands.WhileEnd)
                                cycleEnds--;
                            if (commands[i] == Commands.WhileStart)
                                cycleEnds++;
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
                            if (commands[i] == Commands.WhileEnd)
                                cycleStarts++;
                            if (commands[i] == Commands.WhileStart)
                                cycleStarts--;
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
