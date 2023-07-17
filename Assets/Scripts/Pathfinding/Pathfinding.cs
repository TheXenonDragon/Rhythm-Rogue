using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour {

	Grid grid;
	
	void Awake() {
		grid = GetComponent<Grid>();
	}

	public void UpdateGrid(TileType[,] map){
		grid.CreateGrid(map);
	}
	
	public PathResult FindPath(PathRequest request) {
		Vector2Int[] waypoints = new Vector2Int[0];
		bool pathSuccess = false;
		
		Node startNode = grid.GetNodeAtPosition(request.pathStart);
		Node targetNode = grid.GetNodeAtPosition(request.pathEnd);
		startNode.parent = startNode;

		if (targetNode.walkable) {
			Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node>();
			openSet.Add(startNode);
			
			while (openSet.Count > 0) {
				Node currentNode = openSet.RemoveFirst();
				closedSet.Add(currentNode);
				
				//	If path found, exit
				if (currentNode == targetNode) {
					//print ("Path found: " + sw.ElapsedMilliseconds + " ms");
					pathSuccess = true;
					break;
				}
				
				foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
					if (!neighbour.walkable || closedSet.Contains(neighbour)) {
						continue;
					}
					
					int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
					if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
						neighbour.gCost = newMovementCostToNeighbour;
						neighbour.hCost = GetDistance(neighbour, targetNode);
						neighbour.parent = currentNode;
						
						if (!openSet.Contains(neighbour))
							openSet.Add(neighbour);
						else 
							openSet.UpdateItem(neighbour);
					}
				}
			}
		}

		if (pathSuccess) {
			waypoints = RetracePath(startNode,targetNode);
			pathSuccess = waypoints.Length > 0;
		}

		return new PathResult (waypoints, pathSuccess, request.callback);
	}

	public void MultiThreadFindPath(PathRequest request, Action<PathResult> callback) {
		
		Stopwatch sw = new Stopwatch();
		sw.Start();
		
		Vector2Int[] waypoints = new Vector2Int[0];
		bool pathSuccess = false;
		
		Node startNode = grid.GetNodeAtPosition(request.pathStart);
		Node targetNode = grid.GetNodeAtPosition(request.pathEnd);
		startNode.parent = startNode;
		
		
		if (targetNode.walkable) {
			Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node>();
			openSet.Add(startNode);
			
			while (openSet.Count > 0) {
				Node currentNode = openSet.RemoveFirst();
				closedSet.Add(currentNode);
				
				//	If path found, exit
				if (currentNode == targetNode) {
					sw.Stop();
					//print ("Path found: " + sw.ElapsedMilliseconds + " ms");
					pathSuccess = true;
					break;
				}
				
				foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
					if (!neighbour.walkable || closedSet.Contains(neighbour)) {
						continue;
					}
					
					int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
					if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
						neighbour.gCost = newMovementCostToNeighbour;
						neighbour.hCost = GetDistance(neighbour, targetNode);
						neighbour.parent = currentNode;
						
						if (!openSet.Contains(neighbour))
							openSet.Add(neighbour);
						else 
							openSet.UpdateItem(neighbour);
					}
				}
			}
		}

		if (pathSuccess) {
			waypoints = RetracePath(startNode,targetNode);
			pathSuccess = waypoints.Length > 0;
		}

		callback (new PathResult (waypoints, pathSuccess, request.callback));
	}

	//	Create an array of correctly ordered nodes.
	Vector2Int[] RetracePath(Node startNode, Node endNode) {
		List<Node> path = new List<Node>();
		Node currentNode = endNode;
		
		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}

		return NodeListToWaypoints(path);
	}

	//	Convert from node list to a reversed position array.
	Vector2Int[] NodeListToWaypoints(List<Node> path){
		Vector2Int[] waypoints = new Vector2Int[path.Count];

		//	Reverse Path
		for(int i = 0; i < waypoints.Length; i++){
			waypoints[i] = path[path.Count - 1 - i].position;
		}

		return waypoints;
	}
	
	int GetDistance(Node nodeA, Node nodeB) {
		int dstX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
		int dstY = Mathf.Abs(nodeA.position.y - nodeB.position.y);
		
		if (dstX > dstY)
			return 14*dstY + 10* (dstX-dstY);
		return 14*dstX + 10 * (dstY-dstX);
	}
	
	
}