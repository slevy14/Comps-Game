using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorListController : MonoBehaviour {

    [SerializeField] private WarriorListWrapper warriorListWrapper; //serializing for debug

    public void Awake() {
        CheckSingleton();
        warriorListWrapper = new WarriorListWrapper();
        FindJSON();
    }

    public void CheckSingleton() {
        DontDestroyOnLoad(this.gameObject);
        GameObject found_object = GameObject.Find("WarriorListPersistent");
        if (found_object != this.gameObject) {
            Destroy(this.gameObject);
        }
    }

    public void FindJSON() { // meant to be used for initialization
        string filepath = Application.persistentDataPath + $"/warriors.json";
        if (System.IO.File.Exists(filepath)) {
            string json = System.IO.File.ReadAllText(filepath);
            warriorListWrapper = JsonUtility.FromJson<WarriorListWrapper>(json);
            Debug.Log("Warriors File exists!");
            return;
        }
        Debug.Log("Warriors file doesn't exist. Creating a new file.");
        UpdateJSON();
    }

    public void UpdateJSON() {
        string filePath = Application.persistentDataPath + $"/warriors.json";
        string warriorsJSON = JsonUtility.ToJson(warriorListWrapper);
        System.IO.File.WriteAllText(filePath, warriorsJSON);
        Debug.Log("saving json at " + filePath);
    }

    public void AddWarrior(string name, WarriorFunctionalityData warriorData) {
        for (int i = 0; i < warriorListWrapper.warriorList.Count; i ++) {
            if (warriorListWrapper.warriorList[i].warriorName.Equals(name)) {
                warriorListWrapper.warriorList[i] = warriorData;
                UpdateJSON();
                return;
            }
        }
        warriorListWrapper.warriorList.Add(warriorData);
        UpdateJSON();
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
