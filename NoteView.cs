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

using Godot;
using System;
using System.Collections.Generic;

public partial class NoteView : Control
{
	public int NoteId
	{ get; set; }
	NoteGraph parent;
	CodeEdit editor;
	TextEdit output;
	public AnimationPlayer Animation;

	[Signal]
	public delegate void NoteClosedEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		editor = GetNode<CodeEdit>("VBoxContainer/HBoxContainer/Editor");
		parent = GetParent<NoteGraph>();
		editor.Text = parent.graph.NodeAt(NoteId).Markup;
		editor.TextChanged += OnTextChanged;
		output = GetNode<TextEdit>("VBoxContainer/HBoxContainer/Output");
		output.Text = editor.Text;

		Animation = GetNode<AnimationPlayer>("AnimationPlayer");
		Animation.Play("Open");

		var exitButton = GetNode<TextureButton>("VBoxContainer/Titlebar/Exit");
		exitButton.Pressed += OnClose;
		var title = GetNode<Label>("VBoxContainer/Titlebar/Title");
		title.Text = parent.graph.NodeAt(NoteId).Name;
	}

	private void OnTextChanged()
	{
		parent.graph.NodeAt(NoteId).Markup = editor.Text;
		output.Text = "";
		Lexer lexer = new Lexer(editor.Text);
		try
		{
			List<Token> tokens = lexer.Lex();
			foreach (var token in tokens)
			{
				output.Text += token.ToString() + "\n";
			}
		}
		catch (LexErrors errors)
		{
			output.Text = errors.ToString();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private async void OnClose()
	{
		Animation.Play("Close");
		EmitSignal(SignalName.NoteClosed);
		await ToSignal(Animation, "animation_finished");
		QueueFree();
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionPressed("exit"))
		{
			OnClose();
		}
	}
}
