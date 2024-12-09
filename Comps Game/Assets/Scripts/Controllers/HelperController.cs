using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// SOME HELPER FUNCTIONS USEFUL TO ALL!
// this could just be a static class
// but by making it a monobehaviour object, we can still use update for checks
public class HelperController : MonoBehaviour {

    public static HelperController Instance = null; 

    public void CheckSingleton() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void Awake() {
        CheckSingleton();
    }

    void Update() {
        // play click sound effect any time mouse is clicked
        if (Input.GetMouseButtonDown(0)) {
            AudioController.Instance.PlaySoundEffect("Click");
        }
    } 


    public List<RaycastResult> OverUI() {
        // get pointer position
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        // loop through all objects below the mouse
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        for (int i = 0; i < raycastResults.Count; i++) {
            // if an object is not on the UI layer, remove it
            if (raycastResults[i].gameObject.layer != 5) { // ui layer
                raycastResults.RemoveAt(i);
                i--;
            }
        }
        
        // debug: print all things that are under ui
        if (raycastResults.Count > 0) foreach (RaycastResult raycastResult in raycastResults) {
            Debug.Log("Hit " + raycastResult.gameObject.name);
        } else {
            Debug.Log("Not over UI! Count is " + raycastResults.Count);
        }

        // return all objects remaining in the list
        return raycastResults;
    }

    public int CalculateWarriorStrength(int attackPower, int attackRange, int healPower, int projectilePower, int speed, int maxHealth, int defense) {
        // perform strength calculation, reutn it rounded to an int
        float strength = attackPower*attackRange + healPower*attackRange + projectilePower + speed/10 + maxHealth*(1 + ((float)defense / (float)(defense+maxHealth)));
        return Mathf.RoundToInt(strength);
    }

    public bool ValidateBehaviorCount(int warriorIndex) {
        // count all behaviors that are not functional blocks
        int count = 0;
        List<List<BlockDataStruct>> behaviorLists = new List<List<BlockDataStruct>> {WarriorListController.Instance.GetWarriorAtIndex(warriorIndex).moveFunctions, WarriorListController.Instance.GetWarriorAtIndex(warriorIndex).useWeaponFunctions, WarriorListController.Instance.GetWarriorAtIndex(warriorIndex).useSpecialFunctions};
        List<int> behaviorIndices = new List<int> {1, 2, 3, 4, 5, 6, 13, 14, 15};
        foreach (List<BlockDataStruct> behaviorList in behaviorLists) {
            foreach (BlockDataStruct block in behaviorList) {
                if (behaviorIndices.Contains((int)block.behavior)) {
                    count += 1;
                }
            }
        }
        // return true if count is below max
        return count <= ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxBlocks;
    }

    public bool ValidateStrength(int warriorIndex) {
        return WarriorListController.Instance.GetWarriorAtIndex(warriorIndex).warriorStrength <= ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxTotalStrength;
    }

    public LevelDataSO GetCurrentLevelData() {
        return ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel];
    }

}
