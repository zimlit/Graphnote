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
using Godot;

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
    Underscore,
    Caret,
    LBracket,
    RBracket,
    Pipe,
    Hash,
    TextSpan,
}

public class Token
{
    public TokenType Type;
    public string Value;
    public int Line;
    public int Column;

    public override string ToString()
    {
        return $"Token({Type}, {System.Text.RegularExpressions.Regex.Unescape(Value)}, {Line}, {Column})";
    }
}

public class LexError
{
    public int Line;
    public int Column;
    public string Message;
    public string LineText;

    public LexError(int line, int column, string message, string lineText)
    {
        Line = line;
        Column = column;
        Message = message;
        LineText = lineText;
    }

    public override string ToString()
    {
        string s = "";
        int lineNumberLength = Line.ToString().Length;

        s += $"{Line} | {LineText}\n";
        s += new string(' ', Column + lineNumberLength + 2);
        s += $"^ {Message}\n";

        return s;
    }
}

public class LexErrors : Exception
{
    List<LexError> errors;

    public LexErrors(List<LexError> errors) : base() { this.errors = errors; }

    public override string ToString()
    {
        string s = "";
        foreach (var error in errors)
        {
            s += error;
        }
        return s;
    }
}

public class Lexer
{
    string[] Source;
    int Index;
    int Line;

    public Lexer(string source)
    {
        Source = source.Split("\n");
        Index = 0;
        Line = 1;
    }

    private char Advance()
    {
        if (Line > Source.Length)
        {
            return '\0';
        }
        if (Line == Source.Length && Index > Source[Line - 1].Length)
        {
            return '\0';
        }
        if (Source[Line - 1].Length == Index)
        {
            Index++;
            return '\n';
        }
        if (Source[Line - 1].Length < Index)
        {
            Index = 0;
            Line++;
            if (Line >= Source.Length && Source[Line - 1].Length == 0)
            {
                return '\0';
            }
        }

        char c = Source[Line - 1][Index];

        Index++;

        return c;
    }

    private char Peek()
    {
        if (Line > Source.Length)
        {
            return '\0';
        }
        if (Line == Source.Length && Index > Source[Line - 1].Length)
        {
            return '\0';
        }
        if (Source[Line - 1].Length == Index)
        {
            return '\n';
        }
        if (Source[Line - 1].Length < Index)
        {
            return '\0';
        }

        return Source[Line - 1][Index];
    }

    public List<Token> Lex()
    {
        List<Token> tokens = new List<Token>();
        List<LexError> errors = new List<LexError>();

        String span = "";
        int spanStart = 1;

        void textSpan(int start = 1)
        {
            if (span.Length > 0)
            {
                tokens.Add(new Token() { Type = TokenType.TextSpan, Value = span, Line = Line, Column = spanStart });
                span = "";
                spanStart = 1;
            }
        }

        while (true)
        {
            char c = Advance();
            int column = Index;

            switch (c)
            {
                case '\n':
                    {
                        textSpan();
                        tokens.Add(new Token() { Type = TokenType.LineBreak, Value = "\n", Line = Line, Column = column });
                        break;
                    }
                case '\0':
                    {
                        textSpan();
                        if (errors.Count > 0)
                        {
                            throw new LexErrors(errors);
                        }
                        return tokens;
                    }
                case '\r':
                    {
                        break;
                    }
                case '=':
                    {
                        textSpan();
                        string value = "=";
                        int headerLevel = 1;
                        while (Peek() == '=')
                        {
                            value += "=";
                            Advance();
                            headerLevel++;
                        }
                        if (headerLevel > 6)
                        {
                            errors.Add(new LexError(Line, column, "Header level cannot be greater than 6", Source[Line - 1]));
                        }
                        tokens.Add(new Token() { Type = (TokenType)headerLevel + (int)TokenType.Header1 - 1, Value = value, Line = Line, Column = column });
                        break;
                    }
                case '-':
                    {
                        textSpan();
                        if (Peek() == '-')
                        {
                            Advance();
                            if (Peek() == '-')
                            {
                                Advance();
                                tokens.Add(new Token() { Type = TokenType.HRule, Value = "---", Line = Line, Column = column });
                                break;
                            }
                            else
                            {
                                span += "--";
                            }
                        }
                        else
                        {
                            span += c;
                        }

                        break;
                    }
                case ':':
                    {
                        textSpan();
                        tokens.Add(new Token() { Type = TokenType.Colon, Value = ":", Line = Line, Column = column });
                        break;
                    }
                case '{':
                    {
                        textSpan();
                        tokens.Add(new Token() { Type = TokenType.LBrace, Value = "{", Line = Line, Column = column });
                        break;
                    }
                case '}':
                    {
                        textSpan();
                        tokens.Add(new Token() { Type = TokenType.RBrace, Value = "}", Line = Line, Column = column });
                        break;
                    }
                case '*':
                    {
                        textSpan();
                        tokens.Add(new Token() { Type = TokenType.Star, Value = "*", Line = Line, Column = column });
                        break;
                    }
                case '_':
                    {
                        textSpan();
                        tokens.Add(new Token() { Type = TokenType.Underscore, Value = "_", Line = Line, Column = column });
                        break;
                    }
                case '^':
                    {
                        textSpan();
                        tokens.Add(new Token() { Type = TokenType.Caret, Value = "^", Line = Line, Column = column });
                        break;
                    }
                case '[':
                    {
                        textSpan();
                        tokens.Add(new Token() { Type = TokenType.LBracket, Value = "[", Line = Line, Column = column });
                        break;
                    }
                case ']':
                    {
                        textSpan();
                        tokens.Add(new Token() { Type = TokenType.RBracket, Value = "]", Line = Line, Column = column });
                        break;
                    }
                case '|':
                    {
                        textSpan();
                        tokens.Add(new Token() { Type = TokenType.Pipe, Value = "|", Line = Line, Column = column });
                        break;
                    }
                case '#':
                    {
                        textSpan();
                        tokens.Add(new Token() { Type = TokenType.Hash, Value = "#", Line = Line, Column = column });
                        break;
                    }
                default:
                    {
                        span += c;
                        break;
                    }
            }
        }
    }
}

