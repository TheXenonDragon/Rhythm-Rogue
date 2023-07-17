using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantController : MediumEnemyController
{
    // Start is called before the first frame update
    void Start()
    {
        currentXPDrop = Random.Range(minXPDrop, maxXPDrop + 1);
    }

    public void StartEnemy(Vector2Int spawnPostion, BeatController beatController, MapManager mapManager, EnemySpawnManager spawnManager, GameObject player, float tileSize){
        //  Base Class Constructor
        base.StartEnemy(spawnPostion, beatController, mapManager, spawnManager, player, tileSize, GiantData.moveSpeed, GiantData.health, GiantData.damage);
        
        //  Data
        this.viewRange = GiantData.viewRange;
        this.attackRange = GiantData.attackRange;
        this.attackRange = tileSize * 1.5f * 3;

        //  Scripts
        this.spawnManager = spawnManager;
    }
}
