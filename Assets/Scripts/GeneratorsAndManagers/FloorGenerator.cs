using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FloorGenerator : MonoBehaviour
{
    //  Size
    public int totalSize;
    public int chamberSize;
    
    private int roomWidth;
    private int roomHeight;
    public int minRoomWidth;
    public int minRoomHeight;

    public float tileSize;
    private float chamberBaseFloorSize = 10f;

    //  Indices
    private int exitRoomChamberIndex = -1;

    //  Prefabs
    public GameObject emptyTilePrefab;
    public GameObject dungeonTilePrefab;
    public GameObject wallTilePrefab;
    public GameObject chamberBaseFloorPrefab;
    public GameObject viewBoundaryPrefab;
    public GameObject obstacleCleanerPrefab;
    public GameObject exitPortalPrefab;

    //  Active Objects
    private TileType[,,] tileTypes;     //  The first dimension is the chamber Index (in order to connect to the correct chamber).  The second dimension is the x position of the tile, and the third dimension is the y position of the tile.
    public GameObject[,,] dungeonTiles;  //  The first dimension is the chamber Index (in order to connect to the correct chamber).  The second dimension is the x position of the tile, and the third dimension is the y position of the tile.
    private Vector2Int[] dungeonRoomDimensions;
    private GameObject[] chambers;
    private GameObject floorPlan;
    private List<GameObject> obstacles;

    //  Furnishings
    public GameObject boxPrefab;
    public GameObject chestPrefab;

    //  Stats for Furnishings
    public int averageBoxRatePerRoom = 12;
    public float chestChancePerChamber = 0.25f;

    //  Misc.


    // Start is called before the first frame update
    void Start()
    {
        //  Instantiation
        floorPlan = new GameObject("FloorPlan");
        chambers = new GameObject[(totalSize * totalSize) + 1];
        tileTypes = new TileType[chambers.Length, chamberSize, chamberSize];
        dungeonTiles = new GameObject[chambers.Length, chamberSize, chamberSize];
        dungeonRoomDimensions = new Vector2Int[chambers.Length];


        CreateLevel();
    }


    //  Public Methods
    public List<List<Vector3>> GetAvailableDungeonTileLocations(){
        List<List<Vector3>> availableTilesLocations = new List<List<Vector3>>();
        
        for(int chamberIndex = 0; chamberIndex < (totalSize * totalSize); chamberIndex++){
            availableTilesLocations.Add(AvailableDungeonTiles(chamberIndex));
        }

        return availableTilesLocations;
    }

    //  Primary Methods
    public void CreateLevel()
    {
        int chamberID = 0;
        floorPlan.transform.parent = this.transform;

        CreateFloorPlan();
        GenerateRooms(chamberID);
        AddTiles();
        CreateViewBounds();
        FurnishInterior();

        //  Shift level
        floorPlan.transform.position = new Vector3(-chamberSize * tileSize / 2f, 0f, -chamberSize * tileSize / 2f);

        CleanObstacles();
        CreateExitPortal();

        //  Add navmesh
        RecalculateNavMesh();
    }

    private void CreateViewBounds(){
        float mapSize = totalSize * chamberSize * tileSize;
        float viewBoundSize = 50f;
        float height = 5f / 2f;

        Vector3 northPosition = new Vector3((mapSize / 2f) - 1f, height, mapSize + (viewBoundSize / 2f) - 1f);
        Vector3 southPosition = new Vector3((mapSize / 2f) - 1f, height, -(viewBoundSize / 2f) - 1f);
        Vector3 eastPosition = new Vector3(mapSize + (viewBoundSize / 2f) - 1f, height, (mapSize / 2f) - 1f);
        Vector3 westPosition = new Vector3(-(viewBoundSize / 2f) - 1f, height, (mapSize / 2f) - 1f);

        GameObject northBound = Instantiate<GameObject>(viewBoundaryPrefab, northPosition, chamberBaseFloorPrefab.transform.rotation, floorPlan.transform);
        GameObject southBound = Instantiate<GameObject>(viewBoundaryPrefab, southPosition, chamberBaseFloorPrefab.transform.rotation, floorPlan.transform);
        GameObject eastBound = Instantiate<GameObject>(viewBoundaryPrefab, eastPosition, chamberBaseFloorPrefab.transform.rotation, floorPlan.transform);
        GameObject westBound = Instantiate<GameObject>(viewBoundaryPrefab, westPosition, chamberBaseFloorPrefab.transform.rotation, floorPlan.transform);

        northBound.transform.localScale = new Vector3((mapSize + (viewBoundSize * 2f)) / chamberBaseFloorSize, 1, viewBoundSize / chamberBaseFloorSize);
        southBound.transform.localScale = new Vector3((mapSize + (viewBoundSize * 2f)) / chamberBaseFloorSize, 1, viewBoundSize / chamberBaseFloorSize);
        eastBound.transform.localScale = new Vector3(viewBoundSize / chamberBaseFloorSize, 1, mapSize / chamberBaseFloorSize);
        westBound.transform.localScale = new Vector3(viewBoundSize / chamberBaseFloorSize, 1, mapSize / chamberBaseFloorSize);
    }

    private void CreateFloorPlan()
    {
        int chamberCounter = 0;

        for (int j = 0; j < totalSize; j++)
        {
            for (int i = 0; i < totalSize; i++)
            {
                chambers[chamberCounter] = new GameObject("Chamber #" + chamberCounter);    //  Create chamber
                chambers[chamberCounter].transform.position = new Vector3((i * tileSize * chamberSize), 0, (j * tileSize * chamberSize)); //  Set position of chamber
                CreateChamber(chamberCounter);                                      //  Determine tile placement in the chamber
                chambers[chamberCounter].transform.parent = floorPlan.transform;    //  Parent chamber to floorplan
                chamberCounter++;                                                   //  Increment chamber counter
            }
        }
    }
    private void GenerateRooms(int startingIndex)
    {
        List<int> nearbyRoomsIndexs = new List<int>();

        int index = startingIndex;
        CreateRoom(index);

        int lastChamberIndex = -1;

        do
        {
            //  Scan tiles around selected tile in clockwise motion
            if (index + totalSize < chambers.Length)
            {
                nearbyRoomsIndexs.Add(index + totalSize);
            }
            if ((index + 1 < chambers.Length) && ((index + 1) / totalSize == index / totalSize))
            {
                nearbyRoomsIndexs.Add(index + 1);
            }
            if (index - totalSize >= 0)
            {
                nearbyRoomsIndexs.Add(index - totalSize);
            }
            if (index - 1 >= 0 && ((index - 1) / totalSize == index / totalSize))
            {
                nearbyRoomsIndexs.Add(index - 1);
            }

            //  Check to see which chambers are available.  If they are not, delete them from the list
            for (int i = nearbyRoomsIndexs.Count - 1; i >= 0; i--)
            {
                if (ChamberHasRoom(nearbyRoomsIndexs[i]))
                {
                    nearbyRoomsIndexs.RemoveAt(i);
                }
            }
            for(int i = nearbyRoomsIndexs.Count - 1; i >= 0; i--)
            {
                if (lastChamberIndex == nearbyRoomsIndexs[i])
                {
                    nearbyRoomsIndexs.RemoveAt(i);
                }
            }

            //  Randomly select which chambers will be used for creating rooms


            //  Create rooms
            for (int i = 0; i < nearbyRoomsIndexs.Count; i++)
            {
                CreateRoom(nearbyRoomsIndexs[i]);
                CreateCorridor(index, nearbyRoomsIndexs[i]);
            }

            //  Assign the current chamber index to the lastChamberIndex so that it can be checked on the next iteration
            lastChamberIndex = index;

            //  Pass on the room generation to the next chambers
            if(nearbyRoomsIndexs.Count < 1)
            {
                exitRoomChamberIndex = index;
                CreateRoom(exitRoomChamberIndex);
                break;
            }
            else
            {
                index = nearbyRoomsIndexs[Random.Range(0, nearbyRoomsIndexs.Count)];
                nearbyRoomsIndexs.Clear();
            }
        } while (true);
    }
    
    private void FurnishInterior()
    {
        int tileCounter;
        int numFurnishings;
        List<Vector3> availableTileCoordinates;
        GameObject furnitureInstance;

        obstacles = new List<GameObject>();

        //  Create Boxes
        for (int chamberIndex = 0; chamberIndex < chambers.Length; chamberIndex++)
        {
            //  Count the number of available tiles in the given chamber
            availableTileCoordinates = AvailableDungeonTiles(chamberIndex);
            tileCounter = availableTileCoordinates.Count;

            //  Add furnishings
            //  Add Chests
            if(Random.Range(0f, chestChancePerChamber) < chestChancePerChamber){
                int index = Random.Range(0, availableTileCoordinates.Count);
                if(index < availableTileCoordinates.Count){
                    furnitureInstance = Instantiate(chestPrefab);
                    furnitureInstance.transform.position = availableTileCoordinates[index] + new Vector3(0f, 1f, 0f);
                    furnitureInstance.transform.parent = chambers[chamberIndex].transform;
                    obstacles.Add(furnitureInstance);
                    availableTileCoordinates.RemoveAt(index);
                }
            }

            //  Add Boxes
            numFurnishings = tileCounter / averageBoxRatePerRoom;
            for(int i = 0; i < numFurnishings; i++)
            {
                int index = Random.Range(0, availableTileCoordinates.Count);
                furnitureInstance = Instantiate(boxPrefab);
                furnitureInstance.transform.position = availableTileCoordinates[index] + new Vector3(0f, 1f, 0f);
                furnitureInstance.transform.parent = chambers[chamberIndex].transform;
                obstacles.Add(furnitureInstance);
                availableTileCoordinates.RemoveAt(index);
            }

            //  Clean List
            availableTileCoordinates.Clear();
        }
    }
    
    public void RecalculateNavMesh(){
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    private void CleanObstacles(){
        GameObject obstacleCleaner = Instantiate<GameObject>(obstacleCleanerPrefab, Vector3.zero, obstacleCleanerPrefab.transform.rotation);
        obstacleCleaner.transform.parent = transform.parent;

        obstacleCleaner.GetComponent<ObstacleCleanerManager>().StartExecution(GetComponent<FloorGenerator>(), obstacles, tileSize, dungeonTiles[exitRoomChamberIndex, chamberSize / 2, chamberSize / 2].transform.position);
    }

    private void CreateExitPortal(){
        GameObject exitPortal = Instantiate<GameObject>(exitPortalPrefab, dungeonTiles[exitRoomChamberIndex, chamberSize / 2, chamberSize / 2].transform.position, exitPortalPrefab.transform.rotation, chambers[exitRoomChamberIndex].transform);
    }


    //  Helper Methods
    private void AddTiles(){
        GameObject chamberFloor;
        GameObject dungeonTileInstance;

        Vector3 tilePosition;
        Vector3 chamberPosition;

        for(int chamberIndex = 0; chamberIndex < (totalSize * totalSize); chamberIndex++){
            chamberPosition = chambers[chamberIndex].transform.position;
            chamberFloor = Instantiate<GameObject>(chamberBaseFloorPrefab, chamberPosition + (new Vector3(1f, 0f, 1f) * (chamberSize * tileSize / 2f)) - new Vector3(1f, 0f, 1f), chamberBaseFloorPrefab.transform.rotation, chambers[chamberIndex].transform);
            chamberFloor.transform.localScale = new Vector3(1f, 0f, 1f) * (chamberSize * tileSize / chamberBaseFloorSize);  //  Planes are 10x the size of cubes.

            for(int x = 0; x < chamberSize; x++){
                for(int z = 0; z < chamberSize; z++){
                    tilePosition = new Vector3(chamberPosition.x + (x * tileSize), 0f, chamberPosition.z + (z * tileSize));

                    if(tileTypes[chamberIndex, x, z] == TileType.FloorTile){
                        //  Floor tile
                        dungeonTileInstance = Instantiate<GameObject>(dungeonTilePrefab, tilePosition, dungeonTilePrefab.transform.rotation, chambers[chamberIndex].transform);
                    }
                    else if(tileTypes[chamberIndex, x, z] == TileType.Wall){
                        //  Wall tile
                        dungeonTileInstance = Instantiate<GameObject>(wallTilePrefab, tilePosition, wallTilePrefab.transform.rotation, chambers[chamberIndex].transform);
                    }
                    else{
                        //  Empty tile
                        dungeonTileInstance = Instantiate<GameObject>(emptyTilePrefab, tilePosition, emptyTilePrefab.transform.rotation, chambers[chamberIndex].transform);
                    }

                    dungeonTiles[chamberIndex, x, z] = dungeonTileInstance;
                }
            }
        }
    }

    private void ReplaceTiles(int chamberID, int blockXPos, int blockYPos)
    {
        tileTypes[chamberID, blockXPos, blockYPos] = TileType.FloorTile;
    }

    private void CreateRoom(int chamberID)
    {
        roomWidth = (Random.Range(minRoomWidth, chamberSize - 1) / 2) * 2;
        roomHeight = (Random.Range(minRoomHeight, chamberSize - 1) / 2) * 2;

        //  Use max size for the exit room
        if(chamberID == exitRoomChamberIndex){
            roomWidth = ((chamberSize - 2) / 2) * 2;
            roomHeight = roomWidth;
        }

        dungeonRoomDimensions[chamberID] = new Vector2Int(roomWidth, roomHeight);

        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                ReplaceTiles(chamberID, (chamberSize / 2) - (roomWidth / 2) + x, (chamberSize / 2) - (roomHeight / 2) + y);
            }
        }
    }

    private void CreateCorridor(int fromChamberIndex, int toChamberIndex)
    {
        bool isHeadingUp, isHeadingDown, isHeadingRight, isHeadingLeft;
        isHeadingUp = isHeadingDown = isHeadingRight = isHeadingLeft = false;
        Vector2 positionTracker;
        int xPosTracker = chamberSize / 2;
        int yPosTracker = chamberSize / 2;

        if (toChamberIndex - fromChamberIndex > 1)
        {
            isHeadingUp = true;
        }
        else if (toChamberIndex - fromChamberIndex == 1)
        {
            isHeadingRight = true;
        }
        else if (toChamberIndex - fromChamberIndex == -1)
        {
            isHeadingLeft = true;
        }
        else if (toChamberIndex - fromChamberIndex < -1)
        {
            isHeadingDown = true;
        }
        else
        {
            Debug.Log("SOMETHING WENT WRONG: The corridor does not have a direction to go");
        }

        if (isHeadingUp)
        {
            positionTracker = FindFirstNullBlockVerticalCheck(fromChamberIndex, toChamberIndex);
            yPosTracker = (int)positionTracker.y + 1;
            xPosTracker = (int)positionTracker.x;

            while (true)
            {
                if (yPosTracker < chamberSize)
                {
                    for (int x = 0; x <= 1; x++)
                    {
                        ReplaceTiles(fromChamberIndex, xPosTracker - x, yPosTracker);
                    }
                }
                else
                {
                    if (tileTypes[toChamberIndex, xPosTracker, yPosTracker - chamberSize] != TileType.Empty)
                    {
                        break;
                    }
                    else
                    {
                        for (int x = 0; x <= 1; x++)
                        {
                            ReplaceTiles(toChamberIndex, xPosTracker - x, yPosTracker - chamberSize);
                        }
                    }
                }
                yPosTracker++;
            }
        }
        if (isHeadingDown)
        {
            positionTracker = FindFirstNullBlockVerticalCheck(toChamberIndex, fromChamberIndex);
            yPosTracker = (int)positionTracker.y + 1;
            xPosTracker = (int)positionTracker.x;

            while (true)
            {
                if (yPosTracker < chamberSize)
                {
                    for (int x = 0; x <= 1; x++)
                    {
                        ReplaceTiles(toChamberIndex, xPosTracker - x, yPosTracker);
                    }
                }
                else
                {
                    if (tileTypes[fromChamberIndex, xPosTracker, yPosTracker - chamberSize] != TileType.Empty)
                    {
                        break;
                    }
                    else
                    {
                        for (int x = 0; x <= 1; x++)
                        {
                            ReplaceTiles(fromChamberIndex, xPosTracker - x, yPosTracker - chamberSize);
                        }
                    }
                }
                yPosTracker++;
            }
        }
        if (isHeadingRight)
        {
            positionTracker = FindFirstNullBlockHorizontalCheck(fromChamberIndex, toChamberIndex);
            yPosTracker = (int)positionTracker.y;
            xPosTracker = (int)positionTracker.x + 1;

            while (true)
            {
                if (xPosTracker < chamberSize)
                {
                    for (int y = 0; y <= 1; y++)
                    {
                        ReplaceTiles(fromChamberIndex, xPosTracker, yPosTracker - y);
                    }
                }
                else
                {
                    if (tileTypes[toChamberIndex, xPosTracker - chamberSize, yPosTracker] != TileType.Empty)
                    {
                        break;
                    }
                    else
                    {
                        for (int y = 0; y <= 1; y++)
                        {
                            ReplaceTiles(toChamberIndex, xPosTracker - chamberSize, yPosTracker - y);
                        }
                    }
                }
                xPosTracker++;
            }
        }
        if (isHeadingLeft)
        {
            positionTracker = FindFirstNullBlockHorizontalCheck(toChamberIndex, fromChamberIndex);
            yPosTracker = (int)positionTracker.y;
            xPosTracker = (int)positionTracker.x + 1;

            while (true)
            {
                if (xPosTracker < chamberSize)
                {
                    for (int y = 0; y <= 1; y++)
                    {
                        ReplaceTiles(toChamberIndex, xPosTracker, yPosTracker - y);
                    }
                }
                else
                {
                    if (tileTypes[fromChamberIndex, xPosTracker - chamberSize, yPosTracker] != TileType.Empty)
                    {
                        break;
                    }
                    else
                    {
                        for (int y = 0; y <= 1; y++)
                        {
                            ReplaceTiles(fromChamberIndex, xPosTracker - chamberSize, yPosTracker - y);
                        }
                    }
                }
                xPosTracker++;
            }
        }
    }
    
    private void CreateChamber(int chamberCounter)
    {
        for (int x = 0; x < chamberSize; x++)
        {
            for (int y = 0; y < chamberSize; y++)
            {
                tileTypes[chamberCounter, x, y] = TileType.Empty;
            }
        }
    }


    private Vector2 FindFirstNullBlockVerticalCheck(int firstChamberID, int secondChamberID)
    {
        /*
         * Checker heads down to find the top
         */


        //Variables
        int firstChamberWidth = 0;
        int secondChamberWidth = 0;
        Vector2 searchValue = new Vector2(0, 0);

        //  Set room widths of first and second chamber
        firstChamberWidth = dungeonRoomDimensions[firstChamberID].x;
        secondChamberWidth = dungeonRoomDimensions[secondChamberID].x;

        //  Randomly select a new x position based on the smaller room
        if (firstChamberWidth <= secondChamberWidth)
        {
            searchValue.x = (chamberSize / 2) - (firstChamberWidth / 2) + Random.Range(1, firstChamberWidth);
        }
        else
        {
            searchValue.x = (chamberSize / 2) - (secondChamberWidth / 2) + Random.Range(1, secondChamberWidth);
        }

        //  Find the top edge (which is stored as y in searchValue)
        for (int y = chamberSize - 1; y >= 0; y--)
        {
            if (tileTypes[firstChamberID, (int)searchValue.x, y] != TileType.Empty)
            {
                searchValue.y = y;
                break;
            }
        }

        //  END
        return searchValue;
    }
    private Vector2 FindFirstNullBlockHorizontalCheck(int firstChamberID, int secondChamberID)
    {
        /*
         * Checker heads left to find the right side
         */


        //  Variables
        int firstChamberHeight = 0;
        int secondChamberHeight = 0;
        Vector2 searchValue = new Vector2(0, 0);
        
        //  Set room heights of first and second chamber
        firstChamberHeight = dungeonRoomDimensions[firstChamberID].y;
        secondChamberHeight = dungeonRoomDimensions[secondChamberID].y;

        //  Randomly select a new y position based on the smaller room
        if (firstChamberHeight <= secondChamberHeight)
        {
            searchValue.y = (chamberSize / 2) - (firstChamberHeight / 2) + Random.Range(1, firstChamberHeight);
        }
        else
        {
            searchValue.y = (chamberSize / 2) - (secondChamberHeight / 2) + Random.Range(1, secondChamberHeight);
        }

        //  Find the right edge (which is stored as x in searchValue)
        for (int x = chamberSize - 1; x >= 0; x--)
        {
            if (tileTypes[firstChamberID, x, (int)searchValue.y] != TileType.Empty)
            {
                searchValue.x = x;
                break;
            }
        }

        //  END
        return searchValue;
    }

    private bool ChamberHasRoom(int chamberIndex)
    {
        for (int x = 0; x < chamberSize; x++)
        {
            for (int y = 0; y < chamberSize; y++)
            {
                if (tileTypes[chamberIndex, x, y] == TileType.FloorTile)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public List<Vector3> AvailableDungeonTiles(int chamberIndex)
    {
        //  Count the number of available tiles in the given chamber
        List<Vector3> availableTileCoordinates = new List<Vector3>();

        for (int xPos = 0; xPos < chamberSize; xPos++)
        {
            for (int yPos = 0; yPos < chamberSize; yPos++)
            {
                //  If it is a dungeonTile, add its coordinates to the availableTileCoordinates list
                if (dungeonTiles[chamberIndex, xPos, yPos] != null)
                {
                    availableTileCoordinates.Add(dungeonTiles[chamberIndex, xPos, yPos].transform.position);
                }
            }
        }
        return availableTileCoordinates;
    }
}
