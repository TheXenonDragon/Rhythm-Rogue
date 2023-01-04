using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    //  Particle System
    public ParticleSystem particleSystemPrefab;
    private float particleSystemStopDelay = 1f;

    //  Weapon
    public int weaponDamage;
    public int weaponLevel = 0;
    private int levelUpAtLevel = 10;

    

    public void AddWeaponLevel(int level){
        weaponLevel += level;

        if(weaponLevel > levelUpAtLevel){
            weaponDamage += 1;
            weaponLevel -= levelUpAtLevel;
        }
    }

    public int LevelUpAtLevel(){
        return levelUpAtLevel;
    }

    public int CurrentWeaponLevel(){
        return weaponLevel;
    }

    public int CurrentWeaponDamage(){
        return weaponDamage;
    }

    public void SetInitialWeaponDamage(int newDamage){
        weaponDamage = newDamage;
    }


    public void DealDamage(GameObject objectToAttack, GameObject attacker){
        StartCoroutine(PlayParticleEffect(objectToAttack));

        HealthManager health = objectToAttack.GetComponent<HealthManager>();
        if(health != null){
            health.Damage(weaponDamage);
        }
    }

    private IEnumerator PlayParticleEffect(GameObject objectToAttack){
        ParticleSystem particleSystem = Instantiate<ParticleSystem>(particleSystemPrefab, objectToAttack.transform.position, particleSystemPrefab.transform.rotation);

        particleSystem.Play();
        yield return new WaitForSeconds(particleSystemStopDelay);
        particleSystemPrefab.Stop();
    }
}
