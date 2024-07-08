namespace BrainFuckCompiler.Generators;

public class PlainILGenerator
{
    private readonly Stream _output;
    private readonly IEnumerable<Commands> _commands;

    private readonly StreamWriter _streamWriter;
    public PlainILGenerator(Stream output, IEnumerable<Commands> commands)
    {
        _output = output;
        _commands = commands;
        _streamWriter = new StreamWriter(output);
    }

    private static string GenStart()
    {
        return @".locals init ([0] int32 index, [1] int32[] array) // allocate index, should be zero by default
ldc.i4.s 3000 // load array size to stack
newarr int32 // allocate array
stlock.1 // move array to its variable";
    }
    private static string GenNext()
    {
        return "ldloc.0 // load index variable to stack\n" +
        "ldc.i4.1 // load 1 to stack\n" +
        "add // increment\n" +
        "stloc.0 // load stack value to index variable";
    }
    private static string GenPrevious()
    {
        return "ldloc.0 // load index variable to stack\n" +
        "ldc.i4.1 // load 1 to stack\n" +
        "sub // increment\n" +
        "stloc.0 // load stack value to index variable";
    }
    private static string GenIncrement()
    {
        return @"ldloc.1 // allocate array
ldloc.0 // load index variable to stack
ldloc.1 // allocate array
ldloc.0 // load index variable to stack
ldelema // array[index] pointer to stack
ldc.i4.1 // load 1 to stack
add // increment
stelem.i4 // move stack value to array[index]";
    }
    private static string GenDecrement()
    {
        return @"ldloc.1 // allocate array
ldloc.0 // load index variable to stack
ldloc.1 // allocate array
ldloc.0 // load index variable to stack
ldelema // array[index] pointer to stack
ldc.i4.1 // load 1 to stack
sub // increment
stelem.i4 // move stack value to array[index]";
    }
    public void Generate()
    {
        _streamWriter.WriteLine(GenStart());
        foreach (var command in _commands)
        {
            switch (command)
            {
                case Commands.Next:
                    _streamWriter.WriteLine(GenNext());
                    break;
                case Commands.Previous:
                    _streamWriter.WriteLine(GenPrevious());
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
    }

}
