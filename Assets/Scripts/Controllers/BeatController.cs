using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatController : MonoBehaviour
{
    //  Old
    public event EventHandler BeatExecuted;
    private bool isPaused = false;

    //  New
    //  Song Beats Per Minute
    public float songBpm;

    //  How long each beat lasts in seconds
    public float secPerBeat;

    //  The Current song position, in seconds
    public float songPosition;

    //  The Current song position, in beats
    public float songPositionInBeats;

    //  How many seconds have passed since the song started
    public float dspSongTime;
    
    //The offset to the first beat of the song in seconds
    public float firstBeatOffset;

    //  Current Song
    private AudioSource audioSource;

    //  List of available songs.
    public AudioClip[] songs;
    
    public int songIndex;

    private int beatIndex = 0;

    private float[] beats;



    // Start is called before the first frame update
    void Start()
    {
        beats = new float[2000];
        for(int i = 0; i < beats.Length; i++){
            beats[i] = (float)i * 2;
        }

        audioSource = GetComponent<AudioSource>();

        //  Set song for audio source.
        audioSource.clip = songs[songIndex];

        //Calculate the number of seconds in each beat
        secPerBeat = 60f / songBpm;

        //Record the time when the music starts
        dspSongTime = (float)AudioSettings.dspTime;

        //Start the music
        audioSource.Play();
    }

    void Update()
    {
        //determine how many seconds since the song started
        songPosition = (float)(AudioSettings.dspTime - dspSongTime - firstBeatOffset);

        //determine how many beats since the song started
        songPositionInBeats = songPosition / secPerBeat;

        if (beatIndex < beats.Length && beats[beatIndex] < songPositionInBeats + 0)
        {
            print($"Note: {beatIndex}");
            BeatExecuted.Invoke(this, null);

            beatIndex++;
        }
    }

    /*
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
    */

    public void Pause(){
        isPaused = true;
    }

    public void Resume(){
        isPaused = false;
    }
}
