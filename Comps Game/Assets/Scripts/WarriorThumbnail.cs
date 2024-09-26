using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WarriorThumbnail : MonoBehaviour, IPointerDownHandler {

    public WarriorListController warriorListController;
    public int warriorIndex;

    public void OnPointerDown(PointerEventData eventData) {
        Debug.Log(gameObject.name + " clicked");
    }

    public void Awake() {
        warriorListController = GameObject.Find("WarriorListPersistent").GetComponent<WarriorListController>();
    }

}
