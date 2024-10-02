using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class WarriorThumbnail : MonoBehaviour, IPointerDownHandler {

    public WarriorListController warriorListController;
    public DesignerController designerController;
    public int warriorIndex;

    public void Awake() {
        warriorListController = GameObject.Find("WarriorListPersistent").GetComponent<WarriorListController>();
        if (SceneManager.GetActiveScene().name == "CodeEditor") {
            designerController = GameObject.Find("DesignerController").GetComponent<DesignerController>();
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        Debug.Log(gameObject.name + " clicked at index " + warriorIndex);
        if (SceneManager.GetActiveScene().name == "CodeEditor") {            
            designerController.LoadWarriorToWhiteboard(warriorIndex, false);
        }
    }



}
