using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitPortalManager : MonoBehaviour
{
    private BeatController beatController;
    private NextLevelScreenManager nextLevelScreenManager;
    private GameManager gameManager;

    void Start(){
        gameManager = GameObject.FindObjectOfType<GameManager>();
        beatController = GameObject.FindObjectOfType<BeatController>();
        nextLevelScreenManager = GameObject.FindObjectOfType<NextLevelScreenManager>();
    }

    public void Activate(){
        //  Pause game
        Time.timeScale = 0f;
        beatController.Pause();

        //  Activate the Next Level UI Screen
        nextLevelScreenManager.Activate(GetComponent<ExitPortalManager>());
    }

    public void Cancel(){
        beatController.Resume();
        Time.timeScale = 1f;
    }

    public void NextLevel(){
        gameManager.CreateNextLevel();
    }
}
