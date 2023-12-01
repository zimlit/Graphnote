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
	Hash,
	Underscore,
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

	public override string ToString()
	{
		return $"Token({Type}, {System.Text.RegularExpressions.Regex.Unescape(Value)}, {Line}, {Column})";
	}
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

	public List<Token> Lex()
	{
		List<Token> tokens = new List<Token>();

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
						return tokens;
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

