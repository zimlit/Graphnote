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
	public readonly HashSet<int> selected = new HashSet<int>();
	public readonly HashSet<(int, int)> selectedEdges = new HashSet<(int, int)>();
	int dragging = -1;

	readonly Color nodeColor = new Color(0.1f, 0.5f, 0.2f);
	readonly Color selectedColor = new Color(0.5f, 0.2f, 0.1f);

	// drawing stuff
	double radius = 100;
	double step;
	float nodeRadius = 10;

	[Signal]
	public delegate void EdgeAddedEventHandler(int id1, int id2);

	[Signal]
	public delegate void NodeOpenedEventHandler(int id);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var noteGraph = GetParent<NoteGraph>();
		graph = noteGraph.graph;
		size = GetViewportRect().Size;
		noteGraph.Resized += OnResize;
	}

	private void OnResize()
	{
		size = GetViewportRect().Size;
		QueueRedraw();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (dragging != -1)
		{
			QueueRedraw();
		}
	}

	public override void _Draw()
	{
		List<Vector2> nodesToDraw = new List<Vector2>();

		step = Math.Tau / graph.VertexCount;
		while (Math.Tau * radius / 3 < graph.VertexCount * 20)
			radius *= 2;

		if (dragging != -1)
		{
			var mousePos = GetLocalMousePosition();
			var nodePos = new Vector2(
				(float)(radius * Math.Cos(step * dragging) + size.X / 2),
				(float)(radius * Math.Sin(step * dragging) + size.Y / 2)
			);
			DrawLine(nodePos, mousePos, Colors.White, antialiased: true);
		}

		int i = -1;
		foreach (var node in graph.vertexSet)
		{
			i++;

			var nodePos = new Vector2(
				(float)(radius * Math.Cos(step * i) + size.X / 2),
				(float)(radius * Math.Sin(step * i) + size.Y / 2)
			);
			nodesToDraw.Add(nodePos);


			foreach (var edge in node.AdjacencySet)
			{
				Color color;
				if (selectedEdges.Contains((node.VertexId, edge)))
					color = selectedColor;
				else
					color = Colors.White;
				DrawLine(
					nodePos,
					new Vector2(
						(float)(radius * Math.Cos(step * edge) + size.X / 2),
						(float)(radius * Math.Sin(step * edge) + size.Y / 2)
					),
					color,
					antialiased: true
				);
			}
		}

		foreach (var node in nodesToDraw)
		{
			if (selected.Contains(nodesToDraw.IndexOf(node)))
			{
				DrawArc(node, nodeRadius + 1, 0, 360, 100, selectedColor, 1);
			}
			DrawCircle(node, nodeRadius, nodeColor);
			Font defaultFont = ThemeDB.FallbackFont;
			int defaultFontSize = ThemeDB.FallbackFontSize;
			var name = graph.NodeAt(nodesToDraw.IndexOf(node)).Name;
			var width = defaultFont.GetStringSize(name, HorizontalAlignment.Left, -1, defaultFontSize);
			DrawString(defaultFont, node + new Vector2(-width.X / 2, 30), name, HorizontalAlignment.Left, -1, defaultFontSize);
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionPressed("click"))
		{
			var mousePos = GetLocalMousePosition();
			foreach (var node in graph.vertexSet)
			{
				var nodePos = new Vector2(
					(float)(radius * Math.Cos(step * node.VertexId) + size.X / 2),
					(float)(radius * Math.Sin(step * node.VertexId) + size.Y / 2)
				);
				if (nodePos.DistanceTo(mousePos) < nodeRadius)
				{
					if (dragging == -1)
						dragging = node.VertexId;
					if (Input.IsKeyPressed(Key.Shift))
						if (selected.Contains(node.VertexId))
							selected.Remove(node.VertexId);
						else
							selected.Add(node.VertexId);
					else
					{
						selected.Clear();
						selectedEdges.Clear();
						selected.Add(node.VertexId);
					}
					QueueRedraw();
					break;
				}

				foreach (var adjacent in node.AdjacencySet)
				{
					if (dragging != -1)
						break;
					var endPos = new Vector2(
						(float)(radius * Math.Cos(step * adjacent) + size.X / 2),
						(float)(radius * Math.Sin(step * adjacent) + size.Y / 2)
					);

					if (Math.Abs((endPos - nodePos).Cross(mousePos - nodePos)) > 1000)
						continue;

					var dotproduct = (endPos - nodePos).Dot(mousePos - nodePos);
					if (dotproduct < 0)
						continue;

					var length = (endPos - nodePos).LengthSquared();
					if (dotproduct > length)
						continue;

					if (Input.IsKeyPressed(Key.Shift))
					{
						if (selectedEdges.Contains((node.VertexId, adjacent)))
						{
							selectedEdges.Remove((node.VertexId, adjacent));
						}
						else
						{
							selectedEdges.Add((node.VertexId, adjacent));
						}
					}
					else
					{
						selectedEdges.Clear();
						selected.Clear();
						selectedEdges.Add((node.VertexId, adjacent));
					}
					QueueRedraw();
					break;
				}
			}

			if (@event is InputEventMouseButton buttonEvent && buttonEvent.DoubleClick)
			{
				try
				{
					EmitSignal(SignalName.NodeOpened, selected.Last());
				}
				catch (Exception) { }
			}
		}
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && !mouseEvent.Pressed)
		{
			var mousePos = GetLocalMousePosition();

			foreach (var node in graph.vertexSet)
			{
				if (node.VertexId == dragging)
					continue;
				var nodePos = new Vector2(
					(float)(radius * Math.Cos(step * node.VertexId) + size.X / 2),
					(float)(radius * Math.Sin(step * node.VertexId) + size.Y / 2)
				);
				if (nodePos.DistanceTo(mousePos) < nodeRadius)
				{
					EmitSignal(SignalName.EdgeAdded, dragging, node.VertexId);
				}
			}
			QueueRedraw();
			dragging = -1;

		}
		if (Input.IsActionJustPressed("delete"))
		{
			foreach (var node in selected)
			{
				graph.RemoveNode(node);
			}
			foreach (var edge in selectedEdges)
			{
				graph.RemoveEdge(edge.Item1, edge.Item2);
			}
			selected.Clear();
			selectedEdges.Clear();
			QueueRedraw();
		}
	}
}
