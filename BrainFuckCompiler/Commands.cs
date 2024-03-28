namespace BrainFuckCompiler;
/// <summary>
/// Represents list of Brainfuck commands.
/// </summary>
public enum Commands
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
