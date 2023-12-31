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
using System.Linq;

public class Node
{
	public readonly int VertexId;
	public readonly HashSet<int> AdjacencySet;

	public string Name
	{ get; set; }

	public string Markup
	{ get; set; }

	public Node(int vertexId, string name = "")
	{
		this.VertexId = vertexId;
		this.AdjacencySet = new HashSet<int>();
		this.Name = name;
	}

	public void AddEdge(int v)
	{
		if (this.VertexId == v)
			throw new ArgumentException("The vertex cannot be adjacent to itself");
		this.AdjacencySet.Add(v);
	}

	public void RemoveEdge(int v)
	{
		this.AdjacencySet.Remove(v);
	}
}

public class Graph
{
	public HashSet<Node> vertexSet { get; } = new HashSet<Node>();
	private int idCounter = 0;
	private Stack<int> deadIds = new Stack<int>();

	public int VertexCount
	{ get { return vertexSet.Count; } }

	public Node AddNode(string nodeName)
	{
		int id;
		bool isRe = deadIds.TryPop(out id);
		if (!isRe) id = idCounter;
		Node node = new Node(id, nodeName);
		vertexSet.Add(node);
		if (!isRe) idCounter++;
		return node;
	}

	public void RemoveNode(int id)
	{
		if (!vertexSet.Any(n => n.VertexId == id))
			throw new ArgumentOutOfRangeException("Vertex is out of bounds");
		vertexSet.RemoveWhere(n => n.VertexId == id);
		foreach (var node in vertexSet)
		{
			node.AdjacencySet.RemoveWhere(n => n == id);
		}
		deadIds.Push(id);
	}

	public void AddEdge(int v1, int v2)
	{
		if (!vertexSet.Any(n => n.VertexId == v1) || !vertexSet.Any(n => n.VertexId == v2))
			throw new ArgumentOutOfRangeException("Vertices are out of bounds");

		this.vertexSet.ElementAt(v1).AddEdge(v2);
	}

	public void RemoveEdge(int v1, int v2)
	{
		if (!vertexSet.Any(n => n.VertexId == v1) || !vertexSet.Any(n => n.VertexId == v2))
			throw new ArgumentOutOfRangeException("Vertices are out of bounds");

		this.vertexSet.ElementAt(v1).RemoveEdge(v2);
	}

	public IEnumerable<int> GetAdjacentVertices(int v)
	{
		if (!vertexSet.Any(n => n.VertexId == v))
			throw new ArgumentOutOfRangeException("Vertex is out of bounds");

		return this.vertexSet.ElementAt(v).AdjacencySet;
	}

#nullable enable
	public Node? NodeAt(int v)
	{
		if (!vertexSet.Any(n => n.VertexId == v))
			return null;

		return this.vertexSet.ElementAt(v);
	}
#nullable disable

	public override string ToString()
	{
		string s = "";
		foreach (var node in vertexSet)
		{
			s += node.VertexId + " " + node.Name + ": ";
			foreach (var edge in node.AdjacencySet)
			{
				s += edge + ", ";
			}
			s += "\n";
		}
		return s;
	}
}
