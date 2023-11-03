using System;
using System.Collections.Generic;

abstract class Command
{
    public abstract void Execute();
    public abstract void Undo();
}

class CommandList : List<Command>
{
    private int commandPos = -1;

    public void AddCommand(Command item)
    {
        if (commandPos < Count - 1)
        {
            RemoveRange(commandPos + 1, Count - commandPos - 1);
        }
        Add(item);
        commandPos++;
    }

    public void Undo()
    {
        if (commandPos >= 0)
        {
            this[commandPos].Undo();
            commandPos--;
        }
    }

    public void Redo()
    {
        if (commandPos < Count - 1)
        {
            commandPos++;
            this[commandPos].Execute();
        }
    }
}