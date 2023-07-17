using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonController : RegularEnemyController
{
    // Start is called before the first frame update
    void Start()
    {
        currentXPDrop = Random.Range(minXPDrop, maxXPDrop + 1);
    }

    public void StartEnemy(Vector2Int spawnPostion, BeatController beatController, MapManager mapManager, EnemySpawnManager spawnManager, GameObject player, float tileSize){
        //  Base Class Constructor
        base.StartEnemy(spawnPostion, beatController, mapManager, spawnManager, player, tileSize, SkeletonData.moveSpeed, SkeletonData.health, SkeletonData.damage);
        
        //  Data
        this.viewRange = SkeletonData.viewRange;
        this.attackRange = SkeletonData.attackRange;
        this.attackRange = tileSize * 1.5f;

        //  Scripts
        this.spawnManager = spawnManager;
    }
}
