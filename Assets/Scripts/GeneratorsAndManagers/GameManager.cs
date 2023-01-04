using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject floorGeneratorPrefab;
    private BeatController beatController;
    private EnemySpawnManager enemySpawnManager;
    private PlayerController playerController;

    public int currentLevel;

    void Start(){
        beatController = GameObject.FindObjectOfType<BeatController>();
        enemySpawnManager = GameObject.FindObjectOfType<EnemySpawnManager>();
        playerController = GameObject.FindObjectOfType<PlayerController>();

        CreateNextLevel();
    }

    public void CreateNextLevel(){
        //  Floor Generator
        try{
            Destroy(GameObject.FindObjectOfType<FloorGenerator>().gameObject);
        } catch(Exception){}
        FloorGenerator floorGeneratorScript = Instantiate<GameObject>(floorGeneratorPrefab, Vector3.zero, floorGeneratorPrefab.transform.rotation, transform).GetComponent<FloorGenerator>();
        floorGeneratorScript.totalSize = floorGeneratorScript.totalSize + currentLevel;    //  Modify with level
        floorGeneratorScript.chamberSize = floorGeneratorScript.chamberSize + ((currentLevel / 2) * 2);    //  Modify with level  (times 2 so that it stays even despite odd numbers working)
        if(floorGeneratorScript.chamberSize > 24){
            floorGeneratorScript.chamberSize = 24;
        }

        //  Enemy spawner
        enemySpawnManager.Reset(currentLevel);
        enemySpawnManager.floorGenerator = floorGeneratorScript;
        
        //  Player
        playerController.Reset();

        //  Destroy all particle effects
        ParticleSystem[] oldParticleEffects = GameObject.FindObjectsOfType<ParticleSystem>();
        for(int i = 0; i < oldParticleEffects.Length; i++){
            Destroy(oldParticleEffects[i].gameObject);
        }

        currentLevel++;

        beatController.Resume();
        Time.timeScale = 1f;
    }
}
