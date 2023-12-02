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

class AddNodeCommand : Command
{
	readonly string nodeName;
	readonly Graph graph;
	int id;

	public AddNodeCommand(Graph graph, string nodeName)
	{
		this.graph = graph;
		this.nodeName = nodeName;
	}

	public override void Execute()
	{
		id = graph.AddNode(nodeName).VertexId;
	}

	public override void Undo()
	{
		graph.RemoveNode(id);
	}
}

class AddEdgeCommand : Command
{
	readonly Graph graph;
	readonly int id1;
	readonly int id2;

	public AddEdgeCommand(Graph graph, int id1, int id2)
	{
		this.graph = graph;
		this.id1 = id1;
		this.id2 = id2;
	}

	public override void Execute()
	{
		graph.AddEdge(id1, id2);
	}

	public override void Undo()
	{
		graph.RemoveEdge(id1, id2);
	}
}

class DeleteNodeCommand : Command
{
	readonly Graph graph;
	readonly int id;
	readonly string name;

	public DeleteNodeCommand(Graph graph, int id)
	{
		this.graph = graph;
		this.id = id;
		name = graph.NodeAt(id).Name;
	}

	public override void Execute()
	{
		graph.RemoveNode(id);
	}

	public override void Undo()
	{
		graph.AddNode(name);
	}
}

class DeleteEdgeCommand : Command
{
	readonly Graph graph;
	readonly int id1;
	readonly int id2;

	public DeleteEdgeCommand(Graph graph, int id1, int id2)
	{
		this.graph = graph;
		this.id1 = id1;
		this.id2 = id2;
	}

	public override void Execute()
	{
		graph.RemoveEdge(id1, id2);
	}

	public override void Undo()
	{
		graph.AddEdge(id1, id2);
	}
}

public partial class NoteGraph : Control
{
	public Graph graph
	{ get; }
		= new Graph();

	Popup popup;
	LineEdit nodeName;
	GraphView graphView;

	CommandList commands = new CommandList();
	PackedScene noteView;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var button = GetNode<TextureButton>("HBoxContainer/TextureButton");
		popup = GetNode<Popup>("Popup");
		nodeName = GetNode<LineEdit>("Popup/ColorRect/VBoxContainer/LineEdit");
		var popupDone = GetNode<Button>("Popup/ColorRect/VBoxContainer/Button");
		popupDone.Pressed += OnPopupDone;
		graphView = GetNode<GraphView>("GraphView");
		graphView.EdgeAdded += OnEdgeAdded;
		graphView.NodeOpened += OnNodeOpened;

		button.Pressed += OnTextureButtonPressed;

		noteView = GD.Load<PackedScene>("res://NoteView.tscn");
	}

	private async void OnNodeOpened(int id)
	{
		NoteView view = noteView.Instantiate<NoteView>();
		view.NoteId = id;
		view.NoteClosed += OnNoteClosed;
		AddChild(view);
		await ToSignal(view.Animation, "animation_finished");
		graphView.Hide();
		GetNode<HBoxContainer>("HBoxContainer").Hide();
	}

	private void OnNoteClosed()
	{
		graphView.Show();
		GetNode<HBoxContainer>("HBoxContainer").Show();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnTextureButtonPressed()
	{
		popup.PopupCentered();
	}

	private void OnPopupDone()
	{
		popup.Hide();
		var command = new AddNodeCommand(graph, nodeName.Text);
		command.Execute();
		commands.AddCommand(command);
		nodeName.Text = "";
		graphView.QueueRedraw();
	}

	private void OnEdgeAdded(int id1, int id2)
	{
		var command = new AddEdgeCommand(graph, id1, id2);
		command.Execute();
		commands.AddCommand(command);
		graphView.QueueRedraw();
	}

	private void OnEdgeDeleted(int id1, int id2)
	{
		var command = new DeleteEdgeCommand(graph, id1, id2);
		command.Execute();
		commands.AddCommand(command);
		graphView.QueueRedraw();
	}

	private void OnNodeDeleted(int id)
	{
		var command = new DeleteNodeCommand(graph, id);
		command.Execute();
		commands.AddCommand(command);
		graphView.QueueRedraw();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (Input.IsActionJustPressed("redo"))
		{
			commands.Redo();
			graphView.QueueRedraw();
		}
		else if (Input.IsActionJustPressed("undo"))
		{
			commands.Undo();
			graphView.QueueRedraw();
		}
	}
}
