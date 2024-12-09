using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class WarriorEditorThumbnail : MonoBehaviour, IPointerDownHandler {

    // placed on warrior and enemy thumbnail prefabs
    // for the code editor (different data for level thumbnails)

    // data set when instantiated

    public WarriorListController warriorListController;
    public DesignerController designerController;
    public int warriorIndex;
    public int thumbnailIndex;

    public bool isEnemy = false;

    public void Awake() {
        // set references
        warriorListController = GameObject.Find("WarriorListPersistent").GetComponent<WarriorListController>();
        if (SceneManager.GetActiveScene().name == "CodeEditor") {
            designerController = GameObject.Find("DesignerController").GetComponent<DesignerController>();
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (SceneManager.GetActiveScene().name == "CodeEditor") {
            // prompt warrior switch if clicked in the editor
            designerController.ShowSwitchSavePrompt(warriorIndex, thumbnailIndex, isEnemy);
        }
    }



}
