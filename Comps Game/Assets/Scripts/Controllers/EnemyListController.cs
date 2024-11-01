using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyListController : MonoBehaviour {

    [SerializeField] private EnemyListWrapper enemyListWrapper; //serializing for debug
    [SerializeField] public List<SpriteData> spriteDataList;

    public static EnemyListController Instance = null; // for persistent

    public void Awake() {
        CheckSingleton();
        enemyListWrapper = new EnemyListWrapper();
        FindJSON();
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

    public void FindJSON() { // meant to be used for initialization
        string filepath = Application.persistentDataPath + $"/ALL_ENEMIES.json";
        if (System.IO.File.Exists(filepath)) {
            string json = System.IO.File.ReadAllText(filepath);
            enemyListWrapper = JsonUtility.FromJson<EnemyListWrapper>(json);
            Debug.Log("Enemies File exists!");
        } else {
            Debug.Log("Enemies file doesn't exist. Pulling from asset files.");
            TextAsset json_textAsset = Resources.Load<TextAsset>("WarriorListTemplates/ALL_ENEMIES_DEFAULT.json");
            enemyListWrapper = JsonUtility.FromJson<EnemyListWrapper>(json_textAsset.text);
        }
    }

    public void UpdateJSON() {
        string filePath = Application.persistentDataPath + $"/ALL_ENEMIES.json";
        string warriorsJSON = JsonUtility.ToJson(enemyListWrapper);
        System.IO.File.WriteAllText(filePath, warriorsJSON);
        Debug.Log("saving json at " + filePath);
    }

    public void AddWarrior(int index, WarriorFunctionalityData warriorData) {
        if (index >= enemyListWrapper.enemyList.Count) {
            enemyListWrapper.enemyList.Add(warriorData);
        } else { // editing an existing warrior
            enemyListWrapper.enemyList[index] = warriorData;
        }
        UpdateJSON();
    }

    public void RemoveWarrior(int index) {
        Debug.Log("count is " + enemyListWrapper.enemyList.Count + ", removing " + enemyListWrapper.enemyList[index].warriorName);
        enemyListWrapper.enemyList.RemoveAt(index);
        for (int i = 0; i < enemyListWrapper.enemyList.Count; i++) {
            enemyListWrapper.enemyList[i].warriorIndex = i;
        }
        Debug.Log("deleted! remaining warriors are:");
        for (int i = 0; i < enemyListWrapper.enemyList.Count; i++) {
            Debug.Log(enemyListWrapper.enemyList[i].warriorName);
        }
    }

    public int GetCount() {
        return enemyListWrapper.enemyList.Count;
    }

    public WarriorFunctionalityData GetWarriorAtIndex(int index) {
        if (index < enemyListWrapper.enemyList.Count && index >= 0) {
            return enemyListWrapper.enemyList[index];
        } else {
            return null;
        }
    }
}

// wrapper class for serializing json
[System.Serializable]
class EnemyListWrapper {
    public List<WarriorFunctionalityData> enemyList;

    public EnemyListWrapper() {
        enemyList = new List<WarriorFunctionalityData>();
    }
}
