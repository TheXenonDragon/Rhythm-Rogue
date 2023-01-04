using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonController : EnemyController
{
    //  Attacking
    public float viewRange = 10.0f;
    public float attackRange;

    //  Drops
    public float chanceOfWeaponDrop = 0.1f;
    private int weaponLevelDrop = 1;
    private int minXPDrop = 1;
    private int maxXPDrop = 5;
    private int currentXPDrop;




    // Start is called before the first frame update
    void Start()
    {
        attackRange = tileSize * 1.5f;
        currentXPDrop = Random.Range(minXPDrop, maxXPDrop + 1);
    }

    public void StartEnemy(Vector3 spawnPostion, BeatController beatController, GameObject player, float tileSize, float moveSpeed, int health, int damage, float viewRange, float attackRange){
        base.StartEnemy(spawnPostion, beatController, player, tileSize, moveSpeed, health, damage);
        this.viewRange = viewRange;
        this.attackRange = attackRange;
    }



    protected override void ChangeEnemyState(){
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection((player.transform.position - transform.position).normalized), out hit, viewRange)){
            if(hit.transform.gameObject == player.gameObject){
                if(Vector3.Distance(player.transform.position, transform.position) < attackRange){
                    //  Attacking: Player is within range
                    base.currentState = EnemyController.EnemyState.Attacking;
                }
                else{
                    //  Chasing: Check for player
                    base.checkpointsNeedToBeDetected = true;
                    base.currentState = EnemyController.EnemyState.Chasing;
                }
            }
        }
        else{
            //  Wandering:
            base.currentState = EnemyController.EnemyState.Wandering;
            if(base.currentState != EnemyController.EnemyState.Wandering){
                base.checkpointsNeedToBeDetected = true;      
            }
        }
    }

    protected override void Die()
    {
        //  Add weapon levels if applicable
        if(Random.Range(0f, 1f) < chanceOfWeaponDrop){
            player.GetComponent<WeaponManager>().AddWeaponLevel(weaponLevelDrop);
        }

        //  Add XP
        player.GetComponent<XPManager>().AddXP(currentXPDrop);

        base.Die();
    }
}
