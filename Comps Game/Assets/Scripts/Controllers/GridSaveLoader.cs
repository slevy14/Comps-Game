using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSaveLoader : MonoBehaviour {

    private string filepath;

    // singleton
    public static GridSaveLoader Instance = null; // for persistent

    public void Awake() {
        CheckSingleton();
        filepath = Application.persistentDataPath + $"/start-level-grid.json";
    }

    public void CheckSingleton() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }
        // Make this object stay around when switching scenes
    }

    public void SaveGridToJSON() {
        string warriorsJSON = JsonUtility.ToJson(new GridWithObjects(LevelController.Instance.objectsOnGrid));
        System.IO.File.WriteAllText(filepath, warriorsJSON);
        Debug.Log("saving json at " + filepath);
    }

}

[System.Serializable]
struct WarriorOnGrid {
    public Vector2 pos;
    public int warriorIndex;
    public bool isEnemy;

    public WarriorOnGrid(Vector2 pos, int warriorIndex, bool isEnemy) {
        this.pos = pos;
        this.warriorIndex = warriorIndex;
        this.isEnemy = isEnemy;
    }
}

// wrapper class for serializing json
[System.Serializable]
class GridWithObjects {
    public List<WarriorOnGrid> warriorList;

    public GridWithObjects(Dictionary<GameObject, Vector2> gridObjectsDict) {
        warriorList = new List<WarriorOnGrid>();
        foreach (GameObject warriorObject in LevelController.Instance.objectsOnGrid.Keys) {
            WarriorBehavior warriorBehavior = warriorObject.GetComponent<WarriorBehavior>();
            warriorList.Add(new WarriorOnGrid(LevelController.Instance.objectsOnGrid[warriorObject], warriorBehavior.warriorIndex, warriorBehavior.isEnemy));
        }
    }
}
