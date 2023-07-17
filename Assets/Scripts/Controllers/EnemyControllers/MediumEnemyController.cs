using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediumEnemyController : EnemyController
{
    //  Scripts
    protected EnemySpawnManager spawnManager;

    //  Attacking
    public float viewRange;
    public float attackRange;

    //  Drops
    public float chanceOfWeaponDrop = 0.1f;
    protected int weaponLevelDrop = 1;
    protected int minXPDrop = 1;
    protected int maxXPDrop = 5;
    protected int currentXPDrop;



    public void StartEnemy(Vector2Int spawnPostion, BeatController beatController, MapManager mapManager, EnemySpawnManager spawnManager, GameObject player, float tileSize, float moveSpeed, int health, int damage){
        //  Base Class Constructor
        base.StartEnemy(spawnPostion, beatController, mapManager, player, UnitSize.Medium, tileSize, moveSpeed, health, damage);
        
        //  Scripts
        this.spawnManager = spawnManager;
    }


    //  Logic
    protected override void ChangeEnemyState(){
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection((player.transform.position - transform.position).normalized), out hit, viewRange)){
            if(hit.transform.gameObject == player.gameObject){
                if(Vector3.Distance(player.transform.position, transform.position) < attackRange){
                    //  Attacking: Player is within range
                    base.currentState = EnemyController.EnemyState.Attacking;
                }
                else{
                    //  Chasing: Check for player
                    base.currentState = EnemyController.EnemyState.Chasing;
                }
            }
            else{
                //  Wandering:
                base.currentState = EnemyController.EnemyState.Wandering;
            }
        }
        else{
            //  Wandering:
            base.currentState = EnemyController.EnemyState.Wandering;
        }
    }


    //  Pathfinding
    protected override void Wander(){
        if(waitingForNewPath && checkpointIndex >= checkpoints.Length){
            checkpoints = new Vector2Int[]{ currentPosition };
            PathRequestManager.RequestPath (new PathRequest(currentPosition, spawnManager.GetNewDestination(), OnPathFound), false);
            waitingForNewPath = false;
        }
        else{
            newPosition = checkpoints[checkpointIndex];
            checkpointIndex++;
        }
    }
    protected override void Chase(){
        pathfindingGrid.CreateGrid(mapManager.GetMediumMap());
        base.Chase();   
    }


    //  Attack
    protected override void TestForPossibleAttack(){
        Vector2Int[] directions = new Vector2Int[]{ Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up};
        Vector2Int[] checkPositions = new Vector2Int[]{ Vector2Int.zero, Vector2Int.right, Vector2Int.up, Vector2Int.one};

        foreach(Vector2Int checkPosition in checkPositions){
            foreach (Vector2Int direction in directions){
                if (mapManager.PositionHasPlayer(currentPosition + checkPosition + direction))
                {
                    attackingPlayer = true;
                    newPosition = currentPosition; //   If attacking, prevent movement.
                    break;
                }
            }
        }
    }


    //  Death Management
    protected override void Die()
    {
        //  Add weapon levels if applicable
        if(Random.Range(0f, 1f) < chanceOfWeaponDrop){
            player.GetComponent<WeaponManager>().AddWeaponLevel(weaponLevelDrop);
        }

        //  Add XP
        player.GetComponent<XPManager>().AddXP(currentXPDrop);

        base.Die();
    }
}
