using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoomGenerator : MonoBehaviour
{
    public TileType[,] GetBossRoomLayout(int levelIndex){
        //  Only accounts for spawn locations of obstacles.  
        //  Must use Pathfinding method for correctly sized obstacle footprints.

        string[] stringMapRows = (new Regex(";")).Split(RoomLayout.GetBossRoomLayout(levelIndex));
        string[] stringMapColumns;
        List<List<string>> stringMap = new List<List<string>>();

        //  Split rows into columns
        for(int row = 0; row < stringMapRows.Length - 1; row++){
            stringMap.Add(new List<string>());

            stringMapColumns = (new Regex(",")).Split(stringMapRows[row]);
            for(int col = 0; col < stringMapColumns.Length - 1; col++){
                stringMap[row].Add(stringMapColumns[col]);
            }
        }

        //  Convert from string to TileType
        TileType[,] map = new TileType[stringMap[0].Count,stringMap.Count];
        int tileTypeIntegerValue;

        for(int row = 0; row < map.GetLength(0); row++){
            for(int col = 0; col < map.GetLength(1); col++){
                Int32.TryParse(stringMap[row][col], out tileTypeIntegerValue);
                map[col, row] = (TileType)tileTypeIntegerValue;
            }
        }

        //  Return result
        return map;
    }

    public TileType[,] GetBossRoomLayoutForPathfinding(int index){
        //  Creates map with accurate obstacle sizes (footprints).
        return SetTileSizesForObstacles(GetBossRoomLayout(index));
    }

    private TileType[,] SetTileSizesForObstacles(TileType[,] map){
        //  This method updates tile sizes to reflect their actual dimensions.
        TileType[,] mapCopy = new TileType[map.GetLength(0), map.GetLength(1)];

        Vector2Int dimensions;
        TileType tempTileType;

        for(int row = 0; row < map.GetLength(0); row++){
            for(int col = 0; col < map.GetLength(1); col++){
                tempTileType = map[col, row];
                
                if(tempTileType != TileType.FloorTile && tempTileType != TileType.Wall){
                    dimensions = TileTypeDimensions.GetDimensions((int)tempTileType);
                    
                    for(int x = 0; x < dimensions.x; x++){
                        for(int y = 0; y < dimensions.y; y++){
                            mapCopy[col + x, row - y] = tempTileType;
                        }
                    }
                }
            }
        }

        return mapCopy;
    }
}
