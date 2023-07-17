using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class PlayerController : MonoBehaviour
{
    //  GameObject references
    public BeatController beatController;
    public CameraController cameraController;
    public MapManager mapManager;
    private WeaponManager weaponManager;
    private HealthManager healthManager;

    //  Cancel key
    public KeyCode cancelMoveKey = KeyCode.LeftShift;

    //  Directions
    private Vector2Int moveDirection;
    private Vector2Int defaultDirection;
    private Vector2Int currentPosition;

    //  Distance
    public float tileSize = 1.0f;
    public float moveSpeed = 100.0f;

    private bool hasMoved = false;

    //  Attack
    private string enemyLayer = "Enemy";

    int floorlevel = 1;


    // Start is called before the first frame update
    void Start()
    {
        beatController.BeatExecuted += MovePlayer;
        beatController.BeatExecuted += CheckForHealth;
        healthManager = gameObject.GetComponent<HealthManager>();
        weaponManager = gameObject.GetComponent<WeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {
        GetUserInput();
        UpdatePosition(false);
    }


    //  Management
    public void Reset(Vector3 moveOffset){
        currentPosition = new Vector2Int(Mathf.RoundToInt(moveOffset.x), Mathf.RoundToInt(moveOffset.z));
        mapManager.InsertUnitPosition(currentPosition, currentPosition, gameObject, true);
        transform.position = (new Vector3(currentPosition.x, 0f, currentPosition.y) * tileSize) + Vector3.up;
        defaultDirection = Vector2Int.zero;
        moveDirection = Vector2Int.zero;
    }


    //  Movement control
    private void GetUserInput(){
        if(Input.anyKey){
            float threshold = 0.01f;

            float xMovement = Input.GetAxisRaw("Horizontal");
            float yMovement = Input.GetAxisRaw("Vertical");

            Transform camTransform = cameraController.GetTransform();
            
            if(xMovement > threshold){
                moveDirection = new Vector2Int(Mathf.RoundToInt(camTransform.right.x), Mathf.RoundToInt(camTransform.right.z));
            }
            else if(xMovement < -threshold){
                moveDirection = -(new Vector2Int(Mathf.RoundToInt(camTransform.right.x), Mathf.RoundToInt(camTransform.right.z)));
            }
            else if(yMovement > threshold){
                moveDirection = new Vector2Int(Mathf.RoundToInt(camTransform.forward.x), Mathf.RoundToInt(camTransform.forward.z));
            }
            else if(yMovement < -threshold){
                moveDirection = -(new Vector2Int(Mathf.RoundToInt(camTransform.forward.x), Mathf.RoundToInt(camTransform.forward.z)));
            }   
        }

        if(Input.GetKeyDown(cancelMoveKey)){
            moveDirection = defaultDirection;
        }
    }

    public void MovePlayer(object sender, EventArgs e){
        AttemptMove(moveDirection);
        
        //UpdatePosition(true);
        //hasMoved = true;
        CheckForEnemyOverlap();
    }

    private void AttemptMove(Vector2Int direction){
        //  Attempt to move and check for obstacles.

        if(mapManager.InsertUnitPosition(moveDirection + currentPosition, currentPosition, gameObject, true)){
            //  Successful move
            currentPosition = moveDirection + currentPosition;
            transform.position = (new Vector3(currentPosition.x, 0f, currentPosition.y) * tileSize) + Vector3.up;
        }
        else{
            //  Obstacle or enemy in position
            RaycastHit hit;

            if(mapManager.PositionHasUnit(currentPosition + direction)){
                Attack(mapManager.GetUnitGameObject(currentPosition + direction));
            }
            else if (Physics.Raycast(transform.position, transform.TransformDirection((new Vector3(direction.x, 0f, direction.y)).normalized), out hit, tileSize))
            {
                if(mapManager.PositionHasChest(currentPosition + direction)){
                    AttemptToOpenChest(hit.transform.gameObject);
                }
                else if(mapManager.PositionHasExitPortal(currentPosition + direction)){
                    AttemptToActivatePortal(hit.transform.gameObject);
                }
            }
            
            moveDirection = defaultDirection;
        }
    }

    private void UpdatePosition(bool finishMove){
        float threshold = 0.01f;

        Vector3 actualPosition = (new Vector3(currentPosition.x, 0f, currentPosition.y) * tileSize) + Vector3.up;

        if(finishMove){
            transform.position = actualPosition;
            hasMoved = false;
        }

        if(hasMoved){
            if(Vector3.Distance(transform.position, actualPosition) > threshold) {
                transform.position = Vector3.Lerp(transform.position, actualPosition, moveSpeed * Time.deltaTime);
            }
            else {
                transform.position = actualPosition;
                hasMoved = false;
            }
        }
    }


    //  Object Activation
    private void AttemptToOpenChest(GameObject possibleChest){
        ChestManager chestManager = possibleChest.GetComponent<ChestManager>();

        if(chestManager != null){
            chestManager.OpenChest(gameObject);
        }
    }
    private void AttemptToActivatePortal(GameObject possiblePortal){
        ExitPortalManager exitPortalManager = possiblePortal.GetComponent<ExitPortalManager>();

        if(exitPortalManager != null){
            floorlevel++;
            exitPortalManager.Activate();
        }
    }


    //  Attack
    private void Attack(GameObject objectToAttack){
        if(objectToAttack != null && objectToAttack.layer == LayerMask.NameToLayer(enemyLayer)){
            EnemyController enemyController = objectToAttack.GetComponent<EnemyController>();
            if(enemyController != null){
                enemyController.AttackedByPlayer();
            }

            weaponManager.DealDamage(objectToAttack, gameObject);
        }
    }
    void CheckForEnemyOverlap()
    {
        Collider[] colliders = Physics.OverlapBox(gameObject.transform.position, transform.localScale, Quaternion.identity);
        for(int i = 0; i < colliders.Length; i++){
            Attack(colliders[i].gameObject);
        }
    }


    //  Being Attacked
    protected void Die(){
        beatController.BeatExecuted -= MovePlayer;
        beatController.BeatExecuted -= CheckForHealth;
        Invoke("GameOver", 1f);
        Debug.Log($"{gameObject.name} has died.");
    }
    public void CheckForHealth(object sender, EventArgs e){
        if(!healthManager.isAlive()){
            Die();
        }
    }
    private void GameOver(){
        string saveScoreFilePath = Application.persistentDataPath + "/saveFiles.json";

        if(File.Exists(saveScoreFilePath)){
            SaveFilesModel saveFile = JsonUtility.FromJson<SaveFilesModel>(File.ReadAllText(saveScoreFilePath));
            
            //  Update current player save file.
            saveFile.currentPlayer.currentXpScore = GetComponent<XPManager>().GetXP();
            saveFile.currentPlayer.currentFloorScore = floorlevel;

            //  Write to memory
            File.WriteAllText(saveScoreFilePath, JsonUtility.ToJson(saveFile));
        }

        SceneManager.LoadScene(2);
    }


    //  Accessors
    public Vector2Int GetCurrentLocation(){
        return currentPosition;
    }
}
