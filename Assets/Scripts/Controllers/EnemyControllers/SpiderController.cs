using UnityEngine;

public class SpiderController : RegularEnemyController
{
    // Start is called before the first frame update
    void Start()
    {
        currentXPDrop = Random.Range(minXPDrop, maxXPDrop + 1);
    }

    public void StartEnemy(Vector2Int spawnPostion, BeatController beatController, MapManager mapManager, EnemySpawnManager spawnManager, GameObject player, float tileSize){
        //  Base Class Constructor
        base.StartEnemy(spawnPostion, beatController, mapManager, spawnManager, player, tileSize, SpiderData.moveSpeed, SpiderData.health, SpiderData.damage);
        
        //  Data
        this.viewRange = SpiderData.viewRange;
        this.attackRange = SpiderData.attackRange;
        this.attackRange = tileSize * 1.5f;

        //  Scripts
        this.spawnManager = spawnManager;
    }
}
