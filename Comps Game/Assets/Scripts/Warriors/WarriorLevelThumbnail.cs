using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class WarriorLevelThumbnail : MonoBehaviour, IBeginDragHandler, IDragHandler {

    // public WarriorListController warriorListController;
    public LevelController levelController;
    public int warriorIndex;
    public GameObject warriorPrefab;

    public bool isEnemy = false;

    private PlacementSystem placementSystem;

    public void Awake() {
        // warriorListController = GameObject.Find("WarriorListPersistent").GetComponent<WarriorListController>();
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
        placementSystem = GameObject.Find("PlacementSystem").GetComponent<PlacementSystem>();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (LevelController.Instance.inBattle || (!LevelController.Instance.inBattle && LevelController.Instance.battleFinished)) {
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

    public void OnDrag(PointerEventData eventData) {

    }



}
