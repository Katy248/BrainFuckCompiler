using BrainFuckCompiler;
using System.CommandLine;
using System.ComponentModel;
using System.Reflection;

var fileArg = new Argument<FileInfo>("file", description: "File with source code to run");
var arrayLengthArg = new Argument<uint>("array length", description: "Length of array", getDefaultValue: () => 3000);

var root = new RootCommand("BrainFuckCompiler is not a real compiler but an interpreter for brainfuck language")
{
    fileArg, arrayLengthArg
};
root.SetHandler((file, arrayLength) =>
{
    try
    {
        var sourceCode = File.ReadAllText(file.FullName);
        var compiler = new Compiler(Console.OpenStandardInput(), Console.OpenStandardOutput(), (int)arrayLength);
        compiler.Compile(sourceCode);
    }
    catch(CompilerException ce)
    {
        Console.Error.WriteLine($"BFCompiler error: {ce.Message}");
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e.Message);
        throw;
    }
}, fileArg, arrayLengthArg);

await root.InvokeAsync(args);