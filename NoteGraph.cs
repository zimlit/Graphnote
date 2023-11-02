using Godot;
using System;

public partial class NoteGraph : Control
{
	int nodeCount = 0;
	public Graph graph 
		{ get; } 
		= new Graph();
		
	Popup popup;
	LineEdit nodeName;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var button = GetNode<TextureButton>("HBoxContainer/TextureButton");
		popup = GetNode<Popup>("Popup");
		nodeName = GetNode<LineEdit>("Popup/ColorRect/VBoxContainer/LineEdit");
		var popupDone = GetNode<Button>("Popup/ColorRect/VBoxContainer/Button");
		popupDone.Pressed += OnPopupDone;

		button.Pressed += OnTextureButtonPressed;
		graph.AddNode("Node 1");
		graph.AddNode("Node 2");
		graph.AddNode("Node 3");
		graph.AddEdge(0, 1);
		graph.AddEdge(0, 2);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnTextureButtonPressed()
	{
		popup.PopupCentered();
	}

	private void OnPopupDone() {
		popup.Hide();
		graph.AddNode(nodeName.Text);
		nodeCount++;
	}
}
