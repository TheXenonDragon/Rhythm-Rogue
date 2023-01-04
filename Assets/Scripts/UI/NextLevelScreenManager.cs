using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevelScreenManager : MonoBehaviour
{
    private ExitPortalManager exitPortalManager;

    public GameObject fadedScreen;

    public void Activate(ExitPortalManager exitPortalManager){
        fadedScreen.SetActive(true);
        this.exitPortalManager = exitPortalManager;
    }

    public void YesButton(){
        fadedScreen.SetActive(false);
        exitPortalManager.NextLevel();
    }

    public void NoButton(){
        fadedScreen.SetActive(false);
        exitPortalManager.Cancel();
    }
}
