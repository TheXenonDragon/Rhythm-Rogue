using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationUnitController : MonoBehaviour
{
    NavMeshAgent agent;
    float waitTime = 0.05f;

    bool checkpointsAreCalculated = false;
    private Vector3[] checkpoints; 



    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        checkpoints = null;
    }

    public Vector3[] GetPositionsArray(){
        return checkpoints;
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
