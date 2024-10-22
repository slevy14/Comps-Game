using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSaveLoader : MonoBehaviour {

    private string filepath;
    public GameObject warriorPrefab;

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

    public void LoadGridFromJson() {
        // load json file
        if (System.IO.File.Exists(filepath)) Debug.Log("warriors level file exists");
        string json = System.IO.File.ReadAllText(filepath);
        GridWithObjects gridWithObjects = JsonUtility.FromJson<GridWithObjects>(json);
        Debug.Log("grid found");
        // load objects from json to grid
        // foreach object in saved grid:
        foreach (WarriorOnGrid warriorOnGrid in gridWithObjects.warriorList) {
            Debug.Log("instantiating warrior from saved grid");
            // instantiate into right position
            GameObject warrior = Instantiate(warriorPrefab, PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)warriorOnGrid.pos.x, (int)warriorOnGrid.pos.y, 0)), this.transform.rotation, GameObject.Find("WarriorsContainer").transform);
            // set properties like in WarriorLevelThumbnail
            LevelController.Instance.SetWarriorData(warrior, warriorOnGrid.isEnemy, warriorOnGrid.warriorIndex);
            warrior.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = !warriorOnGrid.isEnemy ? WarriorListController.Instance.spriteDataList[WarriorListController.Instance.GetWarriorAtIndex(warriorOnGrid.warriorIndex).spriteIndex].animatorController : EnemyListController.Instance.spriteDataList[EnemyListController.Instance.GetWarriorAtIndex(warriorOnGrid.warriorIndex).spriteIndex].animatorController;
            // add object to grid object dict
            LevelController.Instance.objectsOnGrid[warrior] = warriorOnGrid.pos;
        }

    }

}

[System.Serializable]
public struct WarriorOnGrid {
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

    public GridWithObjects() {
        warriorList = new List<WarriorOnGrid>();
    }
}
