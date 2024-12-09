using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour {

    // placed on level controller object
    // handles main battle loop

    [Header("SAVE/LOAD")]
    [SerializeField] public bool isSandbox;

    [Header("REFERENCES")]
    [SerializeField] public Dictionary<GameObject, Vector2> objectsOnGrid;
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private GameObject levelInfoPanel;
    [SerializeField] private GameObject warriorsContainer;

    [Header("Objects")]
    [SerializeField] private GameObject warriorDrawer;
    [SerializeField] private GameObject enemiesDrawer;
    [SerializeField] private GameObject resetButton;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject levelCompleteMenu;
    [SerializeField] private GameObject levelLostMenu;
    [SerializeField] private GameObject falseStartPrefab;
    
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
    [SerializeField] private int maxTurns = 65;
    public GameObject activeProjectile;
    public bool activeDeathDelay;
    public bool battleFinished = false;
    public bool inBattle = false;
    public bool isPaused = false;


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

    void Awake() {
        CheckSingleton();

        // initialize
        objectsOnGrid = new Dictionary<GameObject, Vector2>();
        HideStatsPanel();
        ToggleResetButton(false);
        TogglePauseButton(false);
    }

    void Start() {
        // check if in sandbox
        isSandbox = ProgressionController.Instance.currentLevel == 0 ? true : false;

        // load data from files
        StartCoroutine(LoadWarriorFileWrapper());
        LoadWarriorDrawer();
        GridSaveLoader.Instance.InitializeGrid();
        if (isSandbox) {
            LoadEnemyDrawer();
        } else {
            enemiesDrawer.SetActive(false);
        }
        if (!isSandbox) {
            SetLevelInfoPanel();
        }

        // set default battle speed
        battleSpeed = 0.51f;
        // play sound
        AudioController.Instance.ChangeBGM("Coding BGM");
    }

    private IEnumerator LoadWarriorFile() {
        // load correct warrior file
        WarriorListController.Instance.FindJSON(isSandbox ? "sandbox_warriors" : "level_warriors");
        yield return null;
    }

    private IEnumerator LoadWarriorFileWrapper() {
        // wait until file loaded
        yield return StartCoroutine(LoadWarriorFile());
    }

    void Update() {
        // continually check to end battle, if conditions are game over
        if (inBattle && (enemyWarriorsList.Count <= 0 || yourWarriorsList.Count <= 0)) {
            EndBattle();
        }
    }

    // DRAWERS
    // Warriors
    public void AddWarriorToDrawer(int index) {
        // add thumbnail to drawer
        Transform container = warriorDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
        Instantiate(warriorThumbnailPrefab, container);
        UpdateWarriorDrawerThumbnail(index);
    }

    public void LoadWarriorDrawer() {
        // loop through all warriors when scene is loaded
        for (int i=0; i < WarriorListController.Instance.GetCount(); i++) {
            AddWarriorToDrawer(i);
        }
    }

    public void UpdateWarriorDrawerThumbnail(int index) {
        // get references
        Transform container = warriorDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
        WarriorFunctionalityData warrior = WarriorListController.Instance.GetWarriorAtIndex(index);
        // update sprite
        GameObject thumbnail = container.GetChild(index).gameObject;
        thumbnail.GetComponent<Image>().sprite = WarriorListController.Instance.spriteDataList[warrior.spriteIndex].sprite;
        // update list reference
        thumbnail.GetComponent<WarriorLevelThumbnail>().warriorIndex = index;
        // update name
        thumbnail.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = warrior.warriorName;
        thumbnail.GetComponent<WarriorLevelThumbnail>().CheckIfPlaceable();
    }

    public void SetAllWarriorThumbnailsGrey(bool value) {
        // loop through all thumbnails
        // grey out if can't be placed
        Transform container = warriorDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
        foreach (Transform child in container) {
            if (value) {
                child.GetComponent<WarriorLevelThumbnail>().SetPlaceable(false);
            } else {
                child.GetComponent<WarriorLevelThumbnail>().CheckIfPlaceable();
            }
        }
    }

    // Enemies
    public void AddEnemyToDrawer(int index) {
        // add thumbnail to drawer
        Transform container = enemiesDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
        GameObject enemyThumbnail = Instantiate(warriorThumbnailPrefab, container);
        enemyThumbnail.GetComponent<WarriorLevelThumbnail>().SetIsEnemy(true);
        UpdateEnemyDrawerThumbnail(index);
    }

    public void LoadEnemyDrawer() {
        // loop through all enemies when scene is loaded
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
        thumbnail.GetComponent<WarriorLevelThumbnail>().CheckIfPlaceable();
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
        // destroy objects
        int childCount = warriorsContainer.transform.childCount;
        foreach (Transform child in warriorsContainer.transform) {
            Destroy(child.gameObject);
        }
    }

    public void ClearGridButton() {
        // only clear if not in battle
        if (inBattle) {
            return;
        }
        // loop through warriors and remove
        foreach (Transform child in warriorsContainer.transform) {
            if (child.GetComponent<WarriorBehavior>().isEnemy && ProgressionController.Instance.currentLevel != 0) { // if enemy and not in sandbox
                continue;
            }
            objectsOnGrid.Remove(child.gameObject);
            Destroy(child.gameObject);
        }
        // ungrey warrior thumbnails
        SetAllWarriorThumbnailsGrey(false);
    }

    public void ResetGridButton() {
        // reset the full level data
        inBattle = false;
        battleFinished = false;
        LoadSavedGrid();
        ToggleResetButton(false);
        TogglePauseButton(false);
        AudioController.Instance.PlaySoundEffect("Reset Battle");
        AudioController.Instance.ChangeBGM("Coding BGM");
    }

    public void ResetWholeLevelButton() {
        // just reload the level
        SceneController.Instance.LoadSceneByName(SceneController.Instance.GetCurrentSceneName());
    }

    public void PauseBattle() {
        // only pause battle if not currently paused
        // this stops the battle from continuing, but continues animation and interaction
        if (inBattle && !isPaused) {
            isPaused = true;
            pauseButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Resume";
            ToggleResetButton(true);
        } else if (inBattle && isPaused) {
            isPaused = false;
            pauseButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Pause";
            ToggleResetButton(false);
        }
        // play sound
        AudioController.Instance.PlaySoundEffect("Pause Battle");
    }

    // Loading Warriors
    public void SetWarriorData(GameObject warrior, bool isEnemy, int warriorIndex) {
        // get warrior data
        WarriorBehavior warriorBehavior = warrior.GetComponent<WarriorBehavior>();

        // set data from all block lists
        // do same thing for enemy and warrior, just from correct list object
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
        warriorBehavior.CheckValidOnGrid();
    }

    // LEVEL INFO PANEL
    public void SetLevelInfoPanel() {
        // set text on level info panel
        levelInfoPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = "Level " + ProgressionController.Instance.currentLevel + ":";
        levelInfoPanel.transform.GetChild(1).GetComponent<TMP_Text>().text = ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].levelName;
        levelInfoPanel.transform.GetChild(2).GetComponent<TMP_Text>().text = "Max Warriors: " + ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxWarriorsToPlace;
        levelInfoPanel.transform.GetChild(3).GetComponent<TMP_Text>().text = "Max Strength: " + ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxTotalStrength;
    }

    // STATS PANEL
    public void ShowStatsPanel(int warriorIndex, bool isEnemy) {
        // show panel if not active
        if (!statsPanel.activeSelf) {
            statsPanel.SetActive(true);
        }
        TMP_Text statsDisplay = GameObject.Find("StatsDisplayScroll").GetComponent<TMP_Text>();

        // same thing for enemy and warrior, just different lists
        // show text versions of properties and behavior code
        if (!isEnemy) {
            WarriorFunctionalityData warrior = WarriorListController.Instance.GetWarriorAtIndex(warriorIndex);
            statsPanel.transform.GetChild(1).GetComponent<Image>().sprite = WarriorListController.Instance.spriteDataList[warrior.spriteIndex].sprite;
            statsPanel.transform.GetChild(1).GetComponent<Image>().preserveAspect = true;
            statsPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = warrior.warriorName;
            statsDisplay.text = " STATS: \n " + PropertiesString(warrior);
            statsDisplay.text += "\n\n " + BehaviorString(warrior);

            if (ProgressionController.Instance.currentLevel != 0) { // update strength and behavior display if not sandbox
                statsPanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Strength: " + warrior.warriorStrength;
                statsPanel.transform.GetChild(4).GetComponent<TMP_Text>().color = warrior.warriorStrength <= ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxTotalStrength ? new Color(104f/255f, 241f/255f, 104f/255f) : new Color(241f/255f, 104f/255f, 104f/255f);
                statsPanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Behaviors: " + warrior.behaviorCount;
                statsPanel.transform.GetChild(5).GetComponent<TMP_Text>().color = warrior.behaviorCount <= ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxBlocks ? new Color(104f/255f, 241f/255f, 104f/255f) : new Color(241f/255f, 104f/255f, 104f/255f);
            }
        } else { // enemy
            WarriorFunctionalityData enemy = EnemyListController.Instance.GetWarriorAtIndex(warriorIndex);
            statsPanel.transform.GetChild(1).GetComponent<Image>().sprite = EnemyListController.Instance.spriteDataList[enemy.spriteIndex].sprite;
            statsPanel.transform.GetChild(1).GetComponent<Image>().preserveAspect = true;
            statsPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = enemy.warriorName;
            statsDisplay.text = " STATS: \n " + PropertiesString(enemy);
            statsDisplay.text += "\n\n " + BehaviorString(enemy);

            if (ProgressionController.Instance.currentLevel != 0) { // update strength and behavior display if not sandbox
                statsPanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "";
                statsPanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "";
            }
        }
    }


    // PRINTING THE CODE
    // // this could also maybe be done by storing the relevant string within the block when saving and then printing them all here
    // // but that's for a refactor later if time
    private string PropertiesString(WarriorFunctionalityData warriorData) {
        // initialize string to hold all property info
        string propertiesString = "";

        // loop through all properties and create a dictionary
        List<BlockDataStruct> warriorProperties = warriorData.properties;
        Dictionary<BlockData.Property, string> propertiesDict = new Dictionary<BlockData.Property, string>();
        foreach (BlockData.Property property in Enum.GetValues(typeof(BlockData.Property))) {
            propertiesDict[property] = "";
        }
        // loop through properties from list and convert to string as dict values
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
                case BlockData.Property.MAGIC_SHIELD:
                    propertiesDict[BlockData.Property.MAGIC_SHIELD] = warriorProperties[i].ToString();
                    break;
            }
        }
        // add each property to the string
        foreach (BlockData.Property property in Enum.GetValues(typeof(BlockData.Property))) {
            if (propertiesDict[property] != "") {
                switch (property) {
                    case BlockData.Property.HEALTH:
                        propertiesString += "Health: " + propertiesDict[property] + "\n ";
                        break;
                    case BlockData.Property.DEFENSE:
                        propertiesString += "Defense: " + propertiesDict[property] + "\n ";
                        break;
                    case BlockData.Property.MOVE_SPEED:
                        propertiesString += "Move Speed: " + propertiesDict[property] + "\n ";
                        break;
                    case BlockData.Property.MELEE_ATTACK_RANGE:
                        propertiesString += "Melee Attack Range: " + propertiesDict[property] + "\n ";
                        break;
                    case BlockData.Property.MELEE_ATTACK_POWER:
                        propertiesString += "Melee Attack Power: " + propertiesDict[property] + "\n ";
                        break;
                    case BlockData.Property.MELEE_ATTACK_SPEED:
                        propertiesString += "Melee Attack Speed: " + propertiesDict[property] + "\n ";
                        break;
                    case BlockData.Property.DISTANCED_RANGE:
                        propertiesString += "Projectile Range: " + propertiesDict[property] + "\n ";
                        break;
                    case BlockData.Property.RANGED_ATTACK_POWER:
                        propertiesString += "Magic Attack Power: " + propertiesDict[property] + "\n ";
                        break;
                    case BlockData.Property.RANGED_ATTACK_SPEED:
                        propertiesString += "Magic Speed: " + propertiesDict[property] + "\n ";
                        break;
                    case BlockData.Property.SPECIAL_POWER:
                        propertiesString += "Special Attack Power: " + propertiesDict[property] + "\n ";
                        break;
                    case BlockData.Property.SPECIAL_SPEED:
                        propertiesString += "Special Attack Speed: " + propertiesDict[property] + "\n ";
                        break;
                    case BlockData.Property.HEAL_POWER:
                        propertiesString += "Heal Power: " + propertiesDict[property] + "\n ";
                        break;
                    case BlockData.Property.HEAL_SPEED:
                        propertiesString += "Heal Speed: " + propertiesDict[property] + "\n ";
                        break;
                    case BlockData.Property.MAGIC_SHIELD:
                        propertiesString += "Magic Shield!\n ";
                        break;
                }
            }
        }
        return propertiesString;
    }

    private string BehaviorString(WarriorFunctionalityData warriorData) {
        // initialize string to hold all behavior info
        string behaviorString = "";
        string indent = "    ";
        int indentLevel = 0;
        // loop behaviors for each header
        List<List<BlockDataStruct>> warriorBehaviorLists = new List<List<BlockDataStruct>> {warriorData.moveFunctions, warriorData.useWeaponFunctions, warriorData.useSpecialFunctions};
        foreach (List<BlockDataStruct> warriorBehaviorList in warriorBehaviorLists) {
            // check which header we're using
            if (warriorBehaviorList == warriorData.moveFunctions) {
                if (!HelperController.Instance.GetCurrentLevelData().isMoveHeaderAvailable) {
                    continue;
                }
                behaviorString += "Move: \n ";
            } else if (warriorBehaviorList == warriorData.useWeaponFunctions) {
                behaviorString += "UseWeapon: \n ";
            } else if (warriorBehaviorList == warriorData.useSpecialFunctions) {
                continue;
            }
            indentLevel += 1;

            // loop through all behaviors in list
            // add to behavior string based on block ID and value
            // indent text as needed to fit under conditionals and loops
            for (int i = 0; i < warriorBehaviorList.Count; i++) {
                switch (warriorBehaviorList[i].behavior) {
                    case BlockData.BehaviorType.TURN:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "TURN ";
                        if (warriorBehaviorList[i].values[0] == "0") { // left
                            behaviorString += "left";
                        } else { // right
                            behaviorString += "right";
                        }
                        behaviorString += "\n ";
                        break;
                    case BlockData.BehaviorType.STEP:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "STEP ";
                        if (warriorBehaviorList[i].values[0] == "0") {
                            behaviorString += "forward";
                        } else if (warriorBehaviorList[i].values[0] == "1") {
                            behaviorString += "backward";
                        } else if (warriorBehaviorList[i].values[0] == "2") {
                            behaviorString += "left";
                        } else if (warriorBehaviorList[i].values[0] == "3") {
                            behaviorString += "right";
                        }
                        behaviorString += "\n ";
                        break;
                    case BlockData.BehaviorType.RUN:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "RUN ";
                        if (warriorBehaviorList[i].values[0] == "0") {
                            behaviorString += "forward";
                        } else if (warriorBehaviorList[i].values[0] == "1") {
                            behaviorString += "backward";
                        } else if (warriorBehaviorList[i].values[0] == "2") {
                            behaviorString += "left";
                        } else if (warriorBehaviorList[i].values[0] == "3") {
                            behaviorString += "right";
                        }
                        behaviorString += "\n ";
                        break;
                    case BlockData.BehaviorType.TELEPORT:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "TELEPORT ";
                        if (warriorBehaviorList[i].values[0] == "0") {
                            behaviorString += "behind target";
                        } else if (warriorBehaviorList[i].values[0] == "1") {
                            behaviorString += "flank target";
                        }
                        behaviorString += "\n ";
                        break;
                    case BlockData.BehaviorType.MELEE_ATTACK:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "DO MELEE";
                        behaviorString += "\n ";
                        break;
                    case BlockData.BehaviorType.SET_TARGET:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "SET TARGET ";
                        if (warriorBehaviorList[i].values[0] == "0") {
                            behaviorString += "nearest ";
                        } else if (warriorBehaviorList[i].values[0] == "1") {
                            behaviorString += "strongest ";
                        } else if (warriorBehaviorList[i].values[0] == "2") {
                            behaviorString += "farthest ";
                        } else if (warriorBehaviorList[i].values[0] == "3") {
                            behaviorString += "weakest ";
                        } else if (warriorBehaviorList[i].values[0] == "4") {
                            behaviorString += "random ";
                        } else if (warriorBehaviorList[i].values[0] == "5") {
                            behaviorString += "healthiest ";
                        } else if (warriorBehaviorList[i].values[0] == "6") {
                            behaviorString += "frailest ";
                        }

                        if (warriorBehaviorList[i].values[1] == "0") {
                            behaviorString += "enemy";
                        } else if (warriorBehaviorList[i].values[1] == "1") {
                            behaviorString += "warrior";
                        }

                        behaviorString += "\n ";
                        break;
                    case BlockData.BehaviorType.WHILE_LOOP:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "WHILE ";
                        if (warriorBehaviorList[i].values[0] == "0") {
                            behaviorString += "target in range ";
                        } else if (warriorBehaviorList[i].values[0] == "1") {
                            behaviorString += "target low health ";
                        } else if (warriorBehaviorList[i].values[0] == "2") {
                            behaviorString += "facing target ";
                        } else if (warriorBehaviorList[i].values[0] == "3") {
                            behaviorString += "self low health ";
                        } else if (warriorBehaviorList[i].values[0] == "4") {
                            behaviorString += "target is healer ";
                        } else if (warriorBehaviorList[i].values[0] == "5") {
                            behaviorString += "target is melee ";
                        } else if (warriorBehaviorList[i].values[0] == "6") {
                            behaviorString += "target is magic ";
                        } else if (warriorBehaviorList[i].values[0] == "7") {
                            behaviorString += "target has shield ";
                        } else if (warriorBehaviorList[i].values[0] == "7") {
                            behaviorString += "in target range ";
                        }

                        behaviorString += "IS ";

                        if (warriorBehaviorList[i].values[1] == "0") {
                            behaviorString += "true";
                        } else if (warriorBehaviorList[i].values[1] == "1") {
                            behaviorString += "false";
                        }

                        behaviorString += ":\n ";
                        indentLevel += 1;
                        break;
                    case BlockData.BehaviorType.FOR_LOOP:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "FOR " + warriorBehaviorList[i].values[0] + " TIMES:";
                        behaviorString += "\n ";
                        indentLevel += 1;
                        break;
                    case BlockData.BehaviorType.END_LOOP:
                        indentLevel -= 1;
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "END WHILE";
                        behaviorString += "\n ";
                        break;
                    case BlockData.BehaviorType.END_FOR:
                        indentLevel -= 1;
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "END FOR";
                        behaviorString += "\n ";
                        break;
                    case BlockData.BehaviorType.IF:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "IF ";
                        if (warriorBehaviorList[i].values[0] == "0") {
                            behaviorString += "target in range ";
                        } else if (warriorBehaviorList[i].values[0] == "1") {
                            behaviorString += "target low health ";
                        } else if (warriorBehaviorList[i].values[0] == "2") {
                            behaviorString += "facing target ";
                        } else if (warriorBehaviorList[i].values[0] == "3") {
                            behaviorString += "self low health ";
                        } else if (warriorBehaviorList[i].values[0] == "4") {
                            behaviorString += "target is healer ";
                        } else if (warriorBehaviorList[i].values[0] == "5") {
                            behaviorString += "target is melee ";
                        } else if (warriorBehaviorList[i].values[0] == "6") {
                            behaviorString += "target is magic ";
                        } else if (warriorBehaviorList[i].values[0] == "7") {
                            behaviorString += "target has shield ";
                        } else if (warriorBehaviorList[i].values[0] == "7") {
                            behaviorString += "in target range ";
                        }

                        behaviorString += "IS ";

                        if (warriorBehaviorList[i].values[1] == "0") {
                            behaviorString += "true";
                        } else if (warriorBehaviorList[i].values[1] == "1") {
                            behaviorString += "false";
                        }

                        behaviorString += ":\n ";
                        indentLevel += 1;
                        break;
                    case BlockData.BehaviorType.ELSE:
                        indentLevel -= 1;
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "ELSE";
                        behaviorString += "\n ";
                        indentLevel += 1;
                        break;
                    case BlockData.BehaviorType.END_IF:
                        indentLevel -= 1;
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "END IF";
                        behaviorString += "\n ";
                        break;
                    case BlockData.BehaviorType.MELEE_SETTINGS:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "MELEE WILL ";
                        if (warriorBehaviorList[i].values[0] == "0") {
                            behaviorString += "attack ";
                        } else if (warriorBehaviorList[i].values[0] == "1") {
                            behaviorString += "heal ";
                        }

                        if (warriorBehaviorList[i].values[1] == "0") {
                            behaviorString += "enemies";
                        } else if (warriorBehaviorList[i].values[1] == "1") {
                            behaviorString += "warriors";
                        }

                        behaviorString += "\n ";
                        break;
                    case BlockData.BehaviorType.RANGED_SETTINGS:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "MAGIC WILL ";
                        if (warriorBehaviorList[i].values[0] == "0") {
                            behaviorString += "attack ";
                        } else if (warriorBehaviorList[i].values[0] == "1") {
                            behaviorString += "heal ";
                        }

                        if (warriorBehaviorList[i].values[1] == "0") {
                            behaviorString += "enemies";
                        } else if (warriorBehaviorList[i].values[1] == "1") {
                            behaviorString += "warriors";
                        }

                        behaviorString += "\n ";
                        break;
                    case BlockData.BehaviorType.FIRE_PROJECTILE:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "DO MAGIC";
                        behaviorString += "\n ";
                        break;
                    case BlockData.BehaviorType.FOREACH_LOOP:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "FOR EACH";
                        if (warriorBehaviorList[i].values[0] == "0") {
                            behaviorString += "enemy ";
                        } else if (warriorBehaviorList[i].values[0] == "1") {
                            behaviorString += "warrior ";
                        }
                        behaviorString += "\n ";
                        break;
                    case BlockData.BehaviorType.RECHARGE_STAMINA:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "RECHARGE STAMINA";
                        behaviorString += "\n ";
                        break;
                }
            }
            // reset indent
            indentLevel = 0;
            behaviorString += "\n ";
        }

        return behaviorString;
    }

    // BUTTON AND PANEL VISIBILITY

    public void HideStatsPanel() {
        statsPanel.SetActive(false);
    }

    public void ToggleResetButton(bool value) {
        resetButton.SetActive(value);
    }

    public void TogglePauseButton(bool value) {
        pauseButton.SetActive(value);
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

    // update battle speed from slider
    // new method
    public void ParseBattleSpeedSlider(System.Single newBattleSpeed) {
        battleSpeed = 1.01f - newBattleSpeed;
    }
    // old method (deprecated)
    public void TryParseBattleSpeed(string newBattleSpeedString) {
        float.TryParse(newBattleSpeedString, out battleSpeed);
    }

    // START BATTLE
    // for the button
    public void StartBattleWrapper() {
        // can only start battle if not in battle
        if (inBattle) {
            return;
        } else if (!CheckAllWarriorsValid()) { // can't start if warriors on grid break restrictions
            GameObject falseStart = Instantiate(falseStartPrefab, Vector3.zero, transform.rotation, GameObject.FindGameObjectWithTag("mainUI").transform);
            falseStart.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            return;
        } else if (battleFinished) { // reset grid if clicked after battle
            ResetGridButton();
        }

        // play sound
        AudioController.Instance.PlaySoundEffect("Start Battle");
        // start battle
        StartCoroutine(StartBattle());
    }

    public bool CheckAllWarriorsValid() {
        // if in sandbox, always valid
        if (ProgressionController.Instance.currentLevel == 0) {
            return true;
        }

        // reset warrior lists
        if (allWarriorsList.Count != 0) {
            ClearWarriorLists();
        }
        // generate new warrior lists
        CreateWarriorLists();
        foreach (WarriorBehavior warrior in allWarriorsList) {
            if (!warrior.isValidStrengthAndBehaviors) {
                // if any warriors invalid, false
                return false;
            }
        }
        return true;
    }

    public IEnumerator StartBattle() {
        // reset flags
        inBattle = true;
        battleFinished = false;
        activeDeathDelay = false;
        // reset warrior lists if they're full
        if (allWarriorsList.Count != 0) {
            ClearWarriorLists();
        }
        // generate new warrior lists
        CreateWarriorLists();

        // reset pause
        isPaused = false;
        TogglePauseButton(true);

        // play battle audio
        AudioController.Instance.ChangeBGM("Battle BGM");

        // while game hasn't been won, lost, or timed out, keep looping through
        int turnCounter = 0;
        while (yourWarriorsList.Count > 0 && enemyWarriorsList.Count > 0 && turnCounter < maxTurns) {
            for (int i = 0; i < allWarriorsList.Count; i++) {
                // move to next warrior if current warrior dead
                if (!allWarriorsList[i].isAlive) {
                    continue;
                }
                // start warrior turn
                allWarriorsList[i].MarkCurrentTurn(true);
                Debug.Log(allWarriorsList[i].warriorName + " at position " + i + " is up!");

                // if battle still going, move and use weapon for current warrior
                if (!battleFinished) {
                    yield return StartCoroutine(allWarriorsList[i].Move());
                }
                if (!battleFinished) {
                    yield return StartCoroutine(allWarriorsList[i].UseWeapon());
                }
                // use special (deprecated)
                // if (!battleFinished) {
                //     yield return StartCoroutine(allWarriorsList[i].UseSpecial());
                // }

                // reset warrior turn
                allWarriorsList[i].MarkCurrentTurn(false);
            }
            // end battle if finished
            if (battleFinished) {
                break;
            }
            // continue to next turn after all warriors go
            turnCounter += 1;
        }
        EndBattle();
    }

    private void EndBattle() {
        Debug.Log("battle over");
        battleFinished = true;
        // destroy all projectiles and melee indicators
        GameObject[] icons = GameObject.FindGameObjectsWithTag("icon");
        foreach (GameObject icon in icons) {
            Destroy(icon);
        }
        // show correct end result
        if (enemyWarriorsList.Count <= 0) {
            Debug.Log("you win!");
            StartCoroutine(ShowEndResults(true, false));
        } else if (yourWarriorsList.Count <= 0) {
            Debug.Log("enemies win!");
            StartCoroutine(ShowEndResults(false, false));
        } else {
            Debug.Log("battle timed out");
            StartCoroutine(ShowEndResults(false, true));
        }
        // disable flags
        TogglePauseButton(false);
        inBattle = false;
        // stop sound
        AudioController.Instance.StopBGM();
    }

    private IEnumerator ShowEndResults(bool battleWon, bool timedOut) {
        // show results faster for sandbox
        yield return new WaitForSeconds(ProgressionController.Instance.currentLevel == 0 ? 0.2f : 0.7f);
        // play sound and show menu based on end result
        if (battleWon) {
            levelCompleteMenu.SetActive(true);
            AudioController.Instance.PlaySoundEffect("Level Win");
        } else {
            levelLostMenu.SetActive(true);
            AudioController.Instance.PlaySoundEffect("Level Lose");
        }
        if (timedOut) {
            AudioController.Instance.PlaySoundEffect("Level Lose");
            try {
                levelLostMenu.transform.GetChild(1).GetComponent<TMP_Text>().text += "\n\n(battle timed out)";
            } catch (System.Exception) {
                Debug.Log("no menu exists to add timeout");
            }
        }
    }

    public void CreateWarriorLists() {
        // loop through all warriors, add to respective lists
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
    }

    public void ClearWarriorLists() {
        // empty warrior and enemy lists
        yourWarriorsList.Clear();
        enemyWarriorsList.Clear();
        allWarriorsList.Clear();
    }
}
