using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    //  Script and GameObject references
    private BeatController beatController;
    private NavigationUnitController navUnit;
    private PlayerController playerController;
    private WeaponManager weaponManager;
    private HealthManager healthManager;
    public GameObject player;

    //  Directions
    private Vector3 moveDirection;
    private Vector3 defaultDirection;
    protected Vector3 previousMoveDirection;
    protected Vector3[] checkpoints;

    protected int checkpointIndex;

    //  Distance
    protected float tileSize = 1.0f;
    protected float moveSpeed = 100.0f;
    private float minDistanceToCheckpoint = 2.5f;

    //  Bools
    private bool hasMoved = false;
    protected bool checkpointsNeedToBeDetected = true;
    private bool isBeingAttackedByPlayer = false;

    //  Attack
    private string playerLayer = "Player";
    
    private string tileLayer = "Tile";

    //  Misc.
    protected enum EnemyState {Wandering, Chasing, Attacking, Guarding, Special}
    protected EnemyState currentState = EnemyState.Wandering;




    // Update is called once per frame
    void Update()
    {
        AnimatePosition(false);
    }

    public void StartEnemy(Vector3 spawnPostion, BeatController beatController, GameObject player, float tileSize, float moveSpeed, int health, int damage){
        //  Set up
        this.beatController = beatController;
        this.player = player;
        this.tileSize = tileSize;
        this.moveSpeed = moveSpeed;

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
        previousMoveDirection = spawnPostion;
        defaultDirection = Vector3.zero;
        moveDirection = Vector3.zero;

        //  Get Scripts
        navUnit = transform.GetChild(0).gameObject.GetComponent<NavigationUnitController>();
        playerController = player.GetComponent<PlayerController>();
    }

    public void MoveEnemy(object sender, EventArgs e){
        AnimatePosition(true);
        VerifyChamberIsActive();
        previousMoveDirection = moveDirection + previousMoveDirection;
        hasMoved = true;
        DetermineNextMove();
        CheckForPlayerOverlap();
        isBeingAttackedByPlayer = false;
    }

    private void VerifyChamberIsActive(){
        RaycastHit hit;
        Vector3 offset = Vector3.up * 10f;
        
        if(!Physics.Raycast(previousMoveDirection + moveDirection + offset, Vector3.down, out hit, 20f, 1 << LayerMask.NameToLayer(tileLayer))){
            moveDirection = defaultDirection;
        }
    }

    private void DetermineNextMove(){
        ChangeEnemyState();

        if(currentState == EnemyState.Wandering){
            //  Wandering
            StartCoroutine(CreatePathToTarget(new Vector3(UnityEngine.Random.Range(0f, 200f), 0f, UnityEngine.Random.Range(0f, 200f))));
        }
        else if(currentState == EnemyState.Chasing){
            //  Chasing
            StartCoroutine(CreatePathToTarget(player.transform.position));
        }
        else if(currentState == EnemyState.Attacking){
            //  Attacking
            CloseRangeMovement();
        }
        else if(currentState == EnemyState.Guarding){
            //  Guarding
        }
        else{
            //  Special
            PerformSpecial();
        }
    }

    protected virtual void PerformSpecial(){}
    protected virtual void ChangeEnemyState(){}

    

    private IEnumerator CreatePathToTarget(Vector3 destination){
        List<Vector3> possibleMoveDirections = NonObstaclePositions(); 

        //  Increment checkpoint if too close to current checkpoint
        if(!checkpointsNeedToBeDetected && checkpointIndex < checkpoints.Length && Vector3.Distance(checkpoints[checkpointIndex], previousMoveDirection) < minDistanceToCheckpoint){
            checkpointIndex++;
        }

        //  If the final checkpoint has been reached, find another.
        if(checkpointsNeedToBeDetected || checkpointIndex >= checkpoints.Length){
            yield return StartCoroutine(UpdatePath(destination));
            checkpointsNeedToBeDetected = false;
        }

        //  Determine which tile is the closest to the checkpoint
        if(possibleMoveDirections.Count == 0){
            moveDirection = defaultDirection;
        }
        else{
            int selectedIndex = 0;
            for(int i = 0; i < possibleMoveDirections.Count; i++){
                //  Start at 1 to save time because 0 has already been selected.
                if(Vector3.Distance(checkpoints[checkpointIndex], possibleMoveDirections[i] + previousMoveDirection) < Vector3.Distance(checkpoints[checkpointIndex], possibleMoveDirections[selectedIndex] + previousMoveDirection)){
                    selectedIndex = i;
                }
            }
            moveDirection = possibleMoveDirections[selectedIndex];
        }
    }

    private IEnumerator UpdatePath(Vector3 destination){
        float waitTime = 0.05f;

        StartCoroutine(navUnit.CalculatePositionsArray(destination, previousMoveDirection));

        while(!navUnit.PositionsArrayHasBeenCalculated()){
            yield return new WaitForSeconds(waitTime);
        }
        checkpoints = navUnit.GetPositionsArray();
        checkpointIndex = 1;
    }

    private List<Vector3> NonObstaclePositions(){
        Vector3[] tempArray = {Vector3.forward * tileSize, Vector3.back * tileSize, Vector3.left * tileSize, Vector3.right * tileSize};
        List<Vector3> result = new List<Vector3>();

        RaycastHit hit;
        for(int i = 0; i < tempArray.Length; i++){
            if (!Physics.Raycast(previousMoveDirection, transform.TransformDirection(tempArray[i].normalized), out hit, tileSize))
            {
                result.Add(tempArray[i]);
            }
            else if(hit.transform.gameObject == gameObject){
                //  If the object detected is this object, ignore and add possible direction.
                result.Add(tempArray[i]);
            }
        }

        return result;
    }

    private void AnimatePosition(bool finishMove){
        float threshold = 0.01f;

        if(finishMove){
            transform.position = previousMoveDirection;
            hasMoved = false;
        }

        if(hasMoved){
            if(Vector3.Distance(transform.position, previousMoveDirection) > threshold) {
                transform.position = Vector3.Lerp(transform.position, previousMoveDirection, moveSpeed * Time.deltaTime);
            }
            else {
                transform.position = previousMoveDirection;
                hasMoved = false;
            }
        }
    }

    

    

    //  Attack
    private void CloseRangeMovement(){
        //  Attack if close enough or Close remaining distance
        RaycastHit hit;

        Vector3 direction = transform.TransformDirection((playerController.GetCurrentLocation() - previousMoveDirection).normalized);
        float threshold = 0.75f;
        if(Mathf.Abs(direction.x) > threshold || Mathf.Abs(direction.z) > threshold){
            if(Mathf.Abs(direction.x) > threshold){
                if(direction.x > threshold){
                    //  Right
                    moveDirection = Vector3.right;
                }
                else{
                    //  Left
                    moveDirection = Vector3.left;
                }
            }
            else{
                if(direction.z > threshold){
                    //  Forward
                    moveDirection = Vector3.forward;
                }
                else{
                    //  Backward
                    moveDirection = Vector3.back;
                }
            }
        }
        else{
            if(direction.x > 0){
                if(direction.z > 0){
                    //  Up-right
                    if (!Physics.Raycast(previousMoveDirection, transform.TransformDirection(Vector3.right), out hit, tileSize)){
                        moveDirection = Vector3.right;
                    }
                    else{
                        moveDirection = Vector3.forward;
                    }
                }
                else{
                    //  Down-right
                    if (!Physics.Raycast(previousMoveDirection, transform.TransformDirection(Vector3.back), out hit, tileSize)){
                        moveDirection = Vector3.back;
                    }
                    else{
                        moveDirection = Vector3.right;
                    }
                }
            }
            else{
                if(direction.z > 0){
                    //  Up-left
                    if (!Physics.Raycast(previousMoveDirection, transform.TransformDirection(Vector3.forward), out hit, tileSize)){
                        moveDirection = Vector3.forward;
                    }
                    else{
                        moveDirection = Vector3.left;
                    }
                }
                else{
                    //  Down-left
                    if (!Physics.Raycast(previousMoveDirection, transform.TransformDirection(Vector3.left), out hit, tileSize)){
                        moveDirection = Vector3.left;
                    }
                    else{
                        moveDirection = Vector3.back;
                    }
                }
            }
        }

        moveDirection *= tileSize;

        CheckForObstaclesAtCloseRange(moveDirection);
    }

    private void CheckForObstaclesAtCloseRange(Vector3 direction){
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(direction.normalized), out hit, tileSize))
        {
            Attack(hit.transform.gameObject);
            moveDirection = defaultDirection;
        }
    }

    void CheckForPlayerOverlap()
    {
        Collider[] colliders = Physics.OverlapBox(gameObject.transform.position, transform.localScale, Quaternion.identity);
        for(int i = 0; i < colliders.Length; i++){
            Attack(colliders[i].gameObject);
        }
    }

    private void Attack(GameObject objectToAttack){
        if(objectToAttack.layer == LayerMask.NameToLayer(playerLayer)){
            if(!isBeingAttackedByPlayer){
                weaponManager.DealDamage(objectToAttack, gameObject);
            }
        }
    }


    //  Being Damaged
    public void AttackedByPlayer(){
        isBeingAttackedByPlayer = true;
    }

    protected virtual void Die(){
        Debug.Log($"{gameObject.name} has died.");
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
        navUnit.DestroyNavUnit();
    }
}
