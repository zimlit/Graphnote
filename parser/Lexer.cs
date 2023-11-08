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

public enum TokenType
{
    Header1,
    Header2,
    Header3,
    Header4,
    Header5,
    Header6,
    HRule,
    LineBreak,
    Colon,
    LBrace,
    RBrace,
    Star,
    Hash,
    Poem,
    Underscore,
    SingleQuote,
    Code,
    Small,
    Big,
    Caret,
    LBracket,
    RBracket,
    Pipe,
    TextSpan,
}

public class Token
{
    public TokenType Type;
    public string Value;
    public int Line;
    public int Column;
}

public class LexError : Exception
{
    public LexError() : base() { }
}

public class Lexer
{
    string[] Source;
    int Index;
    int Line;
    int Column;

    public Lexer(string source)
    {
        Source = source.Split("\n");
        Index = 0;
        Line = 1;
        Column = 1;
    }
}
