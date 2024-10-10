using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour {

    [Header("SAVE/LOAD")]
    [SerializeField] private WarriorListController warriorListController; 

    [Header("REFERENCES")]
    [SerializeField] public Dictionary<GameObject, Vector2> objectsOnGrid;
    [SerializeField] private GameObject statsPanel;

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


    [Space(20)]
    [Header("BATTLE WARRIORS")]
    [SerializeField] List<WarriorBehavior> allWarriorsList; // sorted by SPEED VAL
    [SerializeField] List<WarriorBehavior> yourWarriorsList;
    [SerializeField] List<WarriorBehavior> enemyWarriorsList;


    // SINGLETON
    public static LevelController Instance = null; // for persistent

    public void CheckSingleton() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }
    }
    //     // Make this object stay around when switching scenes
    //     DontDestroyOnLoad(this.gameObject);
    // }

    void Awake() {
        CheckSingleton();
        objectsOnGrid = new Dictionary<GameObject, Vector2>();
        HideStatsPanel();
    }

    void Start() {
        if (warriorListController == null) {
            warriorListController = WarriorListController.Instance;
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

    public void ShowStatsPanel(int warriorIndex) {
        if (!statsPanel.activeSelf) {
            statsPanel.SetActive(true);
        }

        WarriorFunctionalityData warrior = warriorListController.GetWarriorAtIndex(warriorIndex);

        statsPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = warrior.warriorName;
        statsPanel.transform.GetChild(1).GetComponent<Image>().sprite = spriteDataList[warrior.spriteIndex].sprite;
        statsPanel.transform.GetChild(2).GetComponent<TMP_Text>().text = warrior.warriorName + "'S STATS: \n" + PropertiesString(warrior);
    }

    private string PropertiesString(WarriorFunctionalityData warriorData) {
        string propertiesString = "";
        List<BlockDataStruct> warriorProperties = warriorData.properties;
        Dictionary<BlockData.Property, string> propertiesDict = new Dictionary<BlockData.Property, string>();
        foreach (BlockData.Property property in Enum.GetValues(typeof(BlockData.Property))) {
            // Debug.Log(property);
            propertiesDict[property] = "";
        }
        for (int i = 0; i < warriorProperties.Count; i++) {
            float newVal = 0;
            switch (warriorProperties[i].property) {
                case BlockData.Property.HEALTH:
                    float.TryParse(warriorProperties[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.HEALTH] = newVal.ToString();
                    break;
                case BlockData.Property.DEFENSE:
                    float.TryParse(warriorProperties[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.DEFENSE] = newVal.ToString();
                    break;
                case BlockData.Property.MOVE_SPEED:
                    float.TryParse(warriorProperties[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.MOVE_SPEED] = newVal.ToString();
                    break;
                case BlockData.Property.MELEE_ATTACK_RANGE:
                    float.TryParse(warriorProperties[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.MELEE_ATTACK_RANGE] = newVal.ToString();
                    break;
                case BlockData.Property.MELEE_ATTACK_POWER:
                    float.TryParse(warriorProperties[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.MELEE_ATTACK_POWER] = newVal.ToString();
                    break;
                case BlockData.Property.MELEE_ATTACK_SPEED:
                    float.TryParse(warriorProperties[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.MELEE_ATTACK_SPEED] = newVal.ToString();
                    break;
                case BlockData.Property.DISTANCED_RANGE:
                    float.TryParse(warriorProperties[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.DISTANCED_RANGE] = newVal.ToString();
                    break;
                case BlockData.Property.RANGED_ATTACK_POWER:
                    float.TryParse(warriorProperties[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.RANGED_ATTACK_POWER] = newVal.ToString();
                    break;
                case BlockData.Property.RANGED_ATTACK_SPEED:
                    float.TryParse(warriorProperties[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.RANGED_ATTACK_SPEED] = newVal.ToString();
                    break;
                case BlockData.Property.SPECIAL_POWER:
                    float.TryParse(warriorProperties[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.SPECIAL_POWER] = newVal.ToString();
                    break;
                case BlockData.Property.SPECIAL_SPEED:
                    float.TryParse(warriorProperties[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.SPECIAL_SPEED] = newVal.ToString();
                    break;
                case BlockData.Property.HEAL_POWER:
                    float.TryParse(warriorProperties[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.HEAL_POWER] = newVal.ToString();
                    break;
                case BlockData.Property.HEAL_SPEED:
                    float.TryParse(warriorProperties[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.HEAL_SPEED] = newVal.ToString();
                    break;
            }
        }
        foreach (BlockData.Property property in Enum.GetValues(typeof(BlockData.Property))) {
            if (propertiesDict[property] != "") {
                propertiesString += Enum.GetName(typeof(BlockData.Property), property) + ": " + propertiesDict[property] + "\n";
            }
        }
        return propertiesString;
    }

    public void HideStatsPanel() {
        statsPanel.SetActive(false);
    }


    /*------------------------------------*/
    /*          BEHAVIOR PARSING          */
    /*------------------------------------*/


    /* general flow of level:

        PLAYER WILL:
            investigate enemies
            code warriors
            place warriors
            hit play (player can change speed of fight)
            watch fight
        
        ONCE PLAY PRESSED:
            get list of all warriors
            get list of all enemies
            get list of ALL UNITS TOTAL, SORTED BY SPEED
            WHILE game not won or lost, loop through list of all units:
                FOR unit:
                    CALL Move
                    CALL UseWeapon
                    CALL UseSpecial
                    IF action can't happen (e.g. can't move), it just doesn't
                    limit to how many times a loop can run, functions can be called, etc
                IF all enemies or all allies are defeated:
                    show appropriate message and break from loop
    */

    public void CreateWarriorLists() {
        foreach (KeyValuePair<GameObject, Vector2> warriorObject in objectsOnGrid) {
            if (warriorObject.Key.tag == "warrior") { // add to ally list if ally
                yourWarriorsList.Add(warriorObject.Key.gameObject.GetComponent<WarriorBehavior>());
            }
            if (warriorObject.Key.tag == "enemy") { // add to enemy list if enemy
                enemyWarriorsList.Add(warriorObject.Key.gameObject.GetComponent<WarriorBehavior>());
            }
            // add to overall list too
            allWarriorsList.Add(warriorObject.Key.gameObject.GetComponent<WarriorBehavior>());
        }
        // sort allWarriorsList by speed
        allWarriorsList.Sort((x, y) => y.GetProperty(BlockData.Property.MOVE_SPEED).CompareTo(x.GetProperty(BlockData.Property.MOVE_SPEED)));
        Debug.Log("warriors sorted by Speed: ");
        foreach (WarriorBehavior warrior in allWarriorsList) {
            Debug.Log(warrior.warriorName + ": " + warrior.GetProperty(BlockData.Property.MOVE_SPEED));
        }

    }



}
