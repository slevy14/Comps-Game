using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProgressionController : MonoBehaviour {

    [Header("Level Data")]
    [SerializeField] public List<LevelDataSO> levelDataList;
    [SerializeField] public int continueLevelFrom; // max level, will be loaded upon clicking "continue" from main menu
    [SerializeField] public int currentLevel; // 0 is sandbox

    [Header("Save/Load")]
    [SerializeField] public bool isLevelJustStarted;
    [SerializeField] public bool madeListEdits; // true if deleted warrior, etc. need to reset grid

    [Header("Recent Warrior")]
    [SerializeField] public int lastEditedWarrior;


    public static ProgressionController Instance = null; // for persistent

    public void Awake() {
        CheckSingleton();
        InitializeSavedData();

        // FOR DEBUG
        if (SceneManager.GetActiveScene().name == "Sandbox" || SceneManager.GetActiveScene().name == "CodeEditor") {
            currentLevel = 0;
        } else {
            currentLevel = continueLevelFrom;
        }
    }

    public void CheckSingleton() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }
        // Make this object stay around when switching scenes
        DontDestroyOnLoad(this.gameObject);
    }

    public void InitializeSavedData() {
        FindJSON();
    }

    public bool FindJSON() { // meant to be used for initialization
        // find the save file
        string filepath = Application.persistentDataPath + $"/player_progression_and_settings.json";
        PlayerProgressionAndSettings playerProgressionAndSettings;
        // if the file exists, save data to it
        if (System.IO.File.Exists(filepath)) {
            string json = System.IO.File.ReadAllText(filepath);
            playerProgressionAndSettings = JsonUtility.FromJson<PlayerProgressionAndSettings>(json);
            continueLevelFrom = playerProgressionAndSettings.continueLevelFrom;
            Debug.Log("Player Save Data file exists!");
            return true;
        }
        // if the file does not exist, create a new file
        Debug.Log("Player Save Data file doesn't exist. Creating a new file.");
        playerProgressionAndSettings.continueLevelFrom = -1;
        continueLevelFrom = playerProgressionAndSettings.continueLevelFrom;
        this.SaveProgressToJson();
        return false;
    }

    public void SaveProgressToJson() {
        // save current progression settings
        string filePath = Application.persistentDataPath + $"/player_progression_and_settings.json";
        string saveDataJSON = JsonUtility.ToJson(new PlayerProgressionAndSettings(continueLevelFrom));
        System.IO.File.WriteAllText(filePath, saveDataJSON);
        Debug.Log("saving json at " + filePath);
    }

    public void StartNewLevel(int newLevel) {
        // save current level data
        continueLevelFrom = newLevel;
        currentLevel = newLevel;
        isLevelJustStarted = true;
        SaveProgressToJson();
    }

    // begin tutorial at the start of each level
    public void SetLevelStarted() {
        isLevelJustStarted = false;
        TutorialController.Instance.StartTutorial();
    }

    // set warrior to load when reloading editor
    public void SetLastEditedWarrior(int index) {
        this.lastEditedWarrior = index;
    }
}


// SETTINGS DATA

[System.Serializable]
public struct PlayerProgressionAndSettings {

    // separate struct to hold progression data
    // stored in json file
    // currently only holds level to continue from

    public int continueLevelFrom;

    public PlayerProgressionAndSettings(int continueLevelFrom) {
        this.continueLevelFrom = continueLevelFrom;
    }
}
