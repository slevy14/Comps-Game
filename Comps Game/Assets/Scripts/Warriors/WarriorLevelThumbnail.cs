using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WarriorLevelThumbnail : MonoBehaviour, IBeginDragHandler, IDragHandler {

    public LevelController levelController;
    public int warriorIndex;
    public GameObject warriorPrefab;

    public bool isEnemy = false;

    // placeability
    public bool isPlaceable = true;
    public Material greyOut;

    private PlacementSystem placementSystem;

    public void Awake() {
        // set references
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
        placementSystem = GameObject.Find("PlacementSystem").GetComponent<PlacementSystem>();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        // if can't place new warriors at the moment, just show stats (don't drag)
        if (LevelController.Instance.inBattle || (!LevelController.Instance.inBattle && LevelController.Instance.battleFinished) || !isPlaceable) {
            // show stats when dragging warrior from drawer
            LevelController.Instance.ShowStatsPanel(warriorIndex, isEnemy);
            return;
        }

        // create a new warrior object at thumbnail location
        Vector3 newPoint = Camera.main.ScreenToWorldPoint(new Vector3(this.transform.position.x, this.transform.position.y, 1));
        GameObject warrior = Instantiate(warriorPrefab, newPoint, this.transform.rotation, GameObject.Find("WarriorsContainer").transform);
        levelController.SetWarriorData(warrior, isEnemy, warriorIndex);
        warrior.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = !this.isEnemy ? WarriorListController.Instance.spriteDataList[WarriorListController.Instance.GetWarriorAtIndex(warriorIndex).spriteIndex].animatorController : EnemyListController.Instance.spriteDataList[EnemyListController.Instance.GetWarriorAtIndex(warriorIndex).spriteIndex].animatorController;

        // update current dragging object to new warrior object
        eventData.pointerDrag = warrior;
        warrior.GetComponent<WarriorBehavior>().StartDrag();
        placementSystem.currentDraggingObject = warrior;

        // show stats when dragging warrior from drawer
        LevelController.Instance.ShowStatsPanel(warriorIndex, warrior.GetComponent<WarriorBehavior>().isEnemy);
    }

    public void SetIsEnemy(bool value) {
        // flip sprite if enemy
        isEnemy = value;
        if (isEnemy) {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            Transform nameText = transform.GetChild(0).transform;
            nameText.localScale = new Vector3(-nameText.localScale.x, nameText.localScale.y, nameText.localScale.z);
        }
    }

    public void OnDrag(PointerEventData eventData) {
        // doesn't do anything, but needed for interface
    }

    public void CheckIfPlaceable() {
        // automatically placeable if in sandbox, or if this is an enemy
        if (ProgressionController.Instance.currentLevel == 0 || isEnemy) {
            SetPlaceable(true);
            return;
        }

        // placeable if behavior count and strength count are less than max
        SetPlaceable(HelperController.Instance.ValidateBehaviorCount(warriorIndex) && HelperController.Instance.ValidateStrength(warriorIndex));
    }

    public void SetPlaceable(bool value) {
        // determine whether this object is placeable or not
        isPlaceable = value;
        if (!isPlaceable) {
            // grey out if not 
            this.gameObject.GetComponent<Image>().material = greyOut;
        } else {
            this.gameObject.GetComponent<Image>().material = null;
        }
    }

}
