using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WarriorLevelThumbnail : MonoBehaviour, IBeginDragHandler, IDragHandler {

    // public WarriorListController warriorListController;
    public LevelController levelController;
    public int warriorIndex;
    public GameObject warriorPrefab;

    public bool isEnemy = false;

    // placeability
    public bool isPlaceable;
    public Material greyOut;

    private PlacementSystem placementSystem;

    public void Awake() {
        // warriorListController = GameObject.Find("WarriorListPersistent").GetComponent<WarriorListController>();
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
        placementSystem = GameObject.Find("PlacementSystem").GetComponent<PlacementSystem>();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (LevelController.Instance.inBattle || (!LevelController.Instance.inBattle && LevelController.Instance.battleFinished) || !isPlaceable) {
            LevelController.Instance.ShowStatsPanel(warriorIndex, isEnemy);
            return;
        }
        Vector3 newPoint = Camera.main.ScreenToWorldPoint(new Vector3(this.transform.position.x, this.transform.position.y, 1));
        GameObject warrior = Instantiate(warriorPrefab, newPoint, this.transform.rotation, GameObject.Find("WarriorsContainer").transform);
        
        levelController.SetWarriorData(warrior, isEnemy, warriorIndex);
        Debug.Log("warrior " + warrior.GetComponent<WarriorBehavior>().warriorName + ". is enemy " + isEnemy + ". index: " + warriorIndex);
        warrior.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = !this.isEnemy ? WarriorListController.Instance.spriteDataList[WarriorListController.Instance.GetWarriorAtIndex(warriorIndex).spriteIndex].animatorController : EnemyListController.Instance.spriteDataList[EnemyListController.Instance.GetWarriorAtIndex(warriorIndex).spriteIndex].animatorController;

        eventData.pointerDrag = warrior;
        warrior.GetComponent<WarriorBehavior>().StartDrag();
        // Debug.Log("started drag from thumbnail");
        placementSystem.currentDraggingObject = warrior;

        LevelController.Instance.ShowStatsPanel(warriorIndex, warrior.GetComponent<WarriorBehavior>().isEnemy);
    }

    public void SetIsEnemy(bool value) {
        isEnemy = value;
        if (isEnemy) {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            Transform nameText = transform.GetChild(0).transform;
            nameText.localScale = new Vector3(-nameText.localScale.x, nameText.localScale.y, nameText.localScale.z);
        }
    }

    public void OnDrag(PointerEventData eventData) {

    }

    public void CheckIfPlaceable() {
        if (ProgressionController.Instance.currentLevel == 0 || isEnemy) { // sandbox check
            SetPlaceable(true);
            return;
        }

        // Debug.Log($"too many blocks for {WarriorListController.Instance.GetWarriorAtIndex(warriorIndex).warriorName}: " + (count > ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxBlocks));
        SetPlaceable(HelperController.Instance.ValidateBehaviorCount(warriorIndex) && HelperController.Instance.ValidateStrength(warriorIndex));
    }

    public void SetPlaceable(bool value) {
        isPlaceable = value;
        if (!isPlaceable) {
            this.gameObject.GetComponent<Image>().material = greyOut;
        } else {
            this.gameObject.GetComponent<Image>().material = null;
        }
    }

}
