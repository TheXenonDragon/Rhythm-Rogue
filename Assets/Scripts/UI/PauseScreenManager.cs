using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseScreenManager : MonoBehaviour
{
    //  References
    public BeatController beatController;
    private GameManager gameManager;
    private XPManager playerXpManager;
    private HealthManager playerHealthManager;

    private int costMultiplier = 10;


    public GameObject restoreHealthButton;
    public GameObject pauseScreen;
    public KeyCode pauseKey = KeyCode.Escape;
    
    private bool isPaused = false;

    void Start(){
        GameObject player = GameObject.FindObjectOfType<PlayerController>().gameObject;
        playerXpManager =  player.GetComponent<XPManager>();
        playerHealthManager =  player.GetComponent<HealthManager>();

        gameManager = GameObject.FindObjectOfType<GameManager>();
    }

    void Update(){
        if(Input.GetKeyDown(pauseKey)){
            TogglePause();
        }

        MakeRestoreHealthVisibleIfApplicable();
    }

    public void OnHomeButtonClick(){
        TogglePause();
        SceneManager.LoadScene(0);
    }

    public void TogglePause(){
        isPaused = !isPaused;

        if(isPaused){
            pauseScreen.SetActive(true);
            Time.timeScale = 0f;
            beatController.Pause();
        }
        else{
            pauseScreen.SetActive(false);
            Time.timeScale = 1f;
            beatController.Resume();
        }
    }

    public void OnClickRestoreHealth(){
        bool success = playerXpManager.RemoveXP(gameManager.currentLevel * costMultiplier);
        
        if(success){
           playerHealthManager.RestoreHealth();
        }
    }

    private void MakeRestoreHealthVisibleIfApplicable(){
        if((playerHealthManager.HealthCount() == 5) || (playerXpManager.GetXP() < gameManager.currentLevel * costMultiplier)){
            restoreHealthButton.SetActive(false);
        }
        else{
            restoreHealthButton.SetActive(true);
        }
    }
}
