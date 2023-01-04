using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPManager : MonoBehaviour
{
    private int XP_Count = 0;


    public void AddXP(int xp){
        XP_Count += xp;
    }

    public int GetXP(){
        return XP_Count;
    }

    public bool RemoveXP(int xp){
        if(xp > XP_Count){
            return false;
        }
        else{
            XP_Count -= xp;
            return true;
        }
    }
}
