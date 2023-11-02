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

public partial class NoteGraph : Control
{
	public Graph graph
	{ get; }
		= new Graph();

	Popup popup;
	LineEdit nodeName;

	List<Command> commands = new List<Command>();
	int commandPos = -1;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var button = GetNode<TextureButton>("HBoxContainer/TextureButton");
		popup = GetNode<Popup>("Popup");
		nodeName = GetNode<LineEdit>("Popup/ColorRect/VBoxContainer/LineEdit");
		var popupDone = GetNode<Button>("Popup/ColorRect/VBoxContainer/Button");
		popupDone.Pressed += OnPopupDone;

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
		commands.Add(command);
		commandPos++;
		nodeName.Text = "";
	}
}
