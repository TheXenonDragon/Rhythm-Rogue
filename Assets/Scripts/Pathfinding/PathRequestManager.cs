using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public class PathRequestManager : MonoBehaviour {

	Queue<PathResult> results = new Queue<PathResult>();

	static PathRequestManager instance;

	Pathfinding[] pathfinders;

	void Awake() {
		instance = this;

		pathfinders = GetComponentsInChildren<Pathfinding>();

		//	pathfinders[1] is MediumPathfinding
	}

	void Update() {
		if (results.Count > 0) {
			int itemsInQueue = results.Count;
			lock (results) {
				for (int i = 0; i < itemsInQueue; i++) {
					PathResult result = results.Dequeue ();
					result.callback (result.path, result.success);
				}
			}
		}
	}
	
	public void UpdateGrid(TileType[,] regularMap, TileType[,] mediumMap){
		pathfinders[0].UpdateGrid(regularMap);
		pathfinders[1].UpdateGrid(mediumMap);
	}

	public static void RequestPath(PathRequest request, bool regularSize) {
		ThreadStart threadStart = delegate {
			if(regularSize){
				instance.pathfinders[0].MultiThreadFindPath (request, instance.FinishedProcessingPath);
			}
			else{
				instance.pathfinders[1].MultiThreadFindPath (request, instance.FinishedProcessingPath);
			}
			
		};
		threadStart.Invoke ();
	}

	public void FinishedProcessingPath(PathResult result) {
		lock (results) {
			results.Enqueue (result);
		}
	}
}

public struct PathResult {
	public Vector2Int[] path;
	public bool success;
	public Action<Vector2Int[], bool> callback;

	public PathResult (Vector2Int[] path, bool success, Action<Vector2Int[], bool> callback)
	{
		this.path = path;
		this.success = success;
		this.callback = callback;
	}

}

public struct PathRequest {
	public Vector2Int pathStart;
	public Vector2Int pathEnd;
	public Action<Vector2Int[], bool> callback;

	public PathRequest(Vector2Int _start, Vector2Int _end, Action<Vector2Int[], bool> _callback) {
		pathStart = _start;
		pathEnd = _end;
		callback = _callback;
	}

}