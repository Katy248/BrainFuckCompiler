using System;
using System.Collections.Generic;
using System.Linq;

namespace BrainFuckCompiler;

public class Parser
{
    public Parser()
    {

    }

    public IEnumerable<Commands> Parse(Stream source)
    {
        int symbol;
        do
        {
            symbol = source.ReadByte();
            switch ((Commands)symbol)
            {
                case Commands.Next:
                case Commands.Previous:
                case Commands.Plus:
                case Commands.Minus:
                case Commands.Out:
                case Commands.In:
                case Commands.WhileStart:
                case Commands.WhileEnd:
                    yield return (Commands)symbol;
                    break;

                default:
                    break;
            }
        }
        while (source.CanRead && symbol != -1);
    }
}
