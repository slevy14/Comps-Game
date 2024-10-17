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
    [SerializeField] private GameObject warriorsContainer;

    [Header("Objects")]
    [SerializeField] private GameObject warriorDrawer;
    [SerializeField] private GameObject enemiesDrawer;
    [SerializeField] private GameObject resetButton;
    
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
    [SerializeField] public List<WarriorBehavior> allWarriorsList; // sorted by SPEED VAL
    [SerializeField] public List<WarriorBehavior> yourWarriorsList;
    [SerializeField] public List<WarriorBehavior> enemyWarriorsList;

    [Header("Battle")]
    [SerializeField] public float battleSpeed;
    [SerializeField] private int maxTurns = 50;


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
        ToggleResetButton(false);
    }

    void Start() {
        if (warriorListController == null) {
            warriorListController = WarriorListController.Instance;
        }
        LoadWarriorDrawer();
        LoadEnemyDrawer();

        // LoadSavedGrid();
    }

    // DRAWERS
    // Warriors
    public void AddWarriorToDrawer(int index) {
        Transform container = warriorDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
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
        Transform container = warriorDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
        WarriorFunctionalityData warrior = warriorListController.GetWarriorAtIndex(index);
        // update sprite
        GameObject thumbnail = container.GetChild(index).gameObject;
        thumbnail.GetComponent<Image>().sprite = warriorListController.spriteDataList[warrior.spriteIndex].sprite;
        // update list reference
        thumbnail.GetComponent<WarriorLevelThumbnail>().warriorIndex = index;
        // update name
        thumbnail.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = warrior.warriorName;
    }

    // Enemies
    public void AddEnemyToDrawer(int index) {
        Transform container = enemiesDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
        GameObject enemyThumbnail = Instantiate(warriorThumbnailPrefab, container);
        enemyThumbnail.GetComponent<WarriorLevelThumbnail>().isEnemy = true;
        UpdateEnemyDrawerThumbnail(index);
    }

    public void LoadEnemyDrawer() { // loop through all warriors when scene is loaded
        for (int i=0; i < EnemyListController.Instance.GetCount(); i++) {
            AddEnemyToDrawer(i);
        }
    }

    public void UpdateEnemyDrawerThumbnail(int index) {
        // get references
        Transform container = enemiesDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
        WarriorFunctionalityData enemy = EnemyListController.Instance.GetWarriorAtIndex(index);
        // update sprite
        GameObject thumbnail = container.GetChild(index).gameObject;
        thumbnail.GetComponent<Image>().sprite = EnemyListController.Instance.spriteDataList[enemy.spriteIndex].sprite;
        // update list reference
        thumbnail.GetComponent<WarriorLevelThumbnail>().warriorIndex = index;
        // update name
        thumbnail.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = enemy.warriorName;

        // Debug.Log("index " + index + ": setting " + thumbnail.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text + " to sprite " + warrior.spriteIndex);
    }

    // SAVE / LOAD GRID
    public void LoadSavedGrid() {
        // clear grid
        ClearGrid();
        Debug.Log("grid cleared");
        GridSaveLoader.Instance.LoadGridFromJson();
    }

    public void ClearGrid() {
        // clear dict
        objectsOnGrid.Clear();
        Debug.Log("cleared from dict");
        // destroy objects
        int childCount = warriorsContainer.transform.childCount;
        Debug.Log("child count" + childCount);
        // for (int i = 0; i < childCount; i++) {
        //     Debug.Log("destroying object");
        //     Destroy(warriorsContainer.transform.GetChild(0).gameObject);
        // }
        foreach (Transform child in warriorsContainer.transform) {
            Destroy(child.gameObject);
        }
    }

    public void ResetGridButton() {
        Debug.Log("reset button pressed");
        LoadSavedGrid();
        ToggleResetButton(false);
    }

    // Loading Warriors
    public void SetWarriorData(GameObject warrior, bool isEnemy, int warriorIndex) {
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
    }


    // STATS PANEL
    public void ShowStatsPanel(int warriorIndex, bool isEnemy) {
        if (!statsPanel.activeSelf) {
            statsPanel.SetActive(true);
        }

        if (!isEnemy) {
            WarriorFunctionalityData warrior = warriorListController.GetWarriorAtIndex(warriorIndex);
            statsPanel.transform.GetChild(1).GetComponent<Image>().sprite = warriorListController.spriteDataList[warrior.spriteIndex].sprite;
            statsPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = warrior.warriorName;
            statsPanel.transform.GetChild(2).GetComponent<TMP_Text>().text = warrior.warriorName + "'S STATS: \n" + PropertiesString(warrior);
        } else { // enemy
            WarriorFunctionalityData warrior = EnemyListController.Instance.GetWarriorAtIndex(warriorIndex);
            statsPanel.transform.GetChild(1).GetComponent<Image>().sprite = EnemyListController.Instance.spriteDataList[warrior.spriteIndex].sprite;
            statsPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = warrior.warriorName;
            statsPanel.transform.GetChild(2).GetComponent<TMP_Text>().text = warrior.warriorName + "'S STATS: \n" + PropertiesString(warrior);
        }
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

    private string BehaviorString(WarriorFunctionalityData warriorData) {
        // FIXME: add ability to print behaviors as string to the stats screen
        return null;
    }

    public void HideStatsPanel() {
        statsPanel.SetActive(false);
    }

    public void ToggleResetButton(bool value) {
        resetButton.SetActive(value);
    }


    /*--------------------------*/
    /*          BATTLE          */
    /*--------------------------*/


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

        ACTUAL PARSING DONE ON WARRIOR BEHAVIOR SCRIPT
        
    */

    public void TryParseBattleSpeed(string newBattleSpeedString) {
        float.TryParse(newBattleSpeedString, out battleSpeed);
    }

    public void ParseBattleSpeedSlider(System.Single newBattleSpeed) {
        battleSpeed = 3.1f - newBattleSpeed;
    }

    public void StartBattleWrapper() { // for the button
        StartCoroutine(StartBattle());
    }

    public IEnumerator StartBattle() {
        // reset warrior lists if they're full
        if (allWarriorsList.Count != 0) {
            ClearWarriorLists();
        }
        // generate new warrior lists
        CreateWarriorLists();

        int turnCounter = 0;
        bool battleFinished = false;
        // while game hasn't been won or lost, keep looping through
        while (yourWarriorsList.Count > 0 && enemyWarriorsList.Count > 0 && turnCounter < maxTurns) {
        // for (int j = 0; j < 5; j++) {
            for (int i =0; i < allWarriorsList.Count; i++) { // doing this as a for loop so things can be deleted if they need to
                if (!allWarriorsList[i].isAlive) {
                    continue;
                }
                Debug.Log(allWarriorsList[i].warriorName + " at position " + i + " is up!");
                yield return StartCoroutine(allWarriorsList[i].Move());
                yield return StartCoroutine(allWarriorsList[i].UseWeapon());
                yield return StartCoroutine(allWarriorsList[i].UseSpecial());

                if (yourWarriorsList.Count <= 0 && enemyWarriorsList.Count <= 0) {
                    battleFinished = true;
                }
            }
            if (battleFinished) {
                break;
            }
            turnCounter += 1;
        }
        Debug.Log("battle over");
        if (enemyWarriorsList.Count <= 0) {
            Debug.Log("you win!");
        } else if (yourWarriorsList.Count <= 0) {
            Debug.Log("enemies win!");
        } else {
            Debug.Log("battle timed out");
        }
        ToggleResetButton(true);
    }

    public void CreateWarriorLists() {
        foreach (KeyValuePair<GameObject, Vector2> warriorObject in objectsOnGrid) {
            if (warriorObject.Key.tag == "warrior") { // add to ally list if ally
                yourWarriorsList.Add(warriorObject.Key.gameObject.GetComponent<WarriorBehavior>());
                // Debug.Log("found ally: " + warriorObject.Key.gameObject.GetComponent<WarriorBehavior>().warriorName);
            }
            if (warriorObject.Key.tag == "enemy") { // add to enemy list if enemy
                enemyWarriorsList.Add(warriorObject.Key.gameObject.GetComponent<WarriorBehavior>());
                // Debug.Log("found enemy: " + warriorObject.Key.gameObject.GetComponent<WarriorBehavior>().warriorName);
            }
            // add to overall list too
            allWarriorsList.Add(warriorObject.Key.gameObject.GetComponent<WarriorBehavior>());
        }
        // sort allWarriorsList by speed
        allWarriorsList.Sort((x, y) => y.GetProperty(BlockData.Property.MOVE_SPEED).CompareTo(x.GetProperty(BlockData.Property.MOVE_SPEED)));
        Debug.Log("warriors sorted by Speed: ");
        int count = 0;
        foreach (WarriorBehavior warrior in allWarriorsList) {
            Debug.Log(count + " " + warrior.warriorName + ": " + warrior.GetProperty(BlockData.Property.MOVE_SPEED));
            count++;
        }
    }

    public void ClearWarriorLists() {
        yourWarriorsList.Clear();
        enemyWarriorsList.Clear();
        allWarriorsList.Clear();
    }



}
