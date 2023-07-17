using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    //  Active Objects
    private TileType[,,] tileTypes;     //  The first dimension is the chamber Index (in order to connect to the correct chamber).  The second dimension is the x position of the tile, and the third dimension is the y position of the tile.
    private Vector2Int[] dungeonRoomDimensions;
    private List<CorridorEntryExitPoints> corridorEntryExitLocations;
    private GameObject[] chambers;

    //  Stats for Furnishings
    public int averageBoxRatePerRoom = 12;
    public float chestChancePerChamber = 0.25f;

    //  Indices
    private int exitRoomChamberIndex = -1;

    //  Misc.
    private bool completedFloorGeneration = false;


    /*  Methods */
    //  Public Methods
    public TileType[,] GetDungeonLayoutMapForPathfinding(){
        TileType[,] map = GetDungeonLayoutMap();
        int xDisplacement = (exitRoomChamberIndex % totalSize) * chamberSize;
        int yDisplacement = (exitRoomChamberIndex / totalSize) * chamberSize;

        //  Add exit portal tiles for sake of pathfinding
        for(int x = (chamberSize / 2) - 2; x < (chamberSize / 2) + 2; x++){
            for(int y = (chamberSize / 2) - 2; y < (chamberSize / 2) + 2; y++){
                map[(x + xDisplacement), (y + yDisplacement)] = TileType.ExitPortal;
            }
        }

        return map;
    }

    public TileType[,] GetDungeonLayoutMap(){
        TileType[,] map = new TileType[totalSize * chamberSize, totalSize * chamberSize];

        //  Convert to a single map
        for(int chamberIndex = 0; chamberIndex < totalSize * totalSize; chamberIndex++){
            for(int x = 0; x < tileTypes.GetLength(1); x++){
                for(int y = 0; y < tileTypes.GetLength(2); y++){
                    map[((chamberIndex % totalSize) * chamberSize) + x, ((chamberIndex / totalSize) * chamberSize) + y] = tileTypes[chamberIndex, x, y];
                }
            }
        }

        return map;
    }

    public bool FloorGenerationIsComplete(){
        return completedFloorGeneration;
    }

    public void ResetFloorGenerator(int totalSize, int chamberSize){
        completedFloorGeneration = false;

        //  Size
        this.totalSize = totalSize;
        this.chamberSize = chamberSize;

        //  Indices
        exitRoomChamberIndex = -1;

        //  Active Objects
        chambers = new GameObject[(totalSize * totalSize) + 1];
        tileTypes = new TileType[chambers.Length, chamberSize, chamberSize];
        dungeonRoomDimensions = new Vector2Int[chambers.Length];
        corridorEntryExitLocations = new List<CorridorEntryExitPoints>();

        CreateLevel();
    }


    //  Primary Methods
    public void CreateLevel()
    {
        int chamberID = 0;

        CreateFloorPlan();          //  Prepare chambers and parent to floorPlan.
        GenerateRooms(chamberID);   //  Select rooms and create path between them.
        //FurnishInterior();
        //ClearPaths();
        CreateExitPortal();

        completedFloorGeneration = true;
    }

    private void CreateFloorPlan()
    {
        int chamberCounter = 0;

        for (int j = 0; j < totalSize; j++)
        {
            for (int i = 0; i < totalSize; i++)
            {
                SetChamberTilesToDefault(chamberCounter);                           //  Determine tile placement in the chamber
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

            //  Remove the chamber which led to this one from the available chambers.
            for(int i = nearbyRoomsIndexs.Count - 1; i >= 0; i--)
            {
                if (lastChamberIndex == nearbyRoomsIndexs[i])
                {
                    nearbyRoomsIndexs.RemoveAt(i);
                }
            }

            //  Create rooms for each available chamber index.
            for (int i = 0; i < nearbyRoomsIndexs.Count; i++)
            {
                CreateRoom(nearbyRoomsIndexs[i]);
                CreateCorridor(index, nearbyRoomsIndexs[i]);
            }

            //  Assign the current chamber index to the lastChamberIndex so that it can be checked on the next iteration
            lastChamberIndex = index;

            //  Pass on the room generation to the next chambers if greater than 0, else it is the exit chamber.
            if(nearbyRoomsIndexs.Count < 1)
            {
                exitRoomChamberIndex = index;
                CreateRoom(exitRoomChamberIndex);   //  Create exit chamber.
                break;
            }
            else
            {
                index = nearbyRoomsIndexs[Random.Range(0, nearbyRoomsIndexs.Count)];
                nearbyRoomsIndexs.Clear();
            }
        } while (true);
    }
    
    private void ClearPaths(){
        //  Remove all faulty coords (if surrounded by walls, remove the entire data entry).
        bool isSurroundedByWalls = true;
        Vector2Int[] checkDirections = new Vector2Int[]{Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down};
        
        for(int i = corridorEntryExitLocations.Count - 1; i >= 0; i--){
            isSurroundedByWalls = true;
            for(int j = 0; j < checkDirections.Length; j++){
                if(tileTypes[corridorEntryExitLocations[i].fromChamberIndex, corridorEntryExitLocations[i].corridorEntryLocation.x, corridorEntryExitLocations[i].corridorEntryLocation.y] != TileType.Wall){
                    isSurroundedByWalls = false;
                    break;
                }
            }

            if(isSurroundedByWalls){
                corridorEntryExitLocations.RemoveAt(i);
                continue;
            }

            isSurroundedByWalls = true;
            for(int j = 0; j < checkDirections.Length; j++){
                if(tileTypes[corridorEntryExitLocations[i].toChamberIndex, corridorEntryExitLocations[i].corridorExitLocation.x, corridorEntryExitLocations[i].corridorExitLocation.y] != TileType.Wall){
                    isSurroundedByWalls = false;
                    break;
                }
            }

            if(isSurroundedByWalls){
                corridorEntryExitLocations.RemoveAt(i);
                continue;
            }
        }


        for(int i = 0; i < corridorEntryExitLocations.Count; i++){
            ClearRoom(i, true, new Vector2Int(chamberSize / 2, chamberSize / 2));   //  Corridor Index, clean fromChamber, chamberCenter
            ClearRoom(i, false, new Vector2Int(chamberSize / 2, chamberSize / 2));  //  Corridor Index, clean toChamber, chamberCenter

            ClearCorridor(i);

            //tileTypes[corridorEntryExitLocations[i].fromChamberIndex, corridorEntryExitLocations[i].corridorEntryLocation.x, corridorEntryExitLocations[i].corridorEntryLocation.y] = TileType.Pillar;
            //tileTypes[corridorEntryExitLocations[i].toChamberIndex, corridorEntryExitLocations[i].corridorExitLocation.x, corridorEntryExitLocations[i].corridorExitLocation.y] = TileType.Pillar;
        }
    }

    private void FurnishInterior()
    {
        int tileCounter;
        int numFurnishings;
        List<int[]> availableTileCoordinates;

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
                    tileTypes[availableTileCoordinates[index][0], availableTileCoordinates[index][1], availableTileCoordinates[index][2]] = TileType.Chest;
                    availableTileCoordinates.RemoveAt(index);
                }
            }

            //  Add Boxes
            numFurnishings = tileCounter / averageBoxRatePerRoom;
            for(int i = 0; i < numFurnishings; i++)
            {
                int index = Random.Range(0, availableTileCoordinates.Count);
                tileTypes[availableTileCoordinates[index][0], availableTileCoordinates[index][1], availableTileCoordinates[index][2]] = TileType.Box;
                availableTileCoordinates.RemoveAt(index);
            }

            //  Clean List
            availableTileCoordinates.Clear();
        }
    }
    
    private void CreateExitPortal(){
        //  Ensure that tiles directly around the portal and beneath it are classified as FloorTiles
        //  so that obstacles do not spawn there.
        for(int x = (chamberSize / 2) - 3; x < (chamberSize / 2) + 3; x++){
            for(int y = (chamberSize / 2) - 3; y < (chamberSize / 2) + 3; y++){
                tileTypes[exitRoomChamberIndex, x, y] = TileType.FloorTile;
            }
        }

        //  Set the exit portal position.
        tileTypes[exitRoomChamberIndex, (chamberSize / 2) - 1, (chamberSize / 2) - 1] = TileType.ExitPortal;
    }

    



    //  Helper Methods
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

            //  Add the entry location of the corridor.
            corridorEntryExitLocations.Add(new CorridorEntryExitPoints(fromChamberIndex, toChamberIndex, new Vector2Int(xPosTracker, yPosTracker), Vector2Int.zero));
            
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
                    if (tileTypes[toChamberIndex, xPosTracker, yPosTracker - chamberSize] != TileType.Wall)
                    {
                        //  Add the exit location of the corridor and break the loop.
                        corridorEntryExitLocations[corridorEntryExitLocations.Count - 1].corridorExitLocation = new Vector2Int(xPosTracker, yPosTracker - chamberSize - 1);
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

            //  Add the entry location of the corridor.
            corridorEntryExitLocations.Add(new CorridorEntryExitPoints(fromChamberIndex, toChamberIndex, new Vector2Int(xPosTracker, yPosTracker), Vector2Int.zero));

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
                    if (tileTypes[fromChamberIndex, xPosTracker, yPosTracker - chamberSize] != TileType.Wall)
                    {
                        //  Add the exit location of the corridor and break the loop.
                        corridorEntryExitLocations[corridorEntryExitLocations.Count - 1].corridorExitLocation = new Vector2Int(xPosTracker, yPosTracker - chamberSize - 1);
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

            //  Add the entry location of the corridor.
            corridorEntryExitLocations.Add(new CorridorEntryExitPoints(fromChamberIndex, toChamberIndex, new Vector2Int(xPosTracker, yPosTracker), Vector2Int.zero));

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
                    if (tileTypes[toChamberIndex, xPosTracker - chamberSize, yPosTracker] != TileType.Wall)
                    {
                        //  Add the exit location of the corridor and break the loop.
                        corridorEntryExitLocations[corridorEntryExitLocations.Count - 1].corridorExitLocation = new Vector2Int(xPosTracker - chamberSize - 1, yPosTracker);
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

            //  Add the entry location of the corridor.
            corridorEntryExitLocations.Add(new CorridorEntryExitPoints(fromChamberIndex, toChamberIndex, new Vector2Int(xPosTracker, yPosTracker), Vector2Int.zero));

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
                    if (tileTypes[fromChamberIndex, xPosTracker - chamberSize, yPosTracker] != TileType.Wall)
                    {
                        //  Add the exit location of the corridor and break the loop.
                        corridorEntryExitLocations[corridorEntryExitLocations.Count - 1].corridorExitLocation = new Vector2Int(xPosTracker - chamberSize - 1, yPosTracker);
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
    
    private void ReplaceTiles(int chamberID, int blockXPos, int blockYPos)
    {
        tileTypes[chamberID, blockXPos, blockYPos] = TileType.FloorTile;
    }
    private void SetChamberTilesToDefault(int chamberCounter)
    {
        /*  Set all tiles to TileType.Wall as default for each chamber. */
        
        for (int x = 0; x < chamberSize; x++)
        {
            for (int y = 0; y < chamberSize; y++)
            {
                tileTypes[chamberCounter, x, y] = TileType.Wall;
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
            if (tileTypes[firstChamberID, (int)searchValue.x, y] != TileType.Wall)
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
            if (tileTypes[firstChamberID, x, (int)searchValue.y] != TileType.Wall)
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
    private List<int[]> AvailableDungeonTiles(int chamberIndex)
    {
        //  Count the number of available tiles in the given chamber
        List<int[]> availableTileCoordinates = new List<int[]>();

        for (int xPos = 0; xPos < chamberSize; xPos++)
        {
            for (int yPos = 0; yPos < chamberSize; yPos++)
            {
                //  If it is a dungeonTile, add its coordinates to the availableTileCoordinates list
                if (tileTypes[chamberIndex, xPos, yPos] == TileType.FloorTile)
                {
                    availableTileCoordinates.Add(new int[]{chamberIndex, xPos, yPos});
                }
            }
        }
        return availableTileCoordinates;
    }
    
    private void ClearRoom(int corridorIndex, bool isFromChamber, Vector2Int destinationPosition){
        //  Clear path from center of room to each corridor.
        Vector2Int[] newCleanDirections = new Vector2Int[]{Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down};
        Vector2Int closestCleanPosition = new Vector2Int(chamberSize * 10, chamberSize * 10);
        Vector2Int cleanPosition;
        Vector2Int tempCleanPosition;
        int chamberIndex;
        
        if(isFromChamber){
            cleanPosition = corridorEntryExitLocations[corridorIndex].corridorEntryLocation;
            chamberIndex = corridorEntryExitLocations[corridorIndex].fromChamberIndex;
        }
        else{
            cleanPosition = corridorEntryExitLocations[corridorIndex].corridorExitLocation;
            chamberIndex = corridorEntryExitLocations[corridorIndex].toChamberIndex;
        }

        while(true){
            //  Set currently selected tile to be a FloorTile
            tileTypes[chamberIndex, cleanPosition.x, cleanPosition.y] = TileType.FloorTile;

            if(Vector2.Distance(destinationPosition, cleanPosition) < 1f){
                break;
            }

            //  Select a new direction
            for(int k = 0; k < newCleanDirections.Length; k++){
                tempCleanPosition = newCleanDirections[k] + cleanPosition;

                //  Ensure the new direction is not out of bounds
                if((tempCleanPosition.x < chamberSize && tempCleanPosition.y < chamberSize) && (tempCleanPosition.x > 0 && tempCleanPosition.y > 0)){
                    //  Do not select a wall tile
                    
                    if(tileTypes[chamberIndex, tempCleanPosition.x, tempCleanPosition.y] != TileType.Wall){
                        //  Determine if the new direction is closest to the center of the chamber
                        if(Vector2.Distance(destinationPosition, tempCleanPosition) < Vector2.Distance(destinationPosition, closestCleanPosition)){
                            closestCleanPosition = tempCleanPosition;
                        }
                    }
                }
            }

            //  Set next cleaning position
            cleanPosition = closestCleanPosition;

            //  Reset closestCleanPosition to max for next iteration.
            closestCleanPosition = new Vector2Int(chamberSize * 10, chamberSize * 10);
        }
    }
    private void ClearCorridor(int corridorIndex){
        //  Clear each corridor.
        float chanceOfdeviation = 25f;
        Vector2Int cleanPosition = corridorEntryExitLocations[corridorIndex].corridorEntryLocation;
        Vector2Int destinationPosition = corridorEntryExitLocations[corridorIndex].corridorExitLocation;
        int destinationChamberIndex = corridorEntryExitLocations[corridorIndex].toChamberIndex;
        int cachedChamberIndex = corridorEntryExitLocations[corridorIndex].fromChamberIndex;;

        if(destinationPosition.x == cleanPosition.x){
            //  Vertical Corridor
            
            while(true){
                //  Set currently selected tile to be a FloorTile
                tileTypes[cachedChamberIndex, cleanPosition.x, cleanPosition.y] = TileType.FloorTile;

                //  Exit the loop if the corridor has been cleared up to the corridor exit position.
                if(cleanPosition.y == destinationPosition.y){
                    break;
                }

                //  Add random movement occasionally
                if(Random.value < (chanceOfdeviation / 100f)){
                    //  50% chance to right and 50% to go left
                    if(Random.value < 0.5f){
                        //  Ensure not to erase a wall
                        if(tileTypes[cachedChamberIndex, cleanPosition.x + 1, cleanPosition.y] != TileType.Wall){
                            cleanPosition += Vector2Int.right;
                            continue;
                        }
                    }
                    else{
                        //  Ensure not to erase a wall
                        if(tileTypes[cachedChamberIndex, cleanPosition.x - 1, cleanPosition.y] != TileType.Wall){
                            cleanPosition += Vector2Int.left;
                            continue;
                        }
                    }
                }

                //  If random movement did not occur, then proceed down corridor.
                if(destinationPosition.y < corridorEntryExitLocations[corridorIndex].corridorEntryLocation.y){
                    //  Heading up
                    cleanPosition += Vector2Int.up;
                }
                else{
                    //  Heading down
                    cleanPosition += Vector2Int.down;
                }
                

                //  Change to the toChamber once the end of the fromChamber has been cleared.
                if(cleanPosition.y >= chamberSize){
                    cachedChamberIndex = destinationChamberIndex;
                    cleanPosition.y = 0;
                }
            }
        }
        else{
            //  Horizontal Corridor

            while(true){
                //  Set currently selected tile to be a FloorTile
                tileTypes[cachedChamberIndex, cleanPosition.x, cleanPosition.y] = TileType.FloorTile;

                //  Exit the loop if the corridor has been cleared up to the corridor exit position.
                if(cleanPosition.x == destinationPosition.x){
                    break;
                }

                //  Add random movement occasionally
                if(Random.value < (chanceOfdeviation / 100f)){
                    //  50% chance to go up and 50% to go down
                    if(Random.value < 0.5f){
                        //  Ensure not to erase a wall
                        if(tileTypes[cachedChamberIndex, cleanPosition.x, cleanPosition.y + 1] != TileType.Wall){
                            cleanPosition += Vector2Int.up;
                            continue;
                        }
                    }
                    else{
                        //  Ensure not to erase a wall
                        if(tileTypes[cachedChamberIndex, cleanPosition.x, cleanPosition.y - 1] != TileType.Wall){
                            cleanPosition += Vector2Int.down;
                            continue;
                        }
                    }
                }

                //  If random movement did not occur, then proceed down corridor.
                if(destinationPosition.x < corridorEntryExitLocations[corridorIndex].corridorEntryLocation.x){
                    //  Heading right
                    cleanPosition += Vector2Int.right;
                }
                else{
                    //  Heading left
                    cleanPosition += Vector2Int.left;
                }
                

                //  Change to the toChamber once the end of the fromChamber has been cleared.
                if(cleanPosition.x >= chamberSize){
                    cachedChamberIndex = destinationChamberIndex;
                    cleanPosition.x = 0;
                }
            }
        }
    }
}
