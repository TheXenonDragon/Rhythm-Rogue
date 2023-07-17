using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorridorEntryExitPoints
{
    public int fromChamberIndex;
    public int toChamberIndex;

    public Vector2Int corridorEntryLocation;
    public Vector2Int corridorExitLocation;

    public CorridorEntryExitPoints(int fromChamberIndex, int toChamberIndex, Vector2Int corridorEntryLocation, Vector2Int corridorExitLocation){
        this.fromChamberIndex = fromChamberIndex;
        this.toChamberIndex = toChamberIndex;
        this.corridorEntryLocation = corridorEntryLocation;
        this.corridorExitLocation = corridorExitLocation;
    }
}
