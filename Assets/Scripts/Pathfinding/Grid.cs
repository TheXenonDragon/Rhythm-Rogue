using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {
	/*	Fields */
	Node[,] grid;
	int gridSizeX, gridSizeY;

	
	/*	Methods	*/
	public void CreateGrid(TileType[,] mapLayout) {
		gridSizeX = mapLayout.GetLength(0);
		gridSizeY = mapLayout.GetLength(1);

		grid = new Node[gridSizeX, gridSizeY];

		for (int x = 0; x < gridSizeX; x ++) {
			for (int y = 0; y < gridSizeY; y ++) {
				grid[x,y] = new Node((mapLayout[x, y] == TileType.FloorTile), x, y);
			}
		}
	}

	public int MaxSize {
		get {
			return gridSizeX * gridSizeY;
		}
	}

	public List<Node> GetNeighbours(Node node) {
		List<Node> neighbours = new List<Node>();
		Vector2Int[] tempDirections = new Vector2Int[]{Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left};

		int checkX;
		int checkY;

		for(int i = 0; i < tempDirections.Length; i++){
			checkX = node.position.x + tempDirections[i].x;
			checkY = node.position.y + tempDirections[i].y;
			
			if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
				neighbours.Add(grid[checkX,checkY]);
			}
		}

		return neighbours;
	}

	public Node GetNodeAtPosition(Vector2Int position){
		return grid[position.x, position.y];
	}
}