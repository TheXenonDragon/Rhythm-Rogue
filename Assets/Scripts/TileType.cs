using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    FloorTile, 
    Wall, 

    Box,
    Crate,
    Barrel,
    BenchHorizontal,
    BenchVertical,
    SmallTableHorizontal,
    SmallTableVertical,
    LargeTableHorizontal,
    LargeTableVertical,
    SideTable,
    Chair,
    Throne,
    SmallBookShelfHorizontal,
    SmallBookShelfVertical,
    LargeBookShelfHorizontal,
    LargeBookShelfVertical,
    Well,
    SmallPillar,
    MediumPillar,
    LargePillar,
    SmallBarricadeHorizontal,
    SmallBarricadeVertical,
    LargeBarricadeHorizontal,
    LargeBarricadeVertical,

    Chest,
    ExitPortal
}

public class TileTypeDimensions{
    private static Vector2Int[] dimensions = {
        new Vector2Int(1,1), //  FloorTile
        new Vector2Int(1,1), //  Wall

        new Vector2Int(1,1), //  Box
        new Vector2Int(2,2), //  Crate
        new Vector2Int(1,1), //  Barrel
        new Vector2Int(2,1), //  BenchHorizontal
        new Vector2Int(1,2), //  BenchVertical
        new Vector2Int(4,3), //  SmallTableHorizontal
        new Vector2Int(3,4), //  SmallTableVertical
        new Vector2Int(6,3), //  LargeTableHorizontal
        new Vector2Int(3,6), //  LargeTableVertical
        new Vector2Int(1,1), //  SideTable
        new Vector2Int(1,1), //  Chair
        new Vector2Int(1,1), //  Throne
        new Vector2Int(1,1), //  SmallBookShelfHorizontal
        new Vector2Int(1,1), //  SmallBookShelfVertical
        new Vector2Int(1,1), //  LargeBookShelfHorizontal
        new Vector2Int(1,1), //  LargeBookShelfVertical
        new Vector2Int(1,1), //  Well
        new Vector2Int(1,1), //  SmallPillar
        new Vector2Int(2,2), //  MediumPillar
        new Vector2Int(3,3), //  LargePillar
        new Vector2Int(2,1), //  SmallBarricadeHorizontal
        new Vector2Int(1,2), //  SmallBarricadeVertical
        new Vector2Int(4,1), //  LargeBarricadeHorizontal
        new Vector2Int(1,4), //  LargeBarricadeVertical

        new Vector2Int(1,1), //  Chest
        new Vector2Int(4,4), //  ExitPortal
    };

    public static Vector2Int GetDimensions(int index){
        return dimensions[index];
    }
}