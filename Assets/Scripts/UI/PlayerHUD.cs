using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    //  Scripts
    public HealthManager healthManager;

    //  Notes
    public GameObject notePrefab;
    private RectTransform[] notes;
    private Image[] noteImages;

    //  Hud Elements
    public RectTransform hudContainer;
    public RectTransform beatBar;
    public RectTransform heartContainer;
    public RectTransform[] hearts;
    public RectTransform energyContainer;
    public RectTransform[] energies;

    //  Cached Values
    int screenPixelWidth = 0;
    int screenPixelHeight = 0;
    int healthCount = 0;
    private float noteWidth = 3f;
    
    //  Color
    private Color energyBlue = new Color(9f / 255f, 246f / 255f, 219f / 255f);
    private Color heartRed = new Color(246f / 255f, 9f / 255f, 36f / 255f);
    

    // Start is called before the first frame update
    void Start()
    {
        healthCount = healthManager.HealthCount();
        UpdateHudSizeAndPosition();
        UpdateHealth();
        CreateNotes();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHudSizeAndPosition();
        UpdateHealth();
        UpdateNotes();   
    }

    private void UpdateHudSizeAndPosition(){
        if(screenPixelWidth != Screen.width || screenPixelHeight != Screen.height){
            int xOffset = 10;
            int yOffset = 5;
            float heartSize;
            float energySize;

            //  Hud Container
            screenPixelWidth = Screen.width;
            screenPixelHeight = Screen.height;
            hudContainer.sizeDelta = new Vector2(screenPixelWidth + xOffset, screenPixelHeight / 8);
            hudContainer.anchoredPosition = new Vector2(0, - (screenPixelHeight / 2) + (hudContainer.sizeDelta.y / 2) - yOffset);

            //  Beat Bar
            beatBar.sizeDelta = new Vector2(hudContainer.sizeDelta.x / 2, hudContainer.sizeDelta.y / 2);

            //  Heart Container
            heartContainer.sizeDelta = new Vector2(beatBar.sizeDelta.x / 2, hudContainer.sizeDelta.y);
            heartContainer.anchoredPosition = new Vector2(-(hudContainer.sizeDelta.x - beatBar.sizeDelta.x) * 0.75f, 0);

            //  Hearts
            if((heartContainer.sizeDelta.y - yOffset) < ((heartContainer.sizeDelta.x - heartContainer.sizeDelta.y)) / 5){
                heartSize = hudContainer.sizeDelta.y / 2;
            }
            else{
                heartSize = ((heartContainer.sizeDelta.x - heartContainer.sizeDelta.y)) / 5;
            }

            for(int i = 0; i < hearts.Length; i++){
                hearts[i].sizeDelta = new Vector2(heartSize, heartSize);
                hearts[i].anchoredPosition = new Vector2((((heartContainer.sizeDelta.x - heartSize)) / 5) * (i - 2), 0);
            }

            //  Energy Container
            energyContainer.sizeDelta = new Vector2(beatBar.sizeDelta.x / 2, hudContainer.sizeDelta.y);
            energyContainer.anchoredPosition = new Vector2((hudContainer.sizeDelta.x - beatBar.sizeDelta.x) * 0.75f, 0);

            //  Energy
            if((energyContainer.sizeDelta.y - yOffset) < ((energyContainer.sizeDelta.x - energyContainer.sizeDelta.y)) / 5){
                energySize = hudContainer.sizeDelta.y / 2;
            }
            else{
                energySize = ((energyContainer.sizeDelta.x - energyContainer.sizeDelta.y)) / 5;
            }

            for(int i = 0; i < energies.Length; i++){
                energies[i].sizeDelta = new Vector2(energySize, energySize);
                energies[i].anchoredPosition = new Vector2((((energyContainer.sizeDelta.x - energySize)) / 5) * (-i + 2), 0);
            }
        }
    }

    private void UpdateHealth(){
        if(healthCount != healthManager.HealthCount()){
            healthCount = healthManager.HealthCount();

            if(healthCount < 0){
                healthCount = 0;
            }

            for(int i = 0; i < healthCount; i++){
                hearts[i].gameObject.SetActive(true);
            }

            for(int i = healthCount; i < hearts.Length; i++){
                hearts[i].gameObject.SetActive(false);
            }
        }
    }

    private void CreateNotes(){
        notes = new RectTransform[30];
        noteImages = new Image[notes.Length];
        for(int i = 0; i < notes.Length; i++){
            notes[i] = Instantiate<GameObject>(notePrefab, Vector3.zero, notePrefab.transform.rotation, beatBar.gameObject.transform).GetComponent<RectTransform>();
            notes[i].sizeDelta = new Vector2(noteWidth, beatBar.sizeDelta.y * 0.8f);
            notes[i].anchoredPosition = new Vector2(i * noteWidth * 5, 0);
            noteImages[i] = notes[i].gameObject.GetComponent<Image>();
            noteImages[i].color = energyBlue;
        }
    }

    private void UpdateNotes(){
        float speed = -100f;
        for(int i = 0; i < notes.Length; i++){
            notes[i].sizeDelta = new Vector2(noteWidth, beatBar.sizeDelta.y * 0.8f);
            notes[i].anchoredPosition = new Vector2(notes[i].anchoredPosition.x + (speed * Time.deltaTime), 0);

            if(notes[i].anchoredPosition.x < -beatBar.sizeDelta.x / 2){
                notes[i].anchoredPosition = new Vector2((notes[i].sizeDelta.x / 2) + (beatBar.sizeDelta.x / 2), 0);
                noteImages[i].color = energyBlue;
            }

            if(notes[i].anchoredPosition.x < 0){
                noteImages[i].color = heartRed;
            }
        }
    }
}
