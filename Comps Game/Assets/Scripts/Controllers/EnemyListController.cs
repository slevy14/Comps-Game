using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyListController : MonoBehaviour {

    // placed on persistent enemy list object

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
        // if saved enemy file exists, load it into object data
        string filepath = Application.persistentDataPath + $"/ALL_ENEMIES.json";
        if (System.IO.File.Exists(filepath)) {
            string json = System.IO.File.ReadAllText(filepath);
            enemyListWrapper = JsonUtility.FromJson<EnemyListWrapper>(json);
            Debug.Log("Enemies File exists!");
        } else { // otherwise, create local file from asset enemy data
            Debug.Log("Enemies file doesn't exist. Pulling from asset files.");
            TextAsset json_textAsset = Resources.Load<TextAsset>("WarriorListTemplates/ALL_ENEMIES_DEFAULT");
            enemyListWrapper = JsonUtility.FromJson<EnemyListWrapper>(json_textAsset.text);
        }
    }

    public void UpdateJSON() {
        // save all enemies to json file
        string filePath = Application.persistentDataPath + $"/ALL_ENEMIES.json";
        string warriorsJSON = JsonUtility.ToJson(enemyListWrapper);
        System.IO.File.WriteAllText(filePath, warriorsJSON);
        Debug.Log("saving json at " + filePath);
    }

    public void AddWarrior(int index, WarriorFunctionalityData warriorData) {
        // add current warrior data to list
        if (index >= enemyListWrapper.enemyList.Count) {
            // editing a new warrior, create new entry
            enemyListWrapper.enemyList.Add(warriorData);
        } else { // editing an existing warrior
            enemyListWrapper.enemyList[index] = warriorData;
        }
        // save to json
        UpdateJSON();
    }

    public void RemoveWarrior(int index) {
        // remove enemy from list
        enemyListWrapper.enemyList.RemoveAt(index);
        // update saved indices
        for (int i = 0; i < enemyListWrapper.enemyList.Count; i++) {
            enemyListWrapper.enemyList[i].warriorIndex = i;
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
