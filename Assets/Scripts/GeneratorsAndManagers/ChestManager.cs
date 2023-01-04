using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestManager : MonoBehaviour
{
    private int addWeaponLevel = 5;
    public int minXP = 5;
    public int maxXP = 15;

    private int currentXP;

    void Start()
    {
        currentXP = Random.Range(minXP, maxXP + 1);
    }

    public void OpenChest(GameObject objectOpeningChest){
        WeaponManager weaponManager = objectOpeningChest.GetComponent<WeaponManager>();
        XPManager xPManager = objectOpeningChest.GetComponent<XPManager>();

        if(weaponManager != null){
            weaponManager.AddWeaponLevel(addWeaponLevel);
        }

        if(xPManager != null){
            xPManager.AddXP(currentXP);
        }

        Destroy(gameObject);
    }
}
