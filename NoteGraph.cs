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

public partial class NoteGraph : Control
{
	public Graph graph
	{ get; }
		= new Graph();

	Popup popup;
	LineEdit nodeName;
	GraphView graphView;

	CommandList commands = new CommandList();

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

		button.Pressed += OnTextureButtonPressed;
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

	public override void _Input(InputEvent @event)
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
