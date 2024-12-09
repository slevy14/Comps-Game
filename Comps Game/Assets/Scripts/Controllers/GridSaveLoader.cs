using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSaveLoader : MonoBehaviour {

    private string filepath;
    public GameObject warriorPrefab;
    private bool isSandbox;

    // singleton
    public static GridSaveLoader Instance = null; // for persistent

    public void Awake() {
        CheckSingleton();
        // rest of initialization happens in start
    }

    public void CheckSingleton() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }
    }

    public void InitializeGrid() {
        isSandbox = ProgressionController.Instance.currentLevel == 0;
        // if in sandbox, load sandbox grid
        if (isSandbox) {
            filepath = Application.persistentDataPath + $"/sandbox_grid.json";
            // if indices in list don't match indices saved to grid, reset grid
            if (ProgressionController.Instance.madeListEdits) {
                ProgressionController.Instance.madeListEdits = false;
                ResetGrid();
                SaveGridToJSON();
            } else {
                LoadGridFromJson();
            }
        } else { // else, load level grid
            filepath = Application.persistentDataPath + $"/{ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].levelNumber}_grid.json";
            if (ProgressionController.Instance.isLevelJustStarted) {
                ProgressionController.Instance.SetLevelStarted();
                LoadGridFromLevelData();
                SaveGridToJSON();
            } else if (ProgressionController.Instance.madeListEdits) {
                ProgressionController.Instance.madeListEdits = false;
                LoadGridFromLevelData();
                SaveGridToJSON();
            } else {
                LoadGridFromJson();
            }
            // grey out thumbnails if need to
            if (ProgressionController.Instance.currentLevel != 0) { // not sandbox
                LevelController.Instance.SetAllWarriorThumbnailsGrey(PlacementSystem.Instance.GetPlacedWarriorCount() >= HelperController.Instance.GetCurrentLevelData().maxWarriorsToPlace);
            }
        }
    }

    public void SaveGridToJSON() {
        // save current grid to json file
        string warriorsJSON = JsonUtility.ToJson(new GridWithObjects(LevelController.Instance.objectsOnGrid));
        System.IO.File.WriteAllText(filepath, warriorsJSON);
        Debug.Log("saving json at " + filepath);
    }

    public void LoadGridFromJson() {
        // load json file
        if (!System.IO.File.Exists(filepath)) {
            // if doesn't exist, just reset grid
            // if data error and grid can't be loaded, just reset it
            ResetGrid();
            LoadGridFromJson();
        }
        string json = System.IO.File.ReadAllText(filepath);
        GridWithObjects gridWithObjects = JsonUtility.FromJson<GridWithObjects>(json);
        Debug.Log("grid found");
        // load objects from json to grid
        // foreach object in saved grid:
        try {
            foreach (WarriorOnGrid warriorOnGrid in gridWithObjects.warriorList) {
                // instantiate into right position
                GameObject warrior = Instantiate(warriorPrefab, PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)warriorOnGrid.pos.x, (int)warriorOnGrid.pos.y, 0)), this.transform.rotation, GameObject.Find("WarriorsContainer").transform);
                // set properties like in WarriorLevelThumbnail
                LevelController.Instance.SetWarriorData(warrior, warriorOnGrid.isEnemy, warriorOnGrid.warriorIndex);
                warrior.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = !warriorOnGrid.isEnemy ? WarriorListController.Instance.spriteDataList[WarriorListController.Instance.GetWarriorAtIndex(warriorOnGrid.warriorIndex).spriteIndex].animatorController : EnemyListController.Instance.spriteDataList[EnemyListController.Instance.GetWarriorAtIndex(warriorOnGrid.warriorIndex).spriteIndex].animatorController;
                // add object to grid object dict
                LevelController.Instance.objectsOnGrid[warrior] = warriorOnGrid.pos;
            }
        } catch {
            // if data error and grid can't be loaded, just reset it
            ResetGrid();
            LoadGridFromJson();
        }

    }

    public void LoadGridFromLevelData() {
        // units to preload are always enemies
        foreach (WarriorOnGrid enemy in ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].enemyPlacementsList) {
            // instantiate into right position
            GameObject warrior = Instantiate(warriorPrefab, PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)enemy.pos.x, (int)enemy.pos.y, 0)), this.transform.rotation, GameObject.Find("WarriorsContainer").transform);
            // set properties like in WarriorLevelThumbnail
            LevelController.Instance.SetWarriorData(warrior, enemy.isEnemy, enemy.warriorIndex);
            warrior.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = !enemy.isEnemy ? WarriorListController.Instance.spriteDataList[WarriorListController.Instance.GetWarriorAtIndex(enemy.warriorIndex).spriteIndex].animatorController : EnemyListController.Instance.spriteDataList[EnemyListController.Instance.GetWarriorAtIndex(enemy.warriorIndex).spriteIndex].animatorController;
            // add object to grid object dict
            LevelController.Instance.objectsOnGrid[warrior] = enemy.pos;
        }
    }

    public void ResetGrid() {
        // empty grid and save to json
        string warriorsJSON = JsonUtility.ToJson(new GridWithObjects());
        System.IO.File.WriteAllText(filepath, warriorsJSON);
        Debug.Log("saving json at " + filepath);
        SaveGridToJSON();
    }

}

// positional warrior data, serializable for saving
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
        // loop through all objects in dictionary, save them to list of warrior objects
        foreach (GameObject warriorObject in LevelController.Instance.objectsOnGrid.Keys) {
            WarriorBehavior warriorBehavior = warriorObject.GetComponent<WarriorBehavior>();
            warriorList.Add(new WarriorOnGrid(LevelController.Instance.objectsOnGrid[warriorObject], warriorBehavior.warriorIndex, warriorBehavior.isEnemy));
        }
    }

    public GridWithObjects() {
        warriorList = new List<WarriorOnGrid>();
    }
}
