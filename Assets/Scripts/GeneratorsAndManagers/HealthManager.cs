using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    private int healthCount;
    public int initialHealthCount = 0;

    void Start(){
        healthCount = initialHealthCount;
    }

    public void Damage(int damage){
        healthCount -= damage;
    }

    public int HealthCount(){
        return healthCount;
    }

    public void SetHealth(int health){
        initialHealthCount = health;
        healthCount = initialHealthCount;
    }

    public void RestoreHealth(){
        healthCount = initialHealthCount;
    }

    public bool isAlive(){
        if(healthCount > 0){
            return true;
        }
        else{
            return false;
        }
    }
}
