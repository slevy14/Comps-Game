using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorListController : MonoBehaviour {

    // placed on persistent warrior list object

    [SerializeField] private WarriorListWrapper warriorListWrapper; //serializing for debug
    [SerializeField] public List<SpriteData> spriteDataList;

    public static WarriorListController Instance = null; // for persistent

    public void Awake() {
        CheckSingleton();
        warriorListWrapper = new WarriorListWrapper();
    }

    public void CheckSingleton() {
        // DontDestroyOnLoad(this.gameObject);
        // GameObject found_object = GameObject.Find("WarriorListPersistent");
        // if (found_object != this.gameObject) {
        //     Destroy(this.gameObject);
        // }
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }
        // Make this object stay around when switching scenes
        DontDestroyOnLoad(this.gameObject);
    }

    void Start() {
        Debug.Log("current level is " + ProgressionController.Instance.currentLevel);
        if (ProgressionController.Instance.currentLevel == 0) {
            FindJSON("sandbox_warriors");
        } else {
            FindJSON("level_warriors");
        }
        // Debug.Log("there are " + GetCount() + " warriors to add to drawer");
    }

    public void FindJSON(string warriorsFile) { // meant to be used for initialization
        // if saved warrior file exists, load it into object data
        string filepath = Application.persistentDataPath + $"/{warriorsFile}.json";
        if (System.IO.File.Exists(filepath)) {
            string json = System.IO.File.ReadAllText(filepath);
            warriorListWrapper = JsonUtility.FromJson<WarriorListWrapper>(json);
            Debug.Log("Warriors File exists!");
            return;
        } else { // otherwise, create new warrior list from default asset file
            Debug.Log("Warriors file doesn't exist. Pulling from asset files.");
            ResetWarriorsJSON(warriorsFile);
        }
    }

    public void ResetWarriorsJSON(string warriorsFile) {
        // completely reset warrior list file
        // used for new game
        Debug.Log("Resetting warriors json file to default");
        TextAsset json_textAsset = Resources.Load<TextAsset>("WarriorListTemplates/ALL_WARRIORS_DEFAULT");
        warriorListWrapper = JsonUtility.FromJson<WarriorListWrapper>(json_textAsset.text);
        UpdateJSON(warriorsFile);
    }

    public void UpdateJSON(string warriorsFile) {
        // save all warriors to json file
        string filePath = Application.persistentDataPath + $"/{warriorsFile}.json";
        string warriorsJSON = JsonUtility.ToJson(warriorListWrapper);
        System.IO.File.WriteAllText(filePath, warriorsJSON);
        Debug.Log("saving json at " + filePath);
    }

    public void AddWarrior(int index, WarriorFunctionalityData warriorData) {
        // add current warrior data to list
        if (index >= warriorListWrapper.warriorList.Count) {
            // editing a new warrior, create new entry
            warriorListWrapper.warriorList.Add(warriorData);
        } else { // editing an existing warrior
            warriorListWrapper.warriorList[index] = warriorData;
        }

        // save to sandbox or level json, depending on which level we are in
        if (ProgressionController.Instance.currentLevel == 0) {
            UpdateJSON("sandbox_warriors");
        } else {
            UpdateJSON("level_warriors");
        }
    }

    public void RemoveWarrior(int index) {
        // remove warrior from list
        warriorListWrapper.warriorList.RemoveAt(index);
        // update saved indices
        for (int i = 0; i < warriorListWrapper.warriorList.Count; i++) {
            warriorListWrapper.warriorList[i].warriorIndex = i;
        }
    }

    public int GetCount() {
        return warriorListWrapper.warriorList.Count;
    }

    public WarriorFunctionalityData GetWarriorAtIndex(int index) {
        if (index < warriorListWrapper.warriorList.Count && index >= 0) {
            return warriorListWrapper.warriorList[index];
        } else {
            return null;
        }
    }
}

// wrapper class for serializing json
[System.Serializable]
class WarriorListWrapper {
    public List<WarriorFunctionalityData> warriorList;

    public WarriorListWrapper() {
        warriorList = new List<WarriorFunctionalityData>();
    }
}
