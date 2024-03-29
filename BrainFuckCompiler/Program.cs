using BrainFuckCompiler;
using System.CommandLine;
using System.ComponentModel;
using System.Reflection;
using System.IO;

var fileArg = new Argument<FileInfo>("file", description: "File with source code to run");
var arrayLengthArg = new Argument<uint>("array length", description: "Length of array", getDefaultValue: () => 3000);

var outputOption = new Option<FileInfo>(["--output", "-o"], description: "Specify output file", getDefaultValue: () => new FileInfo("out.dll"));

var interpretCommand = new Command("interpret", "Just interpret code") { fileArg, arrayLengthArg };
var compileCommand = new Command("compile", "Compile code to .NET binary dll") { fileArg, arrayLengthArg, outputOption };

var root = new RootCommand("BrainFuckCompiler is not a real compiler but an interpreter for brainfuck language")
{
    interpretCommand, compileCommand
};
interpretCommand.SetHandler((file, arrayLength) =>
{
    try
    {
        using var sourceCode = File.Open(file.FullName, FileMode.Open);
        var compiler = new Compiler(Console.OpenStandardInput(), Console.OpenStandardOutput(), (int)arrayLength);
        compiler.Interpret(sourceCode);
    }
    catch (CompilerException ce)
    {
        Console.Error.WriteLine($"BFCompiler error: {ce.Message}");
    }
    catch (Exception e)
    {
        Console.Error.WriteLine("Internal error");
        Console.Error.WriteLine(e.Message);
        throw;
    }
}, fileArg, arrayLengthArg);

compileCommand.SetHandler((file, arrayLength, output) =>
{
    try
    {
        using var sourceCode = File.Open(file.FullName, FileMode.Open);
        var compiler = new Compiler(Console.OpenStandardInput(), Console.OpenStandardOutput(), (int)arrayLength);
        compiler.Compile(sourceCode, OutputInfo.FromFile(output));
    }
    catch (CompilerException ce)
    {
        Console.Error.WriteLine($"BFCompiler error: {ce.Message}");
    }
    catch (Exception e)
    {
        Console.Error.WriteLine("Internal error");
        Console.Error.WriteLine(e.Message);
        throw;
    }
}, fileArg, arrayLengthArg, outputOption);

await root.InvokeAsync(args);
