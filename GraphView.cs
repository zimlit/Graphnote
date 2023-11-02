using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class GraphView : Control
{
	private Graph graph;
	private Vector2 size;
	
	// Editor stuff
	public readonly List<int> selected = new List<int>();
	int dragging = -1;

	readonly Color nodeColor = new Color(0.1f, 0.2f, 0.5f);
	readonly Color selectedNodeColor = new Color(0.3f, 0.5f, 0.7f);
	
	// drawing stuff
	double radius = 100;
	double step;
	float nodeRadius = 10;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var popupDone = GetNode<Button>("../Popup/ColorRect/VBoxContainer/Button");
		popupDone.Pressed += OnPopupDone;
		var noteGraph = GetParent<NoteGraph>();
		graph = noteGraph.graph;
		size = GetViewportRect().Size;
		noteGraph.Resized += OnResize;
	}

    private void OnPopupDone()
	{
		QueueRedraw();
	}

	private void OnResize()
	{
		size = GetViewportRect().Size;
		QueueRedraw();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (dragging != -1) {
			QueueRedraw();
		}
	}

	public override void _Draw()
	{
		List<Vector2> nodesToDraw = new List<Vector2>();

		step = Math.Tau / graph.VertexCount;
		while (Math.Tau * radius / 3 < graph.VertexCount * 20)
			radius *= 2;

		if (dragging != -1) {
			var mousePos = GetGlobalMousePosition();
			mousePos.Y -= 48;
			var nodePos = new Vector2(
				(float)(radius * Math.Cos(step * dragging) + size.X / 2), 
				(float)(radius * Math.Sin(step * dragging) + size.Y / 2)
			);
			DrawLine(nodePos, mousePos, Colors.White, antialiased: true);
		}
		
		foreach (var node in graph.vertexSet)
		{
			var nodePos = new Vector2(
				(float)(radius * Math.Cos(step * node.VertexId) + size.X / 2), 
				(float)(radius * Math.Sin(step * node.VertexId) + size.Y / 2)
			);
			nodesToDraw.Add(nodePos);


			GD.Print(node.Name + " " + nodePos);
			foreach (var edge in node.AdjacencySet) {
				DrawLine(
					nodePos, 
					new Vector2(
						(float)(radius * Math.Cos(step * edge) + size.X / 2), 
						(float)(radius * Math.Sin(step * edge) + size.Y / 2)
					), 
					Colors.White, 
					antialiased: true
				);
			}
		}

		foreach (var node in nodesToDraw)
		{
			var color = selected.Contains(nodesToDraw.IndexOf(node)) ? selectedNodeColor : nodeColor;
			DrawCircle(node, nodeRadius, color);
		}		
	}

    public override void _Input(InputEvent @event)
    {
		if (Input.IsActionPressed("click")) {
			var mousePos = GetGlobalMousePosition();
			mousePos.Y -= 48;
			foreach (var node in graph.vertexSet)
			{
				var nodePos = new Vector2(
					(float)(radius * Math.Cos(step * node.VertexId) + size.X / 2), 
					(float)(radius * Math.Sin(step * node.VertexId) + size.Y / 2)
				);
				if (nodePos.DistanceTo(mousePos) < nodeRadius) {
					if (dragging == -1)
						dragging = node.VertexId;
					if (Input.IsKeyPressed(Key.Shift))
						if (selected.Contains(node.VertexId))
							selected.Remove(node.VertexId);
						else
							selected.Add(node.VertexId);
					else {
						selected.Clear();
						selected.Add(node.VertexId);
					}
					QueueRedraw();
					break;
				}
			}
		}
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && !mouseEvent.Pressed)  {
			var mousePos = GetGlobalMousePosition();
			mousePos.Y -= 48;
			
			foreach (var node in graph.vertexSet)
			{
				var nodePos = new Vector2(
					(float)(radius * Math.Cos(step * node.VertexId) + size.X / 2), 
					(float)(radius * Math.Sin(step * node.VertexId) + size.Y / 2)
				);
				if (nodePos.DistanceTo(mousePos) < nodeRadius) {
					graph.AddEdge(dragging, node.VertexId);
				}
			}
			dragging = -1;
			QueueRedraw();
		}
    }
}
