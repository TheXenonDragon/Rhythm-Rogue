using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationUnitController : MonoBehaviour
{
    NavMeshAgent agent;
    float waitTime = 0.05f;
    float tileSize = 2.0f;

    bool checkpointsAreCalculated = false;
    private Vector3[] checkpoints;
    private List<Vector2Int> tempCheckpoints;



    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        checkpoints = null;
    }

    public Vector2Int[] GetCheckpointsArray(){
        Vector2Int[] gridCheckpoints;
        tempCheckpoints = new List<Vector2Int>();
        
        float minDistanceBetweenCheckpoints = 0.75f;

        //  Convert from Vector3 to Vector2Int
        for(int i = 0; i < checkpoints.Length; i++){
            if(i > 0 && Vector3.Distance(checkpoints[i], checkpoints[i - 1]) > minDistanceBetweenCheckpoints){
                tempCheckpoints.Add(new Vector2Int(Mathf.RoundToInt(checkpoints[i].x / tileSize), Mathf.RoundToInt(checkpoints[i].z / tileSize)));
            }
        }

        //  Convert from list to array
        gridCheckpoints = new Vector2Int[tempCheckpoints.Count];
        for(int i = 0; i < tempCheckpoints.Count; i++){
            gridCheckpoints[i] = tempCheckpoints[i];
        }

        return gridCheckpoints;
    }

    public IEnumerator CalculatePositionsArray(Vector3 destination, Vector3 enemyCurrentPosition){
        checkpointsAreCalculated = false;
        transform.position = enemyCurrentPosition;

        
        if(agent != null && agent.isOnNavMesh){
            agent.destination = destination;
            agent.isStopped = false;

            while(agent.path.corners.Length < 2){
                //wait
                yield return new WaitForSeconds(waitTime);
            }

            checkpoints = agent.path.corners;
            agent.isStopped = true;
            checkpointsAreCalculated = true;
        }
    }

    public void DestroyNavUnit(){
        StopCoroutine("CalculatePositionsArray");
        Destroy(gameObject);
    }

    public bool PositionsArrayHasBeenCalculated(){
        return checkpointsAreCalculated;
    }

    public bool PathHasCompleted(){
        return agent.hasPath;
    }
}
