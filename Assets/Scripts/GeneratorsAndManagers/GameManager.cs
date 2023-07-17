using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    float waitDelay = 0.1f;

    public BeatController beatController;
    public FloorGenerator floorGenerator;
    public BossRoomGenerator bossRoomGenerator;
    public MapManager mapManager;
    public ChunkingManager chunkingManager;
    public PathRequestManager pathRequestManager;
    public FieldOfViewController fieldOfViewController;
    public PlayerController playerController;
    public GameObject canvasObject;
    public EnemySpawnManager enemySpawnManager;

    public int currentLevel;

    void Start(){
        CreateNextLevel();
    }

    private IEnumerator SetUpNextLevel(){
        //  Turn off all scripts.
        floorGenerator.gameObject.SetActive(false);
        bossRoomGenerator.gameObject.SetActive(false);
        mapManager.gameObject.SetActive(false);
        chunkingManager.gameObject.SetActive(false);
        pathRequestManager.gameObject.SetActive(false);
        fieldOfViewController.gameObject.SetActive(false);
        playerController.gameObject.SetActive(false);
        canvasObject.SetActive(false);
        enemySpawnManager.gameObject.SetActive(false);

        //  Start beat controller then pause
        beatController.Pause();

        //  Create floor
        floorGenerator.gameObject.SetActive(true);
        //  Modify totalSize and chamberSize with level  (times 2 for chamberSize so that it stays even despite odd numbers working)
        floorGenerator.ResetFloorGenerator(floorGenerator.totalSize + currentLevel, floorGenerator.chamberSize + ((currentLevel / 2) * 2));
        while(true){
            //  Do not proceed until FloorGenerator has finished constructing the map layout.
            if(floorGenerator.FloorGenerationIsComplete()){
                break;
            }
            else{
                yield return new WaitForSeconds(waitDelay);
            }
        }
        
        //  Start Map Manager
        mapManager.gameObject.SetActive(true);
        mapManager.UpdateMapLayout(floorGenerator.GetDungeonLayoutMapForPathfinding());

        //  Start Chunk Manager
        chunkingManager.gameObject.SetActive(true);
        chunkingManager.LoadMap(floorGenerator.GetDungeonLayoutMap(), (new Vector2Int(1, 1) * Mathf.RoundToInt(floorGenerator.chamberSize / 2f)));
        while(true){
            //  Do not proceed until FloorGenerator has finished constructing the map layout.
            if(chunkingManager.CompletedReset()){
                break;
            }
            else{
                yield return new WaitForSeconds(waitDelay);
            }
        }

        //  Load Grid with map
        pathRequestManager.gameObject.SetActive(true);
        pathRequestManager.UpdateGrid(mapManager.GetMap(), mapManager.GetMediumMap());
        
        //  Start Field of View Controller
        //fieldOfViewController.gameObject.SetActive(true);
        //fieldOfViewController.Reset(floorGenerator);

        //  Start Player Controller
        playerController.gameObject.SetActive(true);
        playerController.Reset(new Vector3(1f, 0f, 1f) * (floorGenerator.chamberSize / 2f));

        //  Start Player Hud
        canvasObject.SetActive(true);
        
        //  Start Enemy Spawn Manager
        enemySpawnManager.gameObject.SetActive(true);
        enemySpawnManager.Reset(currentLevel);

        //  Destroy all particle effects
        ParticleSystem[] oldParticleEffects = GameObject.FindObjectsOfType<ParticleSystem>();   // is deleting exit portal
        for(int i = 0; i < oldParticleEffects.Length; i++){
            Destroy(oldParticleEffects[i].gameObject);
        }

        //  Increment Level
        currentLevel++;

        //  Unpause beat controller
        beatController.Resume();
        Time.timeScale = 1f;
    }

    private IEnumerator SetUpNextBoss(){
        //  Turn off all scripts.
        floorGenerator.gameObject.SetActive(false);
        bossRoomGenerator.gameObject.SetActive(false);
        mapManager.gameObject.SetActive(false);
        chunkingManager.gameObject.SetActive(false);
        pathRequestManager.gameObject.SetActive(false);
        fieldOfViewController.gameObject.SetActive(false);
        playerController.gameObject.SetActive(false);
        canvasObject.SetActive(false);
        enemySpawnManager.gameObject.SetActive(false);

        //  Start beat controller then pause
        beatController.Pause();

        //  Create Boss room layout
        bossRoomGenerator.gameObject.SetActive(true);

        //  Start Map Manager
        mapManager.gameObject.SetActive(true);
        mapManager.UpdateMapLayout(bossRoomGenerator.GetBossRoomLayoutForPathfinding(0));

        //  Start Chunk Manager
        chunkingManager.gameObject.SetActive(true);
        chunkingManager.LoadMap(bossRoomGenerator.GetBossRoomLayout(0), (new Vector2Int(64/2, 10)));
        while(true){
            //  Do not proceed until FloorGenerator has finished constructing the map layout.
            if(chunkingManager.CompletedReset()){
                break;
            }
            else{
                yield return new WaitForSeconds(waitDelay);
            }
        }

        //  Load Grid with map
        pathRequestManager.gameObject.SetActive(true);
        pathRequestManager.UpdateGrid(mapManager.GetMap(), mapManager.GetMediumMap());

        //  Start Player Controller
        playerController.gameObject.SetActive(true);
        playerController.Reset(new Vector3(1f, 0f, 1f) * (floorGenerator.chamberSize / 2f));

        //  Start Player Hud
        canvasObject.SetActive(true);

        //  Start Enemy Spawn Manager
        enemySpawnManager.gameObject.SetActive(true);
        enemySpawnManager.Reset(currentLevel);
        enemySpawnManager.SpawnBoss();

        //  Destroy all particle effects
        ParticleSystem[] oldParticleEffects = GameObject.FindObjectsOfType<ParticleSystem>();   // is deleting exit portal
        for(int i = 0; i < oldParticleEffects.Length; i++){
            Destroy(oldParticleEffects[i].gameObject);
        }

        //  Unpause beat controller
        beatController.Resume();
        Time.timeScale = 1f;
    }

    public void CreateNextLevel(){
        StartCoroutine(SetUpNextLevel());
    }

    public void CreateBossLevel(){
        StartCoroutine(SetUpNextBoss());
    }
}
