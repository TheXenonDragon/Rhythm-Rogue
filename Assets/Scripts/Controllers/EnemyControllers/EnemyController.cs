using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    //  Script and GameObject references
    private BeatController beatController;
    protected MapManager mapManager;
    protected Pathfinding pathfindingController;
    protected Grid pathfindingGrid;
    private WeaponManager weaponManager;
    private HealthManager healthManager;
    public GameObject player;

    //  Directions
    protected Vector2Int currentPosition;
    protected Vector2Int newPosition;
    protected Vector2Int[] checkpoints;

    protected int checkpointIndex;

    //  Distance
    protected float tileSize = 1.0f;
    protected float moveSpeed = 100.0f;
    private float enemyHeightAboveFloor = 0.5f;

    //  Bools
    private bool hasChosenNextMove = false;
    private bool isBeingAttackedByPlayer = false;
    protected bool attackingPlayer = false;
    protected bool waitingForNewPath = true;

    //  Anonymous Functions
    Func<Vector2Int, Vector2Int, GameObject, bool, bool> InsertUnit;
    Func<Vector2Int, bool> RemoveUnit;
    Func<Vector2Int> GetPlayerPosition;
    
    //  Misc.
    protected enum EnemyState {Wandering, Chasing, Attacking, Guarding, Special}
    protected EnemyState currentState = EnemyState.Wandering;


    // Update is called once per frame
    void Update()
    {
        //  This method is used for asynchronous execution.
        if(!hasChosenNextMove){
            AttemptAttack();
            DetermineNextState();
            hasChosenNextMove = true;
        }
    }

    //  Constructor
    public void StartEnemy(Vector2Int spawnPostion, BeatController beatController, MapManager mapManager, GameObject player, UnitSize unitSize, float tileSize, float moveSpeed, int health, int damage){
        //  Set up
        this.beatController = beatController;
        this.mapManager = mapManager;
        this.player = player;
        this.tileSize = tileSize;
        this.moveSpeed = moveSpeed;

        if(unitSize == UnitSize.Regular){
            InsertUnit = mapManager.InsertUnitPosition;
            RemoveUnit = mapManager.RemoveUnit;
            GetPlayerPosition = mapManager.GetPlayerPosition;
        }
        else{
            InsertUnit = mapManager.InsertMediumUnitPosition;
            RemoveUnit = mapManager.RemoveMediumUnit;
            GetPlayerPosition = mapManager.GetPlayerPositionMedium;
        }

        //  Health management
        healthManager = gameObject.GetComponent<HealthManager>();
        healthManager.SetHealth(health);

        //  Weapon management
        weaponManager = gameObject.GetComponent<WeaponManager>();
        weaponManager.SetInitialWeaponDamage(damage);

        //  Load beat controller
        beatController.BeatExecuted += MoveEnemy;
        beatController.BeatExecuted += CheckForHealth;

        //  Set initial directions
        currentPosition = spawnPostion;
        newPosition = currentPosition;
        checkpoints = new Vector2Int[]{ currentPosition };

        //  Get Scripts
        pathfindingController = GetComponent<Pathfinding>();
        pathfindingGrid = GetComponent<Grid>();
    }


    /*  Movement    */
    //  Update Move and Attacks
    public void MoveEnemy(object sender, EventArgs e){
        if(InsertUnit(newPosition, currentPosition, gameObject, false)){
            currentPosition = newPosition;
            transform.position = (new Vector3(currentPosition.x, 0f, currentPosition.y) * tileSize) + (Vector3.up * enemyHeightAboveFloor);
        }

        hasChosenNextMove = false;
    }

    //  Move Logic
    private void DetermineNextState(){
        ChangeEnemyState();
        if(currentState == EnemyState.Wandering){
            //  Wandering
            Wander();
        }
        else if(currentState == EnemyState.Chasing){
            //  Chasing
            Chase();
            TestForPossibleAttack();
        }
        else if(currentState == EnemyState.Attacking){
            //  Attacking
            Chase();    //  Continue to move if near player.
            TestForPossibleAttack();
        }
        else if(currentState == EnemyState.Guarding){
            //  Guarding
        }
        else{
            //  Special
            PerformSpecial();
        }
    }

    //  Pathfinding
    protected virtual void Wander(){}
    protected virtual void Chase(){
        PathResult pathResult = pathfindingController.FindPath(new PathRequest(currentPosition, GetPlayerPosition(), null));

        checkpoints = pathResult.path;
        checkpointIndex = 1;
        
        if(pathResult.path.Length > 0){
            newPosition = pathResult.path[0];
        }
        else{
            newPosition = currentPosition;
        }
    }
    protected void OnPathFound(Vector2Int[] waypoints, bool pathSuccessful) {
        if (pathSuccessful) {
			checkpoints = waypoints;
		}

        checkpointIndex = 0;
        waitingForNewPath = true;
	}
    
    //  Attack
    protected virtual void TestForPossibleAttack(){}
    private void AttemptAttack(){
        if(attackingPlayer && !isBeingAttackedByPlayer){
            weaponManager.DealDamage(player.gameObject, gameObject);
        }
        attackingPlayer = false;
        isBeingAttackedByPlayer = false;
    }
    
    //  Inherited Methods for Pathfinding and Moving.
    protected virtual void ChangeEnemyState(){}
    protected virtual void PerformSpecial(){}


    /*  Animation   */
    private void StartEnemyMovementAnimation(object sender, EventArgs e){
        StopCoroutine(AnimatePosition());

        StartCoroutine(AnimatePosition());
    }
    private IEnumerator AnimatePosition(){
        float threshold = 0.01f;
        
        Vector3 actualCurrentPosition = (new Vector3(currentPosition.x, 0f, currentPosition.y) * tileSize) + (Vector3.up * enemyHeightAboveFloor);
        while(true){
            if(Vector3.Distance(transform.position, actualCurrentPosition) > threshold) {
                transform.position = Vector3.Lerp(transform.position, actualCurrentPosition, moveSpeed * Time.deltaTime);
            }
            else {
                transform.position = actualCurrentPosition;
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }


    //  Being Damaged
    public void AttackedByPlayer(){
        isBeingAttackedByPlayer = true;
    }
    protected virtual void Die(){
        Debug.Log($"{gameObject.name} has died.");
        RemoveUnit(currentPosition);
        Destroy(gameObject);
    }
    public void CheckForHealth(object sender, EventArgs e){
        if(!healthManager.isAlive()){
            Die();
        }
    }
    void OnDestroy(){
        beatController.BeatExecuted -= MoveEnemy;
        beatController.BeatExecuted -= CheckForHealth;
    }
}
