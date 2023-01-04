using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class GameOverUIManager : MonoBehaviour
{
    public TextMeshProUGUI xp;
    public TextMeshProUGUI floor;

    // Start is called before the first frame update
    void Start()
    {
        string saveScoreFilePath = Application.persistentDataPath + "/saveFiles.json";

        if(File.Exists(saveScoreFilePath)){
            SaveFileModel saveFile = JsonUtility.FromJson<SaveFilesModel>(File.ReadAllText(saveScoreFilePath)).currentPlayer;
            
            xp.text = $"{saveFile.currentXpScore}";
            floor.text = $"{saveFile.currentFloorScore}";
        }

        
    }

    public void OnPlayAgainClick(){
        SceneManager.LoadScene(1);
    }

    public void OnMainMenuClick(){
        SceneManager.LoadScene(0);
    }
}
