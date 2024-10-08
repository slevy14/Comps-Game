using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class WarriorLevelThumbnail : MonoBehaviour, IBeginDragHandler, IDragHandler {

    public WarriorListController warriorListController;
    public LevelController levelController;
    public int warriorIndex;
    public GameObject warriorPrefab;

    private PlacementSystem placementSystem;

    public void Awake() {
        warriorListController = GameObject.Find("WarriorListPersistent").GetComponent<WarriorListController>();
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
        placementSystem = GameObject.Find("PlacementSystem").GetComponent<PlacementSystem>();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        Vector3 newPoint = Camera.main.ScreenToWorldPoint(new Vector3(this.transform.position.x, this.transform.position.y, 1));
        GameObject warrior = Instantiate(warriorPrefab, newPoint, this.transform.rotation, GameObject.Find("WarriorsContainer").transform);

        warrior.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = levelController.spriteDataList[warriorListController.GetWarriorAtIndex(warriorIndex).spriteIndex].sprite;
        warrior.GetComponent<WarriorBehavior>().SetPropertiesAndBehaviors(warriorListController.GetWarriorAtIndex(warriorIndex).properties, warriorListController.GetWarriorAtIndex(warriorIndex).moveFunctions, warriorListController.GetWarriorAtIndex(warriorIndex).useWeaponFunctions, warriorListController.GetWarriorAtIndex(warriorIndex).useSpecialFunctions);

        eventData.pointerDrag = warrior;
        warrior.GetComponent<WarriorBehavior>().StartDrag();
        Debug.Log("started drag from thumbnail");
        placementSystem.currentDraggingObject = warrior;
    }

    public void OnDrag(PointerEventData eventData) {

    }



}
