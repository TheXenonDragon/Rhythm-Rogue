using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChunkingManager : MonoBehaviour
{
    //  Object and script references
    public GameObject player;
    public FloorGenerator floorGenerator;
    public MapManager mapManager;
    public BeatController beatController;

    //  Prefabs
    public GameObject chamberBaseFloorPrefab;
    public GameObject exitPortalPrefab;

    //  Object Pool Holders
    public GameObject wallPoolHolder;
    public GameObject floorTilePoolHolder;
    public GameObject boxPoolHolder;
    public GameObject cratePoolHolder;
    public GameObject chestPoolHolder;

    //  Object Instance Pooling
    private GameObject[] wallInstances;
    private GameObject[] floorTileInstances;
    private List<ObstacleData> boxesPool;
    private List<ObstacleData> inUseBoxes;
    private List<ObstacleData> cratesPool;
    private List<ObstacleData> inUseCrates;
    private List<ObstacleData> chestsPool;
    private List<ObstacleData> inUseChests;
    private GameObject exitPortalObject;

    //  Object Pooling Maps
    private GameObject[,] wallMap;
    private GameObject[,] floorTileMap;

    //  Placement Map
    private TileType[,] tileTypes;
    private bool[,] obstaclePlacementMap;

    //  Sizes and offsets
    private float tileSize;
    private float cubeSizePerPlane = 10f;
    private int playableArea = 25;  //  How wide and tall the "rendered" map.

    private Vector2Int previousPosition;

    private bool piecePositionsNeedToBeRecalculated = false;

    //  Reset
    private bool completedChunkManagerReset = false;


    // Start is called before the first frame update
    void Start()
    {
        beatController.BeatExecuted += PiecePositionsNeedToBeRecalculated;
    }

    void Update()
    {
        CheckAndExecutePieceUpdate();
	}

    public void LoadMap(TileType[,] tempMap, Vector2Int playerStartingPositionTemp){
        completedChunkManagerReset = false;

        //  Get data from floorGenerator
        tileTypes = tempMap;
        tileSize = floorGenerator.tileSize;

        obstaclePlacementMap = new bool[tileTypes.GetLength(0), tileTypes.GetLength(1)];

        if(exitPortalObject == null){
            exitPortalObject = Instantiate<GameObject>(exitPortalPrefab, Vector3.zero, exitPortalPrefab.transform.rotation, transform);
            exitPortalObject.SetActive(false);
        }
        else{
            exitPortalObject.SetActive(false);
        }

        //  Set player starting position
        Vector2Int playerStartingPosition = playerStartingPositionTemp;
        previousPosition = playerStartingPosition;

        LoadMapPieces();
        MovePiecePositions(playerStartingPosition);
        ActivateMapPieces(playerStartingPosition);
        
        //  Initial set of chunks
        PiecePositionsNeedToBeRecalculated(null, null);
    }

    private void LoadMapPieces(){
        //  Nullify all previous maps
        wallMap = null;
        floorTileMap = null;

        //  Create map sizes
        wallMap = new GameObject[playableArea, playableArea];
        floorTileMap = new GameObject[playableArea, playableArea];

        int index;

        //  Wall maps
        if(wallInstances == null){
            wallInstances = new GameObject[wallPoolHolder.transform.childCount];
            for(int i = 0; i < wallInstances.Length; i++){
                wallInstances[i] = wallPoolHolder.transform.GetChild(i).gameObject;
            }
        }
        
        index = 0;
        for(int x = 0; x < playableArea; x++){
            for(int y = 0; y < playableArea; y++){
                wallMap[x, y] = wallInstances[index];
                wallMap[x, y].transform.position = new Vector3(x * tileSize, 0f, y * tileSize);
                wallMap[x, y].transform.parent = transform;
                wallMap[x, y].SetActive(false);
                index++;
            }
        }


        //  Floor tile maps
        if(floorTileInstances == null){
            floorTileInstances = new GameObject[floorTilePoolHolder.transform.childCount];
            for(int i = 0; i < floorTileInstances.Length; i++){
                floorTileInstances[i] = floorTilePoolHolder.transform.GetChild(i).gameObject;
            }
        }

        index = 0;
        for(int x = 0; x < playableArea; x++){
            for(int y = 0; y < playableArea; y++){
                floorTileMap[x, y] = floorTileInstances[index];
                floorTileMap[x, y].transform.position = new Vector3(x * tileSize, 0f, y * tileSize);
                floorTileMap[x, y].transform.parent = transform;
                floorTileMap[x, y].SetActive(false);
                index++;
            }
        }


        //  Boxes
        //  Old Boxes
        if(inUseBoxes != null){
            for(int i = 0; i < inUseBoxes.Count; i++){
                boxesPool.Add(inUseBoxes[i]);
            }
        }
        inUseBoxes = new List<ObstacleData>();

        //  New Boxes
        boxesPool = new List<ObstacleData>();
		for(int i = 0; i < boxPoolHolder.transform.childCount; i++){
            boxPoolHolder.transform.GetChild(i).gameObject.SetActive(false);
            boxesPool.Add(new ObstacleData(boxPoolHolder.transform.GetChild(i).gameObject, -1, -1));
		}


        //  Crates
        //  Old Crates
        if(inUseCrates != null){
            for(int i = 0; i < inUseCrates.Count; i++){
                cratesPool.Add(inUseCrates[i]);
            }
        }
        inUseCrates = new List<ObstacleData>();

        //  New Crates
        cratesPool = new List<ObstacleData>();
		for(int i = 0; i < cratePoolHolder.transform.childCount; i++){
            cratePoolHolder.transform.GetChild(i).gameObject.SetActive(false);
            cratesPool.Add(new ObstacleData(cratePoolHolder.transform.GetChild(i).gameObject, -1, -1));
		}


        //  Chests
        //  Old Chests
        if(inUseChests != null){
            for(int i = 0; i < inUseChests.Count; i++){
                chestsPool.Add(inUseChests[i]);
            }
        }
        inUseChests = new List<ObstacleData>();

        //  New Chests
        chestsPool = new List<ObstacleData>();
		for(int i = 0; i < chestPoolHolder.transform.childCount; i++){
            chestPoolHolder.transform.GetChild(i).gameObject.SetActive(false);
            chestsPool.Add(new ObstacleData(chestPoolHolder.transform.GetChild(i).gameObject, -1, -1));
		}
    }

    private void MovePiecePositions(Vector2Int playerStartingPosition){
		int midPoint = wallMap.GetLength(0) / 2;
        Vector3 newPosition;

		for(int x = 0; x < playableArea; x++){
            for(int y = 0; y < playableArea; y++){
                //  Player position:            (new Vector3(playerStartingPosition.x, 0f, playerStartingPosition.y)  * tileSize)
                //  Specific element position:  new Vector3(x * tileSize, -1.0f, y * tileSize)
                //  Midpoint offset:            (new Vector3(1.0f, 0.0f, 1.0f) * midPoint * tileSize)
                newPosition = (new Vector3(playerStartingPosition.x, 0f, playerStartingPosition.y)  * tileSize) + new Vector3(x * tileSize, 0f, y * tileSize) - (new Vector3(1.0f, 0.0f, 1.0f) * midPoint * tileSize);
                wallMap[x, y].transform.position = newPosition;
                floorTileMap[x, y].transform.position = newPosition;
			}
		}
	}

    private void ActivateMapPieces(Vector2Int playerPosition){
        int mapStartX = playerPosition.x - (playableArea / 2);
        int mapStartY = playerPosition.y - (playableArea / 2);

        int bufferStartX = 0;
        int bufferStartY = 0;
        int bufferEndX = tileTypes.GetLength(0) - 1;
        int bufferEndY = tileTypes.GetLength(1) - 1;

        if(mapStartX < 0){
            bufferStartX = -mapStartX;
        }
        if(mapStartY < 0){
            bufferStartY = -mapStartY;
        }

        for(int x = 0; x < playableArea; x++){
            for(int z = 0; z < playableArea; z++){
                if((bufferStartX - x) > 0){
                    //  If mapStartX < 0, set to innactive.
                    floorTileMap[x, z].SetActive(false);
                    wallMap[x, z].SetActive(false);
                }
                else if((bufferStartY - z) > 0){
                    //  If mapStartY < 0, set to innactive.
                    floorTileMap[x, z].SetActive(false);
                    wallMap[x, z].SetActive(false);
                }
                else if((x + mapStartX) > bufferEndX){
                    //  If the position being accessed is greater than tileTypes.GetLength(0), set to innactive.
                    floorTileMap[x, z].SetActive(false);
                    wallMap[x, z].SetActive(false);
                }
                else if((z + mapStartY) > bufferEndY){
                    //  If the position being accessed is greater than tileTypes.GetLength(1), set to innactive.
                    floorTileMap[x, z].SetActive(false);
                    wallMap[x, z].SetActive(false);
                }
                else{
                    if(tileTypes[mapStartX + x, mapStartY + z] == TileType.FloorTile){
                        //  Floor tile
                        if(!floorTileMap[x, z].activeInHierarchy){
                            floorTileMap[x, z].SetActive(true);
                        }
                        
                        if(wallMap[x, z].activeInHierarchy){
                            wallMap[x, z].SetActive(false);
                        }
                    }
                    else if(tileTypes[mapStartX + x, mapStartY + z] == TileType.Wall){
                        //  Wall tile
                        if(!wallMap[x, z].activeInHierarchy){
                            wallMap[x, z].SetActive(true);
                        }

                        if(floorTileMap[x, z].activeInHierarchy){
                            floorTileMap[x, z].SetActive(false);
                        }
                    }
                    else if(tileTypes[mapStartX + x, mapStartY + z] == TileType.Box){
                        //  Box Obstacle
                        if(!floorTileMap[x, z].activeInHierarchy){
                            floorTileMap[x, z].SetActive(true);
                        }
                        
                        if(wallMap[x, z].activeInHierarchy){
                            wallMap[x, z].SetActive(false);
                        }

                        if(!obstaclePlacementMap[mapStartX + x, mapStartY + z]){
                            boxesPool[0].obstacle.SetActive(true);
                            boxesPool[0].obstacle.transform.position = new Vector3((mapStartX + x) * tileSize, boxesPool[0].obstacle.transform.localScale.y / 2f, (mapStartY + z) * tileSize);
                            inUseBoxes.Add(new ObstacleData(boxesPool[0].obstacle, (mapStartX + x), (mapStartY + z)));
                            boxesPool.RemoveAt(0);
                            obstaclePlacementMap[mapStartX + x, mapStartY + z] = true;
                        }
                    }
                    else if(tileTypes[mapStartX + x, mapStartY + z] == TileType.Crate){
                        //  Crate Obstacle
                        if(!floorTileMap[x, z].activeInHierarchy){
                            floorTileMap[x, z].SetActive(true);
                        }
                        
                        if(wallMap[x, z].activeInHierarchy){
                            wallMap[x, z].SetActive(false);
                        }

                        if(!obstaclePlacementMap[mapStartX + x, mapStartY + z]){
                            cratesPool[0].obstacle.SetActive(true);
                            cratesPool[0].obstacle.transform.position = new Vector3((mapStartX + x) * tileSize, cratesPool[0].obstacle.transform.localScale.y / 2f, (mapStartY + z) * tileSize);
                            inUseCrates.Add(new ObstacleData(cratesPool[0].obstacle, (mapStartX + x), (mapStartY + z)));
                            cratesPool.RemoveAt(0);
                            obstaclePlacementMap[mapStartX + x, mapStartY + z] = true;
                        }
                    }
                    else if(tileTypes[mapStartX + x, mapStartY + z] == TileType.Barrel){
                        //  Barrel Obstacle
                        if(!floorTileMap[x, z].activeInHierarchy){
                            floorTileMap[x, z].SetActive(true);
                        }
                        
                        if(wallMap[x, z].activeInHierarchy){
                            wallMap[x, z].SetActive(false);
                        }
                    }
                    else if(tileTypes[mapStartX + x, mapStartY + z] == TileType.LargeBarricadeHorizontal){
                        //  Large Barricade (Horizontal) Obstacle
                        if(!floorTileMap[x, z].activeInHierarchy){
                            floorTileMap[x, z].SetActive(true);
                        }
                        
                        if(wallMap[x, z].activeInHierarchy){
                            wallMap[x, z].SetActive(false);
                        }
                    }
                    else if(tileTypes[mapStartX + x, mapStartY + z] == TileType.LargeBarricadeVertical){
                        //  Large Barricade (Vertical) Obstacle
                        if(!floorTileMap[x, z].activeInHierarchy){
                            floorTileMap[x, z].SetActive(true);
                        }
                        
                        if(wallMap[x, z].activeInHierarchy){
                            wallMap[x, z].SetActive(false);
                        }
                    }
                    else if(tileTypes[mapStartX + x, mapStartY + z] == TileType.Chest){
                        //  Chest Obstacle
                        if(!floorTileMap[x, z].activeInHierarchy){
                            floorTileMap[x, z].SetActive(true);
                        }
                        
                        if(wallMap[x, z].activeInHierarchy){
                            wallMap[x, z].SetActive(false);
                        }

                        if(!obstaclePlacementMap[mapStartX + x, mapStartY + z]){
                            chestsPool[0].obstacle.SetActive(true);
                            chestsPool[0].obstacle.transform.position = new Vector3((mapStartX + x) * tileSize, chestsPool[0].obstacle.transform.localScale.y / 2f, (mapStartY + z) * tileSize);
                            inUseChests.Add(new ObstacleData(chestsPool[0].obstacle, (mapStartX + x), (mapStartY + z)));
                            chestsPool.RemoveAt(0);
                            obstaclePlacementMap[mapStartX + x, mapStartY + z] = true;
                        }
                    }
                    else if(tileTypes[mapStartX + x, mapStartY + z] == TileType.ExitPortal){
                        //  Exit Portal
                        if(!floorTileMap[x, z].activeInHierarchy){
                            floorTileMap[x, z].SetActive(true);
                        }
                        
                        if(wallMap[x, z].activeInHierarchy){
                            wallMap[x, z].SetActive(false);
                        }

                        if(!obstaclePlacementMap[mapStartX + x, mapStartY + z]){
                            exitPortalObject.transform.position = new Vector3((mapStartX + x) * tileSize, exitPortalPrefab.transform.position.y, (mapStartY + z) * tileSize);
                            exitPortalObject.SetActive(true);
                            obstaclePlacementMap[mapStartX + x, mapStartY + z] = true;
                        }
                    }
                    else if(tileTypes[mapStartX + x, mapStartY + z] == TileType.LargePillar){
                        //  Pillar Obstacle
                        if(!floorTileMap[x, z].activeInHierarchy){
                            floorTileMap[x, z].SetActive(true);
                        }
                        
                        if(wallMap[x, z].activeInHierarchy){
                            wallMap[x, z].SetActive(false);
                        }
                    }
                    else{
                        //  Set to innactive
                        if(floorTileMap[x, z].activeInHierarchy){
                            floorTileMap[x, z].SetActive(false);
                        }
                        if(wallMap[x, z].activeInHierarchy){
                            wallMap[x, z].SetActive(false);
                        }
                    }
                }
            }
        }
    }

    private void RemoveBoxes(Vector2Int playerPosition){
        for(int i = inUseBoxes.Count - 1; i >= 0; i--){
            if(Vector2Int.Distance(new Vector2Int(inUseBoxes[i].x, inUseBoxes[i].y), playerPosition) > (playableArea / 2)){
                obstaclePlacementMap[inUseBoxes[i].x, inUseBoxes[i].y] = false;
                inUseBoxes[i].obstacle.SetActive(false);
                boxesPool.Add(inUseBoxes[i]);
                inUseBoxes.RemoveAt(i);
            }
        }
    }

    private void RemoveCrates(Vector2Int playerPosition){
        for(int i = inUseCrates.Count - 1; i >= 0; i--){
            if(Vector2Int.Distance(new Vector2Int(inUseCrates[i].x, inUseCrates[i].y), playerPosition) > (playableArea / 2)){
                obstaclePlacementMap[inUseCrates[i].x, inUseCrates[i].y] = false;
                inUseCrates[i].obstacle.SetActive(false);
                cratesPool.Add(inUseCrates[i]);
                inUseCrates.RemoveAt(i);
            }
        }
    }

    private void RemoveChests(Vector2Int playerPosition){
        for(int i = inUseChests.Count - 1; i >= 0; i--){
            if(Vector2Int.Distance(new Vector2Int(inUseChests[i].x, inUseChests[i].y), playerPosition) > (playableArea / 2)){
                obstaclePlacementMap[inUseChests[i].x, inUseChests[i].y] = false;
                inUseChests[i].obstacle.SetActive(false);
                chestsPool.Add(inUseChests[i]);
                inUseChests.RemoveAt(i);
            }
            else if (!inUseChests[i].obstacle.activeInHierarchy){
                //  If the chest has been destroyed by the player, remove this chest from the active pool.
                obstaclePlacementMap[inUseChests[i].x, inUseChests[i].y] = false;
                tileTypes[inUseChests[i].x, inUseChests[i].y] = TileType.FloorTile;
                chestsPool.Add(inUseChests[i]);
                inUseChests.RemoveAt(i);
            }
        }
    }

    private void CheckAndExecutePieceUpdate(){
        if(piecePositionsNeedToBeRecalculated){
            MovePiecePositions(mapManager.GetPlayerPosition());
            RemoveBoxes(mapManager.GetPlayerPosition());
            RemoveCrates(mapManager.GetPlayerPosition());
            RemoveChests(mapManager.GetPlayerPosition());
            ActivateMapPieces(mapManager.GetPlayerPosition());
            piecePositionsNeedToBeRecalculated = false;
        }
    }

    private void PiecePositionsNeedToBeRecalculated(object sender, EventArgs e){
        piecePositionsNeedToBeRecalculated = true;
        completedChunkManagerReset = true;
    }

    public bool CompletedReset(){
        return completedChunkManagerReset;
    }

    private void PrintDebugMap(int num){
        string map = "INDEX: " + num + "\n";

        for(int x = 0; x < tileTypes.GetLength(0); x++){
            for(int y = 0; y < tileTypes.GetLength(0); y++){
                if(obstaclePlacementMap[x, y]){
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(255f), (byte)(0f), (byte)(0f), "+ ");
                }
                else if(tileTypes[x, y] == TileType.Wall){
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(0f), (byte)(0f), (byte)(0f), "= ");
                }
                else if(tileTypes[x, y] == TileType.FloorTile){
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(100f), (byte)(100f), (byte)(100f), "+ ");
                }
                else if(tileTypes[x, y] == TileType.Box){
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(200f), (byte)(0f), (byte)(200f), "+ ");
                }
                else if(tileTypes[x, y] == TileType.Chest){
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(0f), (byte)(250f), (byte)(250f), "+ ");
                }
                else {
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(255f), (byte)(255f), (byte)(0f), "+ ");
                }
            }
            map += "\n";
        }

        Debug.Log(map);
    }

    

    private struct ObstacleData{
        public GameObject obstacle;
        public int x;
        public int y;

        public ObstacleData(GameObject obstacle, int x, int y){
            this.obstacle = obstacle;
            this.x = x;
            this.y = y;
        }
    }
}
