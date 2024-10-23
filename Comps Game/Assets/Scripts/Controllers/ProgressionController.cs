using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionController : MonoBehaviour {

    [Header("Level Data")]
    [SerializeField] public List<LevelDataSO> levelDataList;
    [SerializeField] public int continueLevelFrom; // max level, will be loaded upon clicking "continue" from main menu
    [SerializeField] public int currentLevel; // 0 is sandbox

    [Header("Save/Load")]
    [SerializeField] public PlayerProgressionAndSettings playerProgressionAndSettings;


    public static ProgressionController Instance = null; // for persistent

    public void Awake() {
        CheckSingleton();
        InitializeSavedData();
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
        continueLevelFrom = playerProgressionAndSettings.continueLevelFrom;
    }

    public bool FindJSON() { // meant to be used for initialization
        string filepath = Application.persistentDataPath + $"/player_progression_and_settings.json";
        if (System.IO.File.Exists(filepath)) {
            string json = System.IO.File.ReadAllText(filepath);
            playerProgressionAndSettings = JsonUtility.FromJson<PlayerProgressionAndSettings>(json);
            Debug.Log("Player Save Data file fxists!");
            return true;
        }
        Debug.Log("Player Save Data file doesn't exist. Creating a new file.");
        playerProgressionAndSettings.continueLevelFrom = 0;
        this.UpdateJSON();
        return false;
    }

    public void UpdateJSON() {
        string filePath = Application.persistentDataPath + $"/player_progression_and_settings.json";
        string saveDataJSON = JsonUtility.ToJson(playerProgressionAndSettings);
        System.IO.File.WriteAllText(filePath, saveDataJSON);
        Debug.Log("saving json at " + filePath);
    }

}

[System.Serializable]
public struct PlayerProgressionAndSettings {
    public int continueLevelFrom;

    public PlayerProgressionAndSettings(int continueLevelFrom) {
        this.continueLevelFrom = continueLevelFrom;
    }
}
