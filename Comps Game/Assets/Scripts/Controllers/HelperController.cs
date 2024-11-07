using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// SOME HELPER FUNCTIONS USEFUL TO ALL!
// this could (and probably should) just be a static class
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
        if (Input.GetMouseButtonDown(0)) {
            AudioController.Instance.PlaySoundEffect("Click");
        }
    } 


    public bool IsOverUI() {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        for (int i = 0; i < raycastResults.Count; i++) {
            // Debug.Log(raycastResults[i].gameObject.name);
            if (raycastResults[i].gameObject.layer != 5) { // ui layer
                raycastResults.RemoveAt(i);
                i--;
            }
        }
        return raycastResults.Count > 0;
    }

    public int CalculateWarriorStrength(int attackPower, int attackRange, int healPower, int projectilePower, int speed, int maxHealth, int defense) {
        float strength = attackPower*attackRange + healPower*attackRange + projectilePower + speed/10 + maxHealth*(1 + ((float)defense / (float)(defense+maxHealth)));
        return Mathf.RoundToInt(strength);
    }

    public bool ValidateBehaviorCount(int warriorIndex) {
        // validate behavior count
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
        return count <= ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxBlocks;
    }

    public bool ValidateStrength(int warriorIndex) {
        return WarriorListController.Instance.GetWarriorAtIndex(warriorIndex).warriorStrength <= ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxTotalStrength;
    }

    public LevelDataSO GetCurrentLevelData() {
        return ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel];
    }

}
