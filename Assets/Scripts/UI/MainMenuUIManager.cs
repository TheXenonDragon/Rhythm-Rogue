using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuUIManager : MonoBehaviour
{
    public Image mainRegionImage;

    public GameObject loadedGameSection;
    public GameObject newGameSection;

    //  Load game
    public TextMeshProUGUI saveGameOne;
    public TextMeshProUGUI saveGameTwo;
    public TextMeshProUGUI saveGameThree;

    //  New game
    public TextMeshProUGUI inputField;
    public GameObject outOfSpacePopUp;

    private List<string> saveFileNames;
    private string defaultSaveGameText = "No Data";

    // Start is called before the first frame update
    void Start()
    {
        saveFileNames = new List<string>();
        LoadInSaveData();
        UpdateSavedGames();
        mainRegionImage.rectTransform.offsetMax = new Vector2((-Screen.width / 2f) - (saveGameOne.rectTransform.sizeDelta.x / 2f), mainRegionImage.rectTransform.offsetMax.y);
    }

    //  General
    public void OnNewGameClick(){
        loadedGameSection.SetActive(false);
        newGameSection.SetActive(true);
    }

    public void OnLoadGameClick(){
        newGameSection.SetActive(false);
        loadedGameSection.SetActive(true);
    }

    private void UpdateSavedGames(){
        if(saveFileNames.Count > 0){
            saveGameOne.text = saveFileNames[0];
        }
        else{
            saveGameOne.text = defaultSaveGameText;
            saveGameTwo.text = defaultSaveGameText;
            saveGameThree.text = defaultSaveGameText;
            return;
        }

        if(saveFileNames.Count > 1){
            saveGameTwo.text = saveFileNames[1];
        }
        else{
            saveGameTwo.text = defaultSaveGameText;
            saveGameThree.text = defaultSaveGameText;
            return;
        }

        if(saveFileNames.Count > 2){
            saveGameThree.text = saveFileNames[2];
        }
        else{
            saveGameThree.text = defaultSaveGameText;
            return;
        }
    }



    //  Memory
    private void LoadInSaveData(){
        string saveScoreFilePath = Application.persistentDataPath + "/saveFiles.json";

        if(File.Exists(saveScoreFilePath)){
            SaveFileModel[] saveFiles = JsonUtility.FromJson<SaveFilesModel>(File.ReadAllText(saveScoreFilePath)).saveFiles;

            foreach(SaveFileModel saveFile in saveFiles){
                saveFileNames.Add(saveFile.username);
            }
        }
    }

    private void RemoveSaveFile(int index){
        string usernameToDelete = saveFileNames[index];
        SaveFileModel[] newSaveFiles;
        List<SaveFileModel> tempSaveFiles = new List<SaveFileModel>();

        string saveScoreFilePath = Application.persistentDataPath + "/saveFiles.json";

        if(File.Exists(saveScoreFilePath)){
            SaveFilesModel oldSaveFiles = JsonUtility.FromJson<SaveFilesModel>(File.ReadAllText(saveScoreFilePath));

            //  Remove from saveFileNames.
            saveFileNames.RemoveAt(index);

            for(int i = 0; i < oldSaveFiles.saveFiles.Length; i++){
                if(oldSaveFiles.saveFiles[i].username != usernameToDelete){
                    tempSaveFiles.Add(oldSaveFiles.saveFiles[i]);
                }
            }

            //  Copy over from list to array
            newSaveFiles = new SaveFileModel[tempSaveFiles.Count];
            for(int i = 0; i < tempSaveFiles.Count; i++){
                newSaveFiles[i] = tempSaveFiles[i];
            }

            //  Set the old array to the new array
            oldSaveFiles.saveFiles = newSaveFiles;

            //  Write to memory
            File.WriteAllText(saveScoreFilePath, JsonUtility.ToJson(oldSaveFiles));
        }
    }

    private void AddSaveFile(string newUsername){
        SaveFileModel[] newSaveFiles;
        string saveScoreFilePath = Application.persistentDataPath + "/saveFiles.json";

        //  Add to list for sake of counting
        saveFileNames.Add(newUsername);

        if(File.Exists(saveScoreFilePath)){
            SaveFilesModel oldSaveFiles = JsonUtility.FromJson<SaveFilesModel>(File.ReadAllText(saveScoreFilePath));

            //  Copy over from old to new
            newSaveFiles = new SaveFileModel[oldSaveFiles.saveFiles.Length + 1];
            for(int i = 0; i < oldSaveFiles.saveFiles.Length; i++){
                newSaveFiles[i] = oldSaveFiles.saveFiles[i];
            }
            
            //  Add the new save name
            newSaveFiles[newSaveFiles.Length - 1] = new SaveFileModel();
            newSaveFiles[newSaveFiles.Length - 1].username = newUsername;

            //  Set the old array to the new array
            oldSaveFiles.saveFiles = newSaveFiles;

            //  Write to memory
            File.WriteAllText(saveScoreFilePath, JsonUtility.ToJson(oldSaveFiles));
        }
        else{
            SaveFilesModel saveFilesModel = new SaveFilesModel();
            saveFilesModel.saveFiles = new SaveFileModel[1];
            saveFilesModel.saveFiles[0] = new SaveFileModel();
            saveFilesModel.saveFiles[0].username = newUsername;

            //  Write to memory
            File.WriteAllText(saveScoreFilePath, JsonUtility.ToJson(saveFilesModel));
        }
    }

    private void OverwriteSaveFile(string newUsername){
        string saveScoreFilePath = Application.persistentDataPath + "/saveFiles.json";

        if(File.Exists(saveScoreFilePath)){
            SaveFilesModel oldSaveFiles = JsonUtility.FromJson<SaveFilesModel>(File.ReadAllText(saveScoreFilePath));
            
            //  Update old array
            oldSaveFiles.saveFiles[0] = new SaveFileModel();
            oldSaveFiles.saveFiles[0].username = newUsername;

            //  Write to memory
            File.WriteAllText(saveScoreFilePath, JsonUtility.ToJson(oldSaveFiles));
        }
    }
    
    private void SetCurrentSavePlayer(int index){
        string saveScoreFilePath = Application.persistentDataPath + "/saveFiles.json";

        if(File.Exists(saveScoreFilePath)){
            SaveFilesModel oldSaveFiles = JsonUtility.FromJson<SaveFilesModel>(File.ReadAllText(saveScoreFilePath));

            //  Set the current save player based on index
            oldSaveFiles.currentPlayer = oldSaveFiles.saveFiles[index];

            //  Write to memory
            File.WriteAllText(saveScoreFilePath, JsonUtility.ToJson(oldSaveFiles));
        } 
    }

    private void DetermineAddSaveOrOverwriteSave(string username){
        int index = -1;

        for(int i = 0; i < saveFileNames.Count; i++){
            if(saveFileNames[i] == username){
                index = i;
                break;
            }
        }

        if(index < 0){
            AddSaveFile(username);
        }
        else{
            OverwriteSaveFile(username);
        }
    }



    //  Save Game
    public void OnStartNewGameClick(){
        bool duplicateName = false;

        foreach(string fileName in saveFileNames){
            if(fileName == inputField.text){
                duplicateName = true;
            }
        }

        if(saveFileNames.Count < 3){
            DetermineAddSaveOrOverwriteSave(inputField.text);
            SetCurrentSavePlayer(saveFileNames.Count - 1);
            SceneManager.LoadScene(1);
        }
        else if(duplicateName){
            SetCurrentSavePlayer(saveFileNames.IndexOf(inputField.text));
            SceneManager.LoadScene(1);
        }
        else{
            outOfSpacePopUp.SetActive(true);
        }
    }

    public void OnOverwriteSaveOneClick(){
        saveFileNames[0] = inputField.text;
        OverwriteSaveFile(inputField.text);
        SetCurrentSavePlayer(0);
        SceneManager.LoadScene(1);
    }

    public void OnDenyOverwriteSaveOneClick(){
        outOfSpacePopUp.SetActive(false);
    }



    //  Load Game

    public void OnDeleteSaveOneClick(){
        if(saveFileNames.Count >= 1){
            RemoveSaveFile(0);
        }
        UpdateSavedGames();
    }

    public void OnDeleteSaveTwoClick(){
        if(saveFileNames.Count >= 2){
            RemoveSaveFile(1);
        }
        UpdateSavedGames();
    }

    public void OnDeleteSaveThreeClick(){
        if(saveFileNames.Count == 3){
            RemoveSaveFile(2);
        }
        UpdateSavedGames();
    }

    public void OnLoadSaveOneClick(){
        if(saveFileNames.Count > 0){
            SetCurrentSavePlayer(0);
            SceneManager.LoadScene(1);
        }
    }

    public void OnLoadSaveTwoClick(){
        if(saveFileNames.Count > 1){
            SetCurrentSavePlayer(1);
            SceneManager.LoadScene(1);
        }
    }

    public void OnLoadSaveThreeClick(){
        if(saveFileNames.Count > 2){
            SetCurrentSavePlayer(2);
            SceneManager.LoadScene(1);
        }
    }
}
