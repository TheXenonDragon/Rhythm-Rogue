using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public HealthManager healthManager;
    public WeaponManager weaponManager;
    public XPManager xPManager;

    public Slider healthSlider;
    public Slider weaponSlider;

    public TextMeshProUGUI weaponDamageLevel;
    public TextMeshProUGUI xpText;


    // Start is called before the first frame update
    void Start()
    {
        healthSlider.maxValue = healthManager.HealthCount();
        weaponSlider.maxValue = weaponManager.LevelUpAtLevel();
    }

    // Update is called once per frame
    void Update()
    {
        healthSlider.value = healthManager.HealthCount();
        weaponSlider.value = weaponManager.CurrentWeaponLevel();
        weaponDamageLevel.text = $"{weaponManager.CurrentWeaponDamage()}";
        xpText.text = $"{xPManager.GetXP()}";
    }
}
