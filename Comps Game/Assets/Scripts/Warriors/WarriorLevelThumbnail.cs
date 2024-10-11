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
        Vector3 newPoint = Camera.main.ScreenToWorldPoint(new Vector3(this.transform.position.x, this.transform.position.y, 1));
        GameObject warrior = Instantiate(warriorPrefab, newPoint, this.transform.rotation, GameObject.Find("WarriorsContainer").transform);
        WarriorBehavior warriorBehavior = warrior.GetComponent<WarriorBehavior>();

        if (!isEnemy) {
            warrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = WarriorListController.Instance.spriteDataList[WarriorListController.Instance.GetWarriorAtIndex(warriorIndex).spriteIndex].sprite;
            warriorBehavior.SetPropertiesAndBehaviors(WarriorListController.Instance.GetWarriorAtIndex(warriorIndex).properties,
                                                      WarriorListController.Instance.GetWarriorAtIndex(warriorIndex).moveFunctions,
                                                      WarriorListController.Instance.GetWarriorAtIndex(warriorIndex).useWeaponFunctions,
                                                      WarriorListController.Instance.GetWarriorAtIndex(warriorIndex).useSpecialFunctions);
            warriorBehavior.warriorIndex = warriorIndex;
        } else { // PULL FROM ENEMY DATA FOR ENEMY
            warrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = EnemyListController.Instance.spriteDataList[EnemyListController.Instance.GetWarriorAtIndex(warriorIndex).spriteIndex].sprite;
            warriorBehavior.SetPropertiesAndBehaviors(EnemyListController.Instance.GetWarriorAtIndex(warriorIndex).properties,
                                                      EnemyListController.Instance.GetWarriorAtIndex(warriorIndex).moveFunctions,
                                                      EnemyListController.Instance.GetWarriorAtIndex(warriorIndex).useWeaponFunctions,
                                                      EnemyListController.Instance.GetWarriorAtIndex(warriorIndex).useSpecialFunctions);
            warriorBehavior.warriorIndex = warriorIndex;
            // explicitly set enemy
            warriorBehavior.SetIsEnemy();
        }

        eventData.pointerDrag = warrior;
        warrior.GetComponent<WarriorBehavior>().StartDrag();
        // Debug.Log("started drag from thumbnail");
        placementSystem.currentDraggingObject = warrior;

        // FIXME: SHOW STATS SCREEN
        LevelController.Instance.ShowStatsPanel(warriorIndex, warrior.GetComponent<WarriorBehavior>().isEnemy);
    }

    public void OnDrag(PointerEventData eventData) {

    }



}
