using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCleanerManager : MonoBehaviour
{
    private List<GameObject> obstacles;
    private FloorGenerator floorGenerator;

    //  Ray lengths
    private float rayStartHeight = 50f;
    private float rayLength = 100f;

    //  Layers
    private string wallLayer = "Wall";
    private string obstacleLayer = "Obstacle";
    private string exitPortalLayer = "ExitPortal";

    private float tileSize;

    private Vector3 exitPortalLocation;

    private bool executeOnce = true;



    public void StartExecution(FloorGenerator floorGenerator, List<GameObject> obstacles, float tileSize, Vector3 exitPortalLocation)
    {
        this.floorGenerator = floorGenerator;
        this.obstacles = obstacles;
        this.tileSize = tileSize;
        this.exitPortalLocation = exitPortalLocation;
    }

    // Update is called once per frame
    void Update()
    {
        if(executeOnce){
            DestroyAllObstaclesInWalls();
            DestroyObstaclesInTheWayOfExitPortal();
            CleanPaths();
            executeOnce = false;
        }
    }

    //  Primary Method
    private void CleanPaths(){
        for(int i = obstacles.Count - 1; i >= 0; i--){
            if(CheckForWallAdjacent(obstacles[i].transform.position)){
                Destroy(obstacles[i]);
                obstacles.RemoveAt(i);
            }
        }

        for(int i = obstacles.Count - 1; i >= 0; i--){
            if(CheckCornersOfPaths(obstacles[i].transform.position)){
                Destroy(obstacles[i]);
                obstacles.RemoveAt(i);
            }
        }

        Invoke("RecalculateNavMesh", 0.5f);
        //floorGenerator.RecalculateNavMesh();

        //  This object is no longer needed, so destroy.
        Destroy(transform.gameObject, 1f);

    }

    private bool CheckCornersOfPaths(Vector3 currentPosition){
        Vector3[] directions = {
            new Vector3(1, 0, 1).normalized,
            new Vector3(-1, 0, 1).normalized,
            new Vector3(1, 0, -1).normalized,
            new Vector3(-1, 0, -1).normalized,
        };

        RaycastHit hit;
        for(int i = 0; i < directions.Length; i++){
            //  1: First diagonal wall
            if(Physics.Raycast(currentPosition + (directions[i] * tileSize) + Vector3.up * rayStartHeight, Vector3.down, out hit, rayLength)){
                if(hit.transform.gameObject.layer == LayerMask.NameToLayer(wallLayer)){
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckForWallAdjacent(Vector3 currentPosition){
        Vector3[] directions = {Vector3.forward, Vector3.back, Vector3.right, Vector3.left};

        RaycastHit hit;
        for(int i = 0; i < directions.Length; i++){
            if (Physics.Raycast(currentPosition, directions[i], out hit, tileSize)){
                if(hit.transform.gameObject.layer == LayerMask.NameToLayer(wallLayer)){
                    return true;
                }
            }
        }

        return false;
    }

    private void DestroyAllObstaclesInWalls(){
        RaycastHit hit;
        for(int i = obstacles.Count - 1; i >= 0; i--){
            if(obstacles[i] != null){
                if(Physics.Raycast(obstacles[i].transform.position + (Vector3.up * rayStartHeight), Vector3.down, out hit, rayLength)){
                    if(hit.transform.gameObject.layer == LayerMask.NameToLayer(wallLayer)){
                        Destroy(obstacles[i]);
                        obstacles.RemoveAt(i);
                    }
                }
            }
            else{
                //  It does not exist anyways, so delete it
                obstacles.RemoveAt(i);
            }
        }
    }

    private void DestroyObstaclesInTheWayOfExitPortal(){
        Vector3 exitPortalOffsetLocation = exitPortalLocation - (new Vector3(2f, 0f, 2f) * tileSize);
        int portalSize = 6;
        
        RaycastHit hit;

        for(int x = 0; x < portalSize; x++){
            for(int z = 0; z < portalSize; z++){
                if(Physics.Raycast(exitPortalOffsetLocation + (new Vector3(x, 0f, z) * tileSize) + (Vector3.up * rayStartHeight), Vector3.down, out hit, rayLength, ~LayerMask.NameToLayer(exitPortalLayer))){
                    if(hit.transform.gameObject.layer == LayerMask.NameToLayer(obstacleLayer)){
                        Destroy(hit.transform.gameObject);
                    }
                }
            }
        }

        for(int i = obstacles.Count - 1; i >= 0; i--){
            if(obstacles[i] == null){
                obstacles.RemoveAt(i);
            }
        }
    }

    private void RecalculateNavMesh(){
        floorGenerator.RecalculateNavMesh();
    }
}
