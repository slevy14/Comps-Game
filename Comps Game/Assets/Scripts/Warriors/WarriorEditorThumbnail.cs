using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class WarriorEditorThumbnail : MonoBehaviour, IPointerDownHandler {

    public WarriorListController warriorListController;
    public DesignerController designerController;
    public int warriorIndex;

    public bool isEnemy = false;

    public void Awake() {
        warriorListController = GameObject.Find("WarriorListPersistent").GetComponent<WarriorListController>();
        if (SceneManager.GetActiveScene().name == "CodeEditor") {
            designerController = GameObject.Find("DesignerController").GetComponent<DesignerController>();
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (SceneManager.GetActiveScene().name == "CodeEditor") {            
            // designerController.LoadWarriorToWhiteboard(warriorIndex, false);
            designerController.ShowSavePrompt(warriorIndex);
        }
    }



}
