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
    private WeaponManager weaponManager;
    private HealthManager healthManager;

    //  Cancel key
    public KeyCode cancelMoveKey = KeyCode.LeftShift;

    //  Directions
    private Vector3 moveDirection;
    private Vector3 defaultDirection;
    private Vector3 previousMoveDirection;

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
        previousMoveDirection = new Vector3(0f, 1f, 0f);
        defaultDirection = Vector3.zero;
        moveDirection = Vector3.zero;
        healthManager = gameObject.GetComponent<HealthManager>();
        weaponManager = gameObject.GetComponent<WeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {
        GetUserInput();
        UpdatePosition(false);
    }

    private void GetUserInput(){
        if(Input.anyKey){
            float threshold = 0.01f;

            float xMovement = Input.GetAxisRaw("Horizontal");
            float yMovement = Input.GetAxisRaw("Vertical");
            
            if(xMovement > threshold){
                moveDirection = cameraController.GetTransform().right * tileSize;
            }
            else if(xMovement < -threshold){
                moveDirection = -cameraController.GetTransform().right * tileSize;
            }
            else if(yMovement > threshold){
                moveDirection = cameraController.GetTransform().forward * tileSize;
            }
            else if(yMovement < -threshold){
                moveDirection = -cameraController.GetTransform().forward * tileSize;
            }   
        }

        if(Input.GetKeyDown(cancelMoveKey)){
            moveDirection = defaultDirection;
        }
    }

    public void MovePlayer(object sender, EventArgs e){
        CheckForObstacles(moveDirection);
        UpdatePosition(true);
        previousMoveDirection = moveDirection + previousMoveDirection;
        hasMoved = true;
        CheckForEnemyOverlap();
    }

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

    //  Management
    public void Reset(){
        previousMoveDirection = new Vector3(0f, 1f, 0f);
        defaultDirection = Vector3.zero;
        moveDirection = Vector3.zero;
    }

    //  Accessors
    public Vector3 GetCurrentLocation(){
        return previousMoveDirection;
    }

    public Vector3 GetNextMoveDirection(){
        return moveDirection;
    }


    //  Movement control
    private void CheckForObstacles(Vector3 direction){
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(direction.normalized), out hit, tileSize))
        {
            Attack(hit.transform.gameObject);
            AttemptToOpenChest(hit.transform.gameObject);
            AttemptToActivatePortal(hit.transform.gameObject);
            
            moveDirection = defaultDirection;
        }
    }

    private void UpdatePosition(bool finishMove){
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
    private void Attack(GameObject objectToAttack){
        if(objectToAttack.layer == LayerMask.NameToLayer(enemyLayer)){
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
}
