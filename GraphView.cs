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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class GraphView : Control
{
	private Graph graph;
	private Vector2 size;

	readonly Color nodeColor = new Color(0.1f, 0.5f, 0.2f);
	readonly Color selectedColor = new Color(0.5f, 0.2f, 0.1f);


	// drawing stuff
	const float nodeRadius = 10;
	private HashSet<Vector2> nodePositions = new HashSet<Vector2>();

	[Signal]
	public delegate void EdgeAddedEventHandler(int id1, int id2);

	[Signal]
	public delegate void NodeOpenedEventHandler(int id);

	[Signal]
	public delegate void NodeDeletedEventHandler(int id);

	[Signal]
	public delegate void EdgeDeletedEventHandler(int id1, int id2);


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
	}

	public override void _Draw()
	{
		ArrangeNodes();
		DrawEdges();
		DrawNodes();
	}

	private void DrawNodes()
	{
		foreach (var nodePosition in nodePositions)
		{
			DrawCircle(nodePosition, nodeRadius, nodeColor);
		}
	}


	private void DrawEdges()
	{
		foreach (var node in graph.vertexSet)
		{
			foreach (var adjacent in node.AdjacencySet)
			{
				DrawLine(nodePositions.ElementAt(node.VertexId), nodePositions.ElementAt(adjacent), nodeColor);
			}
		}
	}


	private void ArrangeNodes()
	{
		nodePositions.Clear();
		int numNodes = graph.vertexSet.Count;
		float angle = 0;
		float angleIncrement = 2 * Mathf.Pi / numNodes;
		float radius = Mathf.Min(size.X, size.Y) / 2 - 50;
		Vector2 center = size / 2;
		DrawCircle(center, radius, Colors.White);
		foreach (var node in graph.vertexSet)
		{
			nodePositions.Add(new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle)) + center);
			angle += angleIncrement;
		}
	}

	public override void _Input(InputEvent @event)
	{
	}
}
