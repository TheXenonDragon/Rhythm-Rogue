using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    //   Enemy Prefabs
    public GameObject skeletonPrefab;

    //  Script and GameObject references
    public BeatController beatController;
    public FloorGenerator floorGenerator;
    public GameObject player;

    //  Spawn Locations
    private List<List<Vector3>> spawnLocations;

    //  Layers to avoid spawning on
    private string obstacleLayer = "Obstacle";
    private string wallLayer = "Wall";
    

    /*  Skeleton    */
    private float skeletonTileSize = 2f;
    private float skeletonMoveSpeed = 5f;
    private float skeletonViewRange = 10f;
    private float skeletonAttackRange = 2f;
    private int skeletonHealth = 1;
    private int skeletonDamage = 1;


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
    private const int currentEnemyCountConstant = 0;

    private int enemyCountLimit;
    private int enemyIncrementSpeed;
    private int currentEnemyCount;



    // Start is called before the first frame update
    void Start()
    {
        Invoke("StartSpawnSequence", 2f);

        delayBetweenSpawns = delayBetweenSpawnsConstant;
        delayBetweenWaves = delayBetweenWavesConstant;
        waveDelayDecrementSpeed = waveDelayDecrementSpeedConstant;

        enemyCountLimit = enemyCountLimitConstant;
        enemyIncrementSpeed = enemyIncrementSpeedConstant;
        currentEnemyCount = currentEnemyCountConstant;
    }

    public void Reset(int level = 1){
        //  Stop spawning
        StopCoroutine("SpawnEnemies");

        //  Delete all current enemies
        for(int i = 0; i < transform.childCount; i++){
            Destroy(transform.GetChild(i).gameObject);
        }

        //  Update stats
        skeletonTileSize = 2f; // * (level);
        skeletonMoveSpeed = 5f; // * (level);
        skeletonViewRange = 10f; // * (level);
        skeletonAttackRange = 2f; // * (level);
        skeletonHealth += 1;    //level;
        skeletonDamage += (level / 7);


        //  Reset
        delayBetweenSpawns = delayBetweenSpawnsConstant;
        delayBetweenWaves = delayBetweenWavesConstant;
        waveDelayDecrementSpeed = waveDelayDecrementSpeedConstant;

        enemyCountLimit = enemyCountLimitConstant;
        enemyIncrementSpeed = enemyIncrementSpeedConstant;
        currentEnemyCount = currentEnemyCountConstant;

        Invoke("StartSpawnSequence", 2f);
    }

    public IEnumerator SpawnEnemies(){
        RaycastHit hit;
        float rayDistance = 100;
        int chamberIndex;
        int tileIndex;

        while(true){
            while(currentEnemyCount < enemyCountLimit){
                //  Randomize the spawn location
                chamberIndex = Random.Range(0, spawnLocations.Count);
                tileIndex = Random.Range(0, spawnLocations[chamberIndex].Count);

                //  Ensure nothing is blocking spawn location
                if (tileIndex < spawnLocations[chamberIndex].Count && Physics.Raycast(spawnLocations[chamberIndex][tileIndex] + (Vector3.up * rayDistance / 2f), Vector3.down, out hit, rayDistance)){
                    //  If there is no wall or obstacle in the spawn location, spawn enemy.
                    if(hit.transform.gameObject.layer != LayerMask.NameToLayer(obstacleLayer) && hit.transform.gameObject.layer != LayerMask.NameToLayer(wallLayer)){
                        //  Spawn then wait.
                        SpawnSkeleton(spawnLocations[chamberIndex][tileIndex], skeletonTileSize, skeletonMoveSpeed, skeletonHealth, skeletonDamage, skeletonViewRange, skeletonAttackRange);
                        currentEnemyCount++;
                        yield return new WaitForSeconds(delayBetweenSpawns);
                    }
                }
            }

            //  Delay between waves
            yield return new WaitForSeconds(delayBetweenWaves);
            delayBetweenWaves -= waveDelayDecrementSpeed;
            enemyCountLimit += enemyIncrementSpeed;
        }
    }

    public void SpawnSkeleton(Vector3 spawnPosition, float tileSize, float moveSpeed, int health, int damage, float viewRange, float attackRange){
        GameObject skeleton = Instantiate<GameObject>(skeletonPrefab, spawnPosition, skeletonPrefab.transform.rotation);
        SkeletonController skeletonController = skeleton.GetComponent<SkeletonController>();

        skeletonController.StartEnemy(spawnPosition + Vector3.up, beatController, player, tileSize, moveSpeed, health, damage, viewRange, attackRange);
        skeleton.transform.parent = this.transform;
    }

    private void GetSpawnLocations(){
        spawnLocations = floorGenerator.GetAvailableDungeonTileLocations();
        float distance = 100;

        //  Remove locations that are filled by obstacles
        RaycastHit hit;
        for(int chamberIndex = 0; chamberIndex < spawnLocations.Count; chamberIndex++){
            for(int tileIndex = spawnLocations[chamberIndex].Count - 1; tileIndex >= 0; tileIndex--){    
                if (Physics.Raycast(spawnLocations[chamberIndex][tileIndex] + (Vector3.up * distance), Vector3.down, out hit, distance)){
                    //  Remove spawn locations blocked by obstacles and walls
                    if(hit.transform.gameObject.layer == LayerMask.NameToLayer(obstacleLayer) || hit.transform.gameObject.layer == LayerMask.NameToLayer(wallLayer)){
                        spawnLocations[chamberIndex].RemoveAt(tileIndex);
                    }
                }
            }
        }
    }
    
    private void StartSpawnSequence(){
        GetSpawnLocations();
        StartCoroutine("SpawnEnemies");
    }
}
