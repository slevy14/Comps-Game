using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorListController : MonoBehaviour {

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
        Debug.Log("there are " + GetCount() + " warriors to add to drawer");
    }

    public void FindJSON(string warriorsFile) { // meant to be used for initialization
        string filepath = Application.persistentDataPath + $"/{warriorsFile}.json";
        if (System.IO.File.Exists(filepath)) {
            string json = System.IO.File.ReadAllText(filepath);
            warriorListWrapper = JsonUtility.FromJson<WarriorListWrapper>(json);
            Debug.Log("Warriors File exists!");
            return;
        }
        Debug.Log("Warriors file doesn't exist. Creating a new file.");
        AddWarrior(0, new WarriorFunctionalityData());
    }

    public void UpdateJSON(string warriorsFile) {
        string filePath = Application.persistentDataPath + $"/{warriorsFile}.json";
        string warriorsJSON = JsonUtility.ToJson(warriorListWrapper);
        System.IO.File.WriteAllText(filePath, warriorsJSON);
        Debug.Log("saving json at " + filePath);
    }

    public void AddWarrior(int index, WarriorFunctionalityData warriorData) {
        if (index >= warriorListWrapper.warriorList.Count) {
            warriorListWrapper.warriorList.Add(warriorData);
        } else { // editing an existing warrior
            warriorListWrapper.warriorList[index] = warriorData;
        }

        if (DesignerController.Instance.isSandbox) {
            UpdateJSON("sandbox_warriors");
        } else {
            UpdateJSON("level_warriors");
        }
    }

    public void RemoveWarrior(int index) {
        Debug.Log("count is " + warriorListWrapper.warriorList.Count + ", removing " + warriorListWrapper.warriorList[index].warriorName);
        warriorListWrapper.warriorList.RemoveAt(index);
        for (int i = 0; i < warriorListWrapper.warriorList.Count; i++) {
            warriorListWrapper.warriorList[i].warriorIndex = i;
        }
        Debug.Log("deleted! remaining warriors are:");
        for (int i = 0; i < warriorListWrapper.warriorList.Count; i++) {
            Debug.Log(warriorListWrapper.warriorList[i].warriorName);
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
