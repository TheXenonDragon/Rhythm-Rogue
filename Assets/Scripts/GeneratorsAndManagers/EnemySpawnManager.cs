using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    //   Enemy Prefabs
    public GameObject skeletonPrefab;
    public GameObject spiderPrefab;
    public GameObject giantPrefab;
    public GameObject bossPrefab;

    //  Script and GameObject references
    public BeatController beatController;
    public FloorGenerator floorGenerator;
    public MapManager mapManager;
    public GameObject player;

    //  Spawn Locations
    private List<Vector2Int> spawnLocations;
    private List<Vector2Int> mediumSpawnLocations;

    //  Tile Size
    private float tileSize = 2f;


    //  Delay
    private const float delayBetweenSpawnsConstant = 5f;   //  If enemies can be produced, they will be spawned every 10 seconds.
    public const float delayBetweenWavesConstant = 60f;  //  Every 60 seconds, the number of enemies will be incremented
    public const float waveDelayDecrementSpeedConstant = 5f;          //  The delayBetweenSpawnIncreases will be reduced by delayDecrementSpeed at each increment.

    private float delayBetweenSpawns;
    private float delayBetweenWaves;
    private float waveDelayDecrementSpeed;
    
    //  Enemy Count
    private const int enemyCountLimitConstant = 10;  //  The max number of enemies able to be spawned.
    private const int enemyIncrementSpeedConstant = 10;

    private int enemyCountLimit;
    private int enemyIncrementSpeed;
    private int currentEnemyCount;



    public void Reset(int level = 1){
        //  Stop spawning
        StopCoroutine(SpawnEnemies());
        
        //  Delete all current enemies
        for(int i = 0; i < transform.childCount; i++){
            Destroy(transform.GetChild(i).gameObject);
        }
        
        //  Reset
        delayBetweenSpawns = delayBetweenSpawnsConstant;
        delayBetweenWaves = delayBetweenWavesConstant;
        waveDelayDecrementSpeed = waveDelayDecrementSpeedConstant;

        enemyCountLimit = enemyCountLimitConstant;
        enemyIncrementSpeed = enemyIncrementSpeedConstant;
        currentEnemyCount = 0;

        GetSpawnLocations();
        StartCoroutine(SpawnEnemies());
    }

    public void SpawnBoss(){
        Vector2Int spawnPosition = mediumSpawnLocations[GenerateRandomSpawnPosition(UnitSize.Medium)];

        GameObject boss = Instantiate<GameObject>(bossPrefab, new Vector3(spawnPosition.x * tileSize, 0.5f, spawnPosition.y * tileSize), bossPrefab.transform.rotation);
        BossController bossController = boss.GetComponent<BossController>();
        boss.name = "Main Boss";

        bossController.StartEnemy(spawnPosition, beatController, mapManager, this, player, tileSize);
        boss.transform.parent = this.transform;
    }

    private void GetSpawnLocations(){
        spawnLocations = mapManager.GetSpawnLocations();
        mediumSpawnLocations = mapManager.GetMediumSpawnLocations();
    }

    public Vector2Int GetNewDestination(){
        return spawnLocations[Random.Range(0, spawnLocations.Count)];
    }

    public IEnumerator SpawnEnemies(){
        int tileIndex;
        UnitSize unitSize;

        while(true){
            while(currentEnemyCount < enemyCountLimit){
                unitSize = EnemySelector();

                //  Randomize the spawn location
                tileIndex = GenerateRandomSpawnPosition(unitSize);

                //  If there is no wall or obstacle in the spawn location, spawn enemy.
                //  Spawn then wait.
                SpawnEnemy(tileIndex, unitSize);
                yield return new WaitForSeconds(delayBetweenSpawns);
            }
            
            //  Delay between waves
            yield return new WaitForSeconds(delayBetweenWaves);
            delayBetweenWaves -= waveDelayDecrementSpeed;
            enemyCountLimit += enemyIncrementSpeed;
        }
    }

    private int GenerateRandomSpawnPosition(UnitSize unitSize){
        //  ERROR Warning: This method may pose an issue of an infinite loop if there are no available spawn locations.

        int tileIndex;
        List<Vector2Int> tempSpawnLocations;
        System.Func<Vector2Int, bool> CheckPos = mapManager.PositionIsEmpty;

        if(unitSize == UnitSize.Regular){
            tempSpawnLocations = spawnLocations;

            while(true){
                //  Randomize the spawn location
                tileIndex = Random.Range(0, tempSpawnLocations.Count);
                if (tileIndex < tempSpawnLocations.Count && CheckPos(tempSpawnLocations[tileIndex])){
                    return tileIndex;
                }
            }
        }
        else{
            tempSpawnLocations = mediumSpawnLocations;

            while(true){
                //  Randomize the spawn location
                tileIndex = Random.Range(0, tempSpawnLocations.Count);
                if (tileIndex < tempSpawnLocations.Count && CheckPos(tempSpawnLocations[tileIndex]) && CheckPos(tempSpawnLocations[tileIndex] + Vector2Int.up) && CheckPos(tempSpawnLocations[tileIndex] + Vector2Int.right) && CheckPos(tempSpawnLocations[tileIndex] + Vector2Int.one)){
                    return tileIndex;
                }
            }
        }
    }

    private UnitSize EnemySelector(){
        int rngNum = Random.Range(0, 10);

        if(rngNum < 3){
            return UnitSize.Regular;
        }
        else if(rngNum < 7){
            return UnitSize.Regular;
        }
        else{
            return UnitSize.Medium;
        }
    }

    private void SpawnEnemy(int tileIndex, UnitSize unitSize){
        int rngNum = Random.Range(0, 10);

        if(unitSize == UnitSize.Regular){
            if(rngNum < 5){
                SpawnSkeleton(spawnLocations[tileIndex], tileSize);
            }
            else{
                SpawnSpider(spawnLocations[tileIndex], tileSize);
            }
        }
        else{
            SpawnGiant(mediumSpawnLocations[tileIndex], tileSize);
        }

        currentEnemyCount++;
    }

    public void SpawnSkeleton(Vector2Int spawnPosition, float tileSize){
        GameObject skeleton = Instantiate<GameObject>(skeletonPrefab, new Vector3(spawnPosition.x * tileSize, 0.5f, spawnPosition.y * tileSize), skeletonPrefab.transform.rotation);
        SkeletonController skeletonController = skeleton.GetComponent<SkeletonController>();
        skeleton.name = "Skeleton #" + currentEnemyCount;

        skeletonController.StartEnemy(spawnPosition, beatController, mapManager, this, player, tileSize);
        skeleton.transform.parent = this.transform;
    }

    public void SpawnSpider(Vector2Int spawnPosition, float tileSize){
        GameObject spider = Instantiate<GameObject>(spiderPrefab, new Vector3(spawnPosition.x * tileSize, 0.5f, spawnPosition.y * tileSize), spiderPrefab.transform.rotation);
        SpiderController spiderController = spider.GetComponent<SpiderController>();
        spider.name = "Spider #" + currentEnemyCount;

        spiderController.StartEnemy(spawnPosition, beatController, mapManager, this, player, tileSize);
        spider.transform.parent = this.transform;
    }

    public void SpawnGiant(Vector2Int spawnPosition, float tileSize){
        GameObject giant = Instantiate<GameObject>(giantPrefab, new Vector3(spawnPosition.x * tileSize, 0.5f, spawnPosition.y * tileSize), giantPrefab.transform.rotation);
        GiantController giantController = giant.GetComponent<GiantController>();
        giant.name = "Giant #" + currentEnemyCount;

        giantController.StartEnemy(spawnPosition, beatController, mapManager, this, player, tileSize);
        giant.transform.parent = this.transform;
    }
}
