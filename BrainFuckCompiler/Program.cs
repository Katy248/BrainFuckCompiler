using System.Reflection;
using BrainFuckCompiler;

const string Help = "\nUse: [filename] [array-length]";

if (args.Length == 0)
{
    Console.WriteLine($"{Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version}");
    Console.WriteLine(Help);
    return;
}


try
{
    string sourceCode = File.ReadAllText(args[0]);
    int length = ParseLength();

    var compiler = new Compiler(Console.OpenStandardInput(), Console.OpenStandardOutput(), length);
    compiler.Compile(sourceCode);
}
catch(CompilerException ce)
{
    Console.WriteLine($"BFCompiler error: {ce.Message}");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}


int ParseLength()
{
    if (args.Length>1)
        if (int.TryParse(args[1], out int result))
            return result;
    return 3000;
}
