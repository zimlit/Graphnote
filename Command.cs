// Copyright (C) 2023 Devin Rockwell
// 
// This file is part of Graphnote.
// 
// Graphnote is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Graphnote is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Graphnote.  If not, see <http://www.gnu.org/licenses/>.

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