using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    /*  Fields */
    //  Regular Map
    private TileType[,] mapLayout;
    private PositionState[,] unitPositions;
    private GameObject[,] unitObjectPositions;

    //  Medium Unit Map
    private TileType[,] mediumMapLayout;
    private PositionState[,] mediumUnitPositions;
    private GameObject[,] mediumUnitObjectPositions;

    //  Misc.
    private Vector2Int playerPosition = Vector2Int.zero;
    protected enum PositionState {Empty, Unit, Player}


    /*  Methods */
    //  Main Reset Method
    public void UpdateMapLayout(TileType[,] tempMap){
        SetUpRegularMap(tempMap);
        SetUpMediumMap(tempMap);
    }

    //  Create Regular and Medium Sized Unit Map
    private void SetUpRegularMap(TileType[,] tempMap){
        //  Regular Map
        mapLayout = tempMap;
        unitPositions = new PositionState[mapLayout.GetLength(0), mapLayout.GetLength(1)];
        unitObjectPositions = new GameObject[mapLayout.GetLength(0), mapLayout.GetLength(1)];
    }
    private void SetUpMediumMap(TileType[,] tempMap){
        //  Medium Unit Map
        mediumMapLayout = new TileType[tempMap.GetLength(0), tempMap.GetLength(1)];
        
        TileType tempTile;
        for(int x = mediumMapLayout.GetLength(0) - 1; x >= 1; x--){
            for(int y = mediumMapLayout.GetLength(1) - 1; y >= 1; y--){
                tempTile = tempMap[x, y];
                if(tempTile != TileType.FloorTile){
                    mediumMapLayout[x, y] = tempTile;
                    mediumMapLayout[x - 1, y] = tempTile;
                    mediumMapLayout[x, y - 1] = mapLayout[x, y];
                    mediumMapLayout[x - 1, y - 1] = mapLayout[x, y];
                }
            }
        }

        //  Horizontal Copy
        for(int x = 0; x < mediumMapLayout.GetLength(0); x++){
            mediumMapLayout[x, 0] = tempMap[x, 0];
        }

        //  Vertical Copy
        for(int y = 0; y < mediumMapLayout.GetLength(1); y++){
            mediumMapLayout[0, y] = tempMap[0, y];
        }

        mediumUnitPositions = new PositionState[tempMap.GetLength(0), tempMap.GetLength(1)];
        mediumUnitObjectPositions = new GameObject[tempMap.GetLength(0), tempMap.GetLength(1)];
    }

    //  Debug Methods
    private void PrintDebugMap(GameObject obj){
        string map = "" + obj.name + "\n";

        for(int x = 0; x < mapLayout.GetLength(0); x++){
            for(int y = 0; y < mapLayout.GetLength(0); y++){
                if(unitPositions[x, y] == PositionState.Player){
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(0f), (byte)(255f), (byte)(0f), "+ ");
                }
                else if(unitPositions[x, y] == PositionState.Unit){
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(255f), (byte)(0f), (byte)(0f), "+ ");
                }
                else if(mapLayout[x, y] == TileType.Wall){
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(0f), (byte)(0f), (byte)(0f), "= ");
                }
                else if(mapLayout[x, y] == TileType.FloorTile){
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(100f), (byte)(100f), (byte)(100f), "+ ");
                }
                else if(mapLayout[x, y] == TileType.ExitPortal){
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(0f), (byte)(100f), (byte)(200f), "+ ");
                }
                else {
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(255f), (byte)(255f), (byte)(0f), "+ ");
                }
            }
            map += "\n";
        }

        Debug.Log(map);
    }
    private void PrintMediumDebugMap(GameObject obj){
        string map = "" + obj.name + "\n";

        for(int x = 0; x < mediumMapLayout.GetLength(0); x++){
            for(int y = 0; y < mediumMapLayout.GetLength(0); y++){
                if(mediumUnitPositions[x, y] == PositionState.Player){
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(0f), (byte)(255f), (byte)(0f), "+ ");
                }
                else if(mediumUnitPositions[x, y] == PositionState.Unit){
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(255f), (byte)(0f), (byte)(0f), "+ ");
                }
                else if(mediumMapLayout[x, y] == TileType.Wall){
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(0f), (byte)(0f), (byte)(0f), "= ");
                }
                else if(mediumMapLayout[x, y] == TileType.FloorTile){
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(100f), (byte)(100f), (byte)(100f), "+ ");
                }
                else if(mediumMapLayout[x, y] == TileType.ExitPortal){
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(0f), (byte)(100f), (byte)(200f), "+ ");
                }
                else {
                    map += string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", (byte)(255f), (byte)(255f), (byte)(0f), "+ ");
                }
            }
            map += "\n";
        }

        Debug.Log(map);
    }

    //  Getter Methods
    public int GetMapSize(){
        return mapLayout.GetLength(0);
    }
    public TileType[,] GetMap(){
        return mapLayout;
    }
    public TileType[,] GetMediumMap(){
        return mediumMapLayout;
    }

    //  Unit Getter Methods
    public GameObject GetUnitGameObject(Vector2Int position){
        //  This method returns a unit's gameObject if it is at the given coordinates.
        //  Otherwise, the method returns null.
        int x = position.x;
        int y = position.y;

        //  Return false for all coordinates outside of the map.
        if((x < unitPositions.GetLength(0) && y < unitPositions.GetLength(1)) && (x >= 0 && y >= 0)){
            return unitObjectPositions[x, y];           
        }
        else{
            //  Coordinates out of bounds. 
            return null;
        }
    }
    public Vector2Int GetPlayerPosition(){
        return playerPosition;
    }
    public Vector2Int GetPlayerPositionMedium(){
        int x = playerPosition.x;
        int y = playerPosition.y;

        if(mediumMapLayout[x, y] != TileType.Wall){
            //  If the current position is not blocked by a wall in the medium map, then return the current player position.
            return playerPosition;
        }
        else if((x - 1 >= 0) && mediumMapLayout[x - 1, y] != TileType.Wall){
            //  If the current player position is blocked by a wall in the medium map but the position -1 on the x-axis
            //  is not, then return the current player position minus 1 on the x-axis.
            return new Vector2Int(x - 1, y);
        }
        else if((y - 1 >= 0) && mediumMapLayout[x, y - 1] != TileType.Wall){
            //  If the current player position is blocked by a wall in the medium map but the position -1 on the y-axis
            //  is not, then return the current player position minus 1 on the y-axis.
            return new Vector2Int(x, y - 1);
        }
        else if ((x - 1 >= 0) && (y - 1 >= 0) && mediumMapLayout[x - 1, y - 1] != TileType.Wall){
            //  If a wall is blocking the current player position, -1 shifted on the x-axis from the current player position,
            //  and -1 shifted on the y-axis from the current player position all in the medium map, but not the current 
            //  player position -1 shifted on the x-axis and y-axis, then return the current player position minus 1 on 
            //  the x-axis and the y-axis.
            return new Vector2Int(x - 1, y - 1);
        }
        else{
            //  If all else fails, return the actual current player position.  This is redundant but will prevent further complications.
            return playerPosition;
        }
    }

    //  Position Methods
    public List<Vector2Int> GetSpawnLocations(){
        List<Vector2Int> spawnList = new List<Vector2Int>();
        
        for(int x = 0; x < mapLayout.GetLength(0); x++){
            for(int y = 0; y < mapLayout.GetLength(1); y++){
                if(mapLayout[x, y] != TileType.Wall){
                    spawnList.Add(new Vector2Int(x, y));
                }
            }
        }

        return spawnList;
    }

    public List<Vector2Int> GetMediumSpawnLocations(){
        List<Vector2Int> spawnList = new List<Vector2Int>();
        
        for(int x = 0; x < mediumMapLayout.GetLength(0); x++){
            for(int y = 0; y < mediumMapLayout.GetLength(1); y++){
                if(mediumMapLayout[x, y] != TileType.Wall){
                    spawnList.Add(new Vector2Int(x, y));
                }
            }
        }

        return spawnList;
    }

    //  Insert and Remove Units
    public bool InsertUnitPosition(Vector2Int toPosition, Vector2Int fromPosition, GameObject unitObject, bool isPlayer){
        int toX = toPosition.x;
        int toY = toPosition.y;
        int fromX = fromPosition.x;
        int fromY = fromPosition.y;

        if(PositionIsEmpty(toPosition)){
            unitPositions[fromX, fromY] = PositionState.Empty;
            unitObjectPositions[fromX, fromY] = null;
            unitObjectPositions[toX, toY] = unitObject;
            
            if(isPlayer){
                playerPosition = new Vector2Int(toX, toY);
                unitPositions[toX, toY] = PositionState.Player;
            }
            else{
                unitPositions[toX, toY] = PositionState.Unit;
            }

            return true;
        }
        else{
            return false;
        }
    }
    public bool InsertMediumUnitPosition(Vector2Int toPosition, Vector2Int fromPosition, GameObject unitObject, bool throwAwayVar){
        int toX = toPosition.x;
        int toY = toPosition.y;
        int fromX = fromPosition.x;
        int fromY = fromPosition.y;

        if(PositionIsEmptyMediumUnit(toPosition, unitObject)){
            //  Clear Old Position
            unitPositions[fromX, fromY] = PositionState.Empty;
            unitPositions[fromX + 1, fromY] = PositionState.Empty;
            unitPositions[fromX, fromY + 1] = PositionState.Empty;
            unitPositions[fromX + 1, fromY + 1] = PositionState.Empty;

            unitObjectPositions[fromX, fromY] = null;
            unitObjectPositions[fromX + 1, fromY] = null;
            unitObjectPositions[fromX, fromY + 1] = null;
            unitObjectPositions[fromX + 1, fromY + 1] = null;
            
            //  Set New Position
            unitPositions[toX, toY] = PositionState.Unit;
            unitPositions[toX + 1, toY] = PositionState.Unit;
            unitPositions[toX, toY + 1] = PositionState.Unit;
            unitPositions[toX + 1, toY + 1] = PositionState.Unit;

            unitObjectPositions[toX, toY] = unitObject;
            unitObjectPositions[toX + 1, toY] = unitObject;
            unitObjectPositions[toX, toY + 1] = unitObject;
            unitObjectPositions[toX + 1, toY + 1] = unitObject;
            
            return true;
        }
        else{
            return false;
        }
    }
    
    public bool RemoveUnit(Vector2Int position){
        //  Removes unit at given position by setting unit position to empty and the unitGameObject to null.
        //  If successful, returns true.  If no unit exists in the given position 
        //  or was otherwise unable to complete the removal, false is returned.
        int x = position.x;
        int y = position.y;

        //  Return false for all coordinates outside of the map.
        if((x < unitPositions.GetLength(0) && y < unitPositions.GetLength(1)) && (x >= 0 && y >= 0)){
            if(unitPositions[x, y] == PositionState.Unit){
                unitObjectPositions[x, y] = null;
                unitPositions[x, y] = PositionState.Empty;
                return true;    //  Success
            }           
        }

        //  Did not successfully complete removal.
        return false;
    }
    public bool RemoveMediumUnit(Vector2Int position){
        //  Removes unit at given position by setting unit position to empty and the unitGameObject to null.
        //  If successful, returns true.  If no unit exists in the given position 
        //  or was otherwise unable to complete the removal, false is returned.
        int x = position.x;
        int y = position.y;

        //  Positions to check.
        Vector2Int center = new Vector2Int(x, y);
        Vector2Int up = new Vector2Int(x, y + 1);
        Vector2Int right = new Vector2Int(x + 1, y);
        Vector2Int upRight = new Vector2Int(x + 1, y + 1);
        
        Vector2Int[] checkPositions = new Vector2Int[]{ center, up, right, upRight };

        //  For the positions that are not held by this unit, check if position is empty.
        //  If the position is empty, return true.  Otherwise, return false.
        //  If a tile is held by another unit, return false.

        //  Check all 4 positions. 
        foreach(Vector2Int checkPos in checkPositions){
            //  If any of the 4 positions return false, then return false for this method.
            if(!RemoveUnit(checkPos)){
                return false;
            }
        }

        //  If there have been no issues so far, the positions have all been cleared.  Return true.
        return true;
    }

    //  Unit Checks
    public bool PositionIsEmpty(Vector2Int position){
        //  This method checks if the position at the coordinates is empty.
        //  If it is empty, the method returns true.  Otherwise, the method returns false.
        int x = position.x;
        int y = position.y;

        //  Return false for all coordinates outside of the map.
        if((x < unitPositions.GetLength(0) && y < unitPositions.GetLength(1)) && (x >= 0 && y >= 0)){
            if(mapLayout[x, y] != TileType.FloorTile || unitPositions[x, y] != PositionState.Empty){
                //  If this position is not a floor tile or a unit already exists in this position, return false.
                return false;
            }
            else{
                //  No unit exists in this position and it is a floor tile, so return true.
                return true;
            }
        }
        else{
            //  Map is empty or position out of bounds.
            return false;
        }
    }
    public bool PositionIsEmptyMediumUnit(Vector2Int position, GameObject unitObject){
        //  This method checks if the position at the coordinates is empty.
        //  If it is empty, the method returns true.  Otherwise, the method returns false.
        int x = position.x;
        int y = position.y;

        //  Positions to check.
        Vector2Int center = new Vector2Int(x, y);
        Vector2Int up = new Vector2Int(x, y + 1);
        Vector2Int right = new Vector2Int(x + 1, y);
        Vector2Int upRight = new Vector2Int(x + 1, y + 1);
        
        Vector2Int[] checkPositions = new Vector2Int[]{ center, up, right, upRight };

        //  For the positions that are not held by this unit, check if position is empty.
        //  If the position is empty, return true.  Otherwise, return false.
        //  If a tile is held by another unit, return false.

        //  Return false for all coordinates outside of the map.
        if((x >= (unitPositions.GetLength(0) - 1)) || (x < 0) || (y >= (unitPositions.GetLength(1) - 1)) || (y < 0)){
            return false;
        }

        //  Check all 4 positions. 
        foreach(Vector2Int checkPos in checkPositions){
            //  If the unit issuing the check is not in this position, make an additional check.
            if(unitObjectPositions[checkPos.x, checkPos.y] != unitObject){
                //  If the position is not empty, return false.
                if(!PositionIsEmpty(checkPos)){
                    return false;
                }
            }
        }

        //  If there have been no issues so far, the positions are empty.  Return true.
        return true;
    }
    public bool PositionHasPlayer(Vector2Int position){
        //  This method checks if the player is at the given coordinates.
        //  If the player is, the method returns true.  Otherwise, the method returns false.
        int x = position.x;
        int y = position.y;

        //  Return false for all coordinates outside of the map.
        if((x < unitPositions.GetLength(0) && y < unitPositions.GetLength(1)) && (x >= 0 && y >= 0)){
            if(unitPositions[x, y] == PositionState.Player){
                //  If the player exists in this position, return true.
                return true;
            }
            else{
                //  The player does not exist in this position, so return false.
                return false;
            }
        }
        else{
            //  Coordinates out of bounds. 
            return false;
        }
    }
    public bool PositionHasUnit(Vector2Int position){
        //  This method checks if a unit is at the given coordinates.
        //  If a unit is, the method returns true.  Otherwise, the method returns false.
        int x = position.x;
        int y = position.y;

        //  Return false for all coordinates outside of the map.
        if((x < unitPositions.GetLength(0) && y < unitPositions.GetLength(1)) && (x >= 0 && y >= 0)){
            if(unitPositions[x, y] == PositionState.Unit){
                //  If unit exists in this position, return true.
                return true;
            }
            else{
                //  A unit does not exist in this position, so return false.
                return false;
            }
        }
        else{
            //  Coordinates out of bounds. 
            return false;
        }
    }

    //  Obstacle Checks
    public bool PositionHasChest(Vector2Int position){
        //  This method checks if a chest is at the given coordinates.
        //  If a chest is, the method returns true.  Otherwise, the method returns false.
        int x = position.x;
        int y = position.y;

        //  Return false for all coordinates outside of the map.
        if((x < unitPositions.GetLength(0) && y < unitPositions.GetLength(1)) && (x >= 0 && y >= 0)){
            if(mapLayout[x, y] == TileType.Chest){
                //  If a chest exists in this position, return true and remove the chest from the position.
                mapLayout[x, y] = TileType.FloorTile;
                return true;
            }
            else{
                //  A chest does not exist in this position, so return false.
                return false;
            }
        }
        else{
            //  Coordinates out of bounds. 
            return false;
        }
    }
    public bool PositionHasExitPortal(Vector2Int position){
        //  This method checks if an exit portal is at the given coordinates.
        //  If an exit portal is, the method returns true.  Otherwise, the method returns false.
        int x = position.x;
        int y = position.y;

        //  Return false for all coordinates outside of the map.
        if((x < unitPositions.GetLength(0) && y < unitPositions.GetLength(1)) && (x >= 0 && y >= 0)){
            if(mapLayout[x, y] == TileType.ExitPortal){
                //  If an exit portal exists in this position, return true.
                return true;
            }
            else{
                //  An exit portal does not exist in this position, so return false.
                return false;
            }
        }
        else{
            //  Coordinates out of bounds. 
            return false;
        }
    }
}
