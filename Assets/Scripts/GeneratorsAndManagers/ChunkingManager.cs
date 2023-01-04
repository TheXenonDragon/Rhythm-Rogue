using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkingManager : MonoBehaviour
{
    public GameObject player;

    private GameObject floorPlan = null;
    private GameObject[] chambers;

    private int floorPlanSize;
    private float maxActiveDistance = 75f;
    private Vector3 offSet;

    // Update is called once per frame
    void Update()
    {
        if(floorPlan == null){
            Invoke("AttemptToLoadData", 1.5f);
        }
        else{
            UpdateActiveChambers();
        }
    }
    
    private void AttemptToLoadData(){
        floorPlan = GameObject.Find("FloorPlan");
        
        if(floorPlan != null){
            //  4 is subtracted because the last 4 children are view boundaries
            chambers = new GameObject[floorPlan.transform.childCount - 4];
            for(int i = 0; i < chambers.Length; i++){
                chambers[i] = floorPlan.transform.GetChild(i).gameObject;
            }
        }

        floorPlanSize = Mathf.RoundToInt(Mathf.Sqrt(chambers.Length));

        if(chambers.Length > 1){
            offSet = new Vector3(1f, 0f, 1f) * Vector3.Distance(chambers[0].transform.position, chambers[1].transform.position) / 2f;
        }
        else{
            offSet = new Vector3(1f, 0f, 1f) * 100f / 2f;    //  100 is arbitrarily chosen as a "large enough" integer
        }
    }

    private void UpdateActiveChambers(){
        for(int i = 0; i < chambers.Length; i++){
            if(Vector3.Distance(chambers[i].transform.position + offSet, player.transform.position) > maxActiveDistance){
                chambers[i].SetActive(false);
            }
            else{
                chambers[i].SetActive(true);
            }
        }
    }
}
