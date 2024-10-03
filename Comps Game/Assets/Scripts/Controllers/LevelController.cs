using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour {

    [Header("SAVE/LOAD")]
    [SerializeField] private WarriorListController warriorListController; 

    [Header("REFERENCES")]
    [SerializeField] public Dictionary<GameObject, Vector2> objectsOnGrid;

    [Header("Objects")]
    [SerializeField] private GameObject warriorDrawer;
    
    [Header("Sprites")]
    [SerializeField] private GameObject warriorThumbnailPrefab;
    [SerializeField] public List<SpriteData> spriteDataList;
    [SerializeField] public int spriteDataIndex;

    [Header("Vars")]
    [Header("Property Blocks")]
    [SerializeField] private List<GameObject> propertyBlocks;
    [SerializeField] private List<GameObject> behaviorBlocks;
    [SerializeField] private int editingIndex;


    // // SINGLETON
    // public static LevelController Instance = null; // for persistent

    // public void Awake() {
    //     CheckSingleton();
    // }

    // public void CheckSingleton() {
    //     if (Instance == null) {
    //         Instance = this;
    //     } else {
    //         Destroy(this.gameObject);
    //         return;
    //     }
    // }
    //     // Make this object stay around when switching scenes
    //     DontDestroyOnLoad(this.gameObject);
    // }

    void Awake() {
        objectsOnGrid = new Dictionary<GameObject, Vector2>();
    }

    void Start() {
        if (warriorListController == null) {
            warriorListController = GameObject.Find("WarriorListPersistent").GetComponent<WarriorListController>();
        }
        LoadWarriorDrawer();
    }

    public void AddWarriorToDrawer(int index) {
        Transform container = warriorDrawer.transform.GetChild(0).transform.GetChild(0);
        Instantiate(warriorThumbnailPrefab, container);
        UpdateWarriorDrawerThumbnail(index);
    }

    public void LoadWarriorDrawer() { // loop through all warriors when scene is loaded
        for (int i=0; i < warriorListController.GetCount(); i++) {
            AddWarriorToDrawer(i);
        }
    }

    public void UpdateWarriorDrawerThumbnail(int index) {
        // get references
        Transform container = warriorDrawer.transform.GetChild(0).transform.GetChild(0);
        WarriorFunctionalityData warrior = warriorListController.GetWarriorAtIndex(index);
        // update sprite
        GameObject thumbnail = container.GetChild(index+1).gameObject;
        thumbnail.GetComponent<Image>().sprite = spriteDataList[warrior.spriteIndex].sprite;
        // update list reference
        thumbnail.GetComponent<WarriorLevelThumbnail>().warriorIndex = index;
        // update name
        thumbnail.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = warrior.warriorName;
    }

}
