using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatController : MonoBehaviour
{
    public event EventHandler BeatExecuted;

    public float primarySpacing = 1.4f;
    public float secondarySpacing = 1.4f;

    private float[] waitTimes;

    AudioSource audioSource;

    private static int index = 0;

    private bool isPaused = false;


    // Start is called before the first frame update
    void Start()
    {
        waitTimes = new float[] {primarySpacing, primarySpacing, secondarySpacing, secondarySpacing};
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(BeatTimer());
    }

    IEnumerator BeatTimer(){
        while(true){
            if(!isPaused){
                if(BeatExecuted != null){
                    BeatExecuted.Invoke(this, null);
                    Invoke("PlayAudio", 0.2f);
                }
                index++;

                if(index >= waitTimes.Length){
                    index = 0;
                }
            }

            yield return new WaitForSeconds(waitTimes[index]);
        }
    }

    private void PlayAudio(){
        audioSource.Play();
    }

    public void Pause(){
        isPaused = true;
    }

    public void Resume(){
        isPaused = false;
    }
}
