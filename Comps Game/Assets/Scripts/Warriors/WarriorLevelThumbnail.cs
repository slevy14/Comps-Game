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

    public void Awake() {
        warriorListController = GameObject.Find("WarriorListPersistent").GetComponent<WarriorListController>();
        levelController = LevelController.Instance;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        Vector3 newPoint = Camera.main.ScreenToWorldPoint(new Vector3(this.transform.position.x, this.transform.position.y, 1));
        GameObject warrior = Instantiate(warriorPrefab, newPoint, this.transform.rotation, GameObject.Find("WarriorsContainer").transform);
        eventData.pointerDrag = warrior;
    }

    public void OnDrag(PointerEventData eventData) {

    }



}
