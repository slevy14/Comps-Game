using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour {

    [Header("SAVE/LOAD")]
    // [SerializeField] private WarriorListController warriorListController; 
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
    [SerializeField] private int maxTurns = 50;
    public bool battleFinished = false;
    public bool inBattle = false;
    public bool isPaused = false;
    // private bool canResetFromPaused = false;


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
        TogglePauseButton(false);
        // levelCompleteMenu.SetActive(false);
    }

    void Start() {

        isSandbox = ProgressionController.Instance.currentLevel == 0 ? true : false;
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
        // LoadSavedGrid();
    }

    private IEnumerator LoadWarriorFile() {
        WarriorListController.Instance.FindJSON(isSandbox ? "sandbox_warriors" : "level_warriors");
        yield return null;
    }

    private IEnumerator LoadWarriorFileWrapper() {
        yield return StartCoroutine(LoadWarriorFile());
    }

    void Update() {
        if (inBattle && (enemyWarriorsList.Count <= 0 || yourWarriorsList.Count <= 0)) {
            EndBattle();
        }
    }

    // DRAWERS
    // Warriors
    public void AddWarriorToDrawer(int index) {
        Transform container = warriorDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
        Instantiate(warriorThumbnailPrefab, container);
        UpdateWarriorDrawerThumbnail(index);
    }

    public void LoadWarriorDrawer() { // loop through all warriors when scene is loaded
        // Debug.Log("there are " + WarriorListController.Instance.GetCount() + " warriors to add to drawer");
        for (int i=0; i < WarriorListController.Instance.GetCount(); i++) {
            AddWarriorToDrawer(i);
        }
        // Debug.Log("added warriors to placeable drawer");
    }

    public void UpdateWarriorDrawerThumbnail(int index) {
        Debug.Log("updating warrior drawer thumbnail for warrior " + index);
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

    // Enemies
    public void AddEnemyToDrawer(int index) {
        Transform container = enemiesDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
        GameObject enemyThumbnail = Instantiate(warriorThumbnailPrefab, container);
        enemyThumbnail.GetComponent<WarriorLevelThumbnail>().SetIsEnemy(true);
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
        inBattle = false;
        battleFinished = false;
        LoadSavedGrid();
        ToggleResetButton(false);
        TogglePauseButton(false);
        AudioController.Instance.PlaySoundEffect("Reset Battle");
    }

    public void ResetWholeLevelButton() {
        SceneController.Instance.LoadSceneByName(SceneController.Instance.GetCurrentSceneName());
    }

    public void PauseBattle() {
        if (inBattle && !isPaused) {
            isPaused = true;
            pauseButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Resume";
            ToggleResetButton(true);
        } else if (inBattle && isPaused) {
            isPaused = false;
            pauseButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Pause";
            ToggleResetButton(false);
        }
        AudioController.Instance.PlaySoundEffect("Pause Battle");
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
        warriorBehavior.CheckValidOnGrid();
    }

    // LEVEL INFO PANEL

    public void SetLevelInfoPanel() {
        levelInfoPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = "Level " + ProgressionController.Instance.currentLevel + ":";
        levelInfoPanel.transform.GetChild(1).GetComponent<TMP_Text>().text = ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].levelName;
        levelInfoPanel.transform.GetChild(2).GetComponent<TMP_Text>().text = "Max Warriors: " + ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxWarriorsToPlace;
        levelInfoPanel.transform.GetChild(3).GetComponent<TMP_Text>().text = "Max Strength: " + ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxTotalStrength;
    }

    // STATS PANEL
    public void ShowStatsPanel(int warriorIndex, bool isEnemy) {
        if (!statsPanel.activeSelf) {
            statsPanel.SetActive(true);
        }

        // old object before scroll
        // TMP_Text statsDisplay = statsPanel.transform.GetChild(2).GetComponent<TMP_Text>();

        // new object
        TMP_Text statsDisplay = GameObject.Find("StatsDisplayScroll").GetComponent<TMP_Text>();

        if (!isEnemy) {
            WarriorFunctionalityData warrior = WarriorListController.Instance.GetWarriorAtIndex(warriorIndex);
            statsPanel.transform.GetChild(1).GetComponent<Image>().sprite = WarriorListController.Instance.spriteDataList[warrior.spriteIndex].sprite;
            statsPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = warrior.warriorName;
            statsDisplay.text = warrior.warriorName + "'S STATS: \n" + PropertiesString(warrior);
            statsDisplay.text += "\n\n" + BehaviorString(warrior);

            statsPanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "Strength: " + warrior.warriorStrength;
            statsPanel.transform.GetChild(4).GetComponent<TMP_Text>().color = warrior.warriorStrength <= ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxTotalStrength ? new Color(104f/255f, 241f/255f, 104f/255f) : new Color(241f/255f, 104f/255f, 104f/255f);
            statsPanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "Behaviors: " + warrior.behaviorCount;
            statsPanel.transform.GetChild(5).GetComponent<TMP_Text>().color = warrior.behaviorCount <= ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxBlocks ? new Color(104f/255f, 241f/255f, 104f/255f) : new Color(241f/255f, 104f/255f, 104f/255f);
        } else { // enemy
            WarriorFunctionalityData enemy = EnemyListController.Instance.GetWarriorAtIndex(warriorIndex);
            statsPanel.transform.GetChild(1).GetComponent<Image>().sprite = EnemyListController.Instance.spriteDataList[enemy.spriteIndex].sprite;
            statsPanel.transform.GetChild(0).GetComponent<TMP_Text>().text = enemy.warriorName;
            statsDisplay.text = enemy.warriorName + "'S STATS: \n" + PropertiesString(enemy);
            statsDisplay.text += "\n\n" + BehaviorString(enemy);
            statsPanel.transform.GetChild(4).GetComponent<TMP_Text>().text = "";
            statsPanel.transform.GetChild(5).GetComponent<TMP_Text>().text = "";
        }
    }

    // PRINTING THE CODE
    // this could also maybe be done by storing the relevant string within the block when saving and then printing them all here
    // but that's for a refactor later if time

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
        string behaviorString = "";
        string indent = "    ";
        int indentLevel = 0;
        List<List<BlockDataStruct>> warriorBehaviorLists = new List<List<BlockDataStruct>> {warriorData.moveFunctions, warriorData.useWeaponFunctions, warriorData.useSpecialFunctions};
        foreach (List<BlockDataStruct> warriorBehaviorList in warriorBehaviorLists) {
            // check which header we're using
            if (warriorBehaviorList == warriorData.moveFunctions) {
                behaviorString += "Move: \n";
            } else if (warriorBehaviorList == warriorData.useWeaponFunctions) {
                behaviorString += "UseWeapon: \n";
            } else if (warriorBehaviorList == warriorData.useSpecialFunctions) {
                behaviorString += "UseSpecial: \n";
            }
            indentLevel += 1;

            for (int i = 0; i < warriorBehaviorList.Count; i++) {
                switch (warriorBehaviorList[i].behavior) {
                    case BlockData.BehaviorType.TURN:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "TURN ";
                        if (warriorBehaviorList[i].values[0] == "0") {
                            behaviorString += "left";
                        } else {
                            behaviorString += "left";
                        }
                        behaviorString += "\n";
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
                        behaviorString += "\n";
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
                        behaviorString += "\n";
                        break;
                    case BlockData.BehaviorType.TELEPORT:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "TELEPORT ";
                        if (warriorBehaviorList[i].values[0] == "0") {
                            behaviorString += "behind target";
                        } else if (warriorBehaviorList[i].values[0] == "1") {
                            behaviorString += "flank target";
                        }
                        behaviorString += "\n";
                        break;
                    case BlockData.BehaviorType.MELEE_ATTACK:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "DO MELEE";
                        behaviorString += "\n";
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
                        }

                        if (warriorBehaviorList[i].values[1] == "0") {
                            behaviorString += "enemy";
                        } else if (warriorBehaviorList[i].values[1] == "1") {
                            behaviorString += "ally";
                        }

                        behaviorString += "\n";
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
                        }

                        behaviorString += "IS ";

                        if (warriorBehaviorList[i].values[1] == "0") {
                            behaviorString += "true";
                        } else if (warriorBehaviorList[i].values[1] == "1") {
                            behaviorString += "false";
                        }

                        behaviorString += ":\n";
                        indentLevel += 1;
                        break;
                    case BlockData.BehaviorType.FOR_LOOP:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "FOR " + warriorBehaviorList[i].values[0] + " TIMES:";
                        behaviorString += "\n";
                        indentLevel += 1;
                        break;
                    case BlockData.BehaviorType.END_LOOP:
                        indentLevel -= 1;
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "END LOOP";
                        behaviorString += "\n";
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
                        }

                        behaviorString += "IS ";

                        if (warriorBehaviorList[i].values[1] == "0") {
                            behaviorString += "true";
                        } else if (warriorBehaviorList[i].values[1] == "1") {
                            behaviorString += "false";
                        }

                        behaviorString += ":\n";
                        indentLevel += 1;
                        break;
                    case BlockData.BehaviorType.ELSE:
                        indentLevel -= 1;
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "ELSE";
                        behaviorString += "\n";
                        indentLevel += 1;
                        break;
                    case BlockData.BehaviorType.END_IF:
                        indentLevel -= 1;
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "END IF";
                        behaviorString += "\n";
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
                            behaviorString += "allies";
                        }

                        behaviorString += "\n";
                        break;
                    case BlockData.BehaviorType.RANGED_SETTINGS:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "PROJECTILES WILL ";
                        if (warriorBehaviorList[i].values[0] == "0") {
                            behaviorString += "attack ";
                        } else if (warriorBehaviorList[i].values[0] == "1") {
                            behaviorString += "heal ";
                        }

                        if (warriorBehaviorList[i].values[1] == "0") {
                            behaviorString += "enemies";
                        } else if (warriorBehaviorList[i].values[1] == "1") {
                            behaviorString += "allies";
                        }

                        behaviorString += "\n";
                        break;
                    case BlockData.BehaviorType.FIRE_PROJECTILE:
                        behaviorString += string.Concat(Enumerable.Repeat(indent, indentLevel)) + "FIRE PROJECTILE";
                        behaviorString += "\n";
                        break;
                }
            }
            // reset indent
            indentLevel = 0;
            behaviorString += "\n";
        }

        return behaviorString;
    }

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

    public void TryParseBattleSpeed(string newBattleSpeedString) {
        float.TryParse(newBattleSpeedString, out battleSpeed);
    }

    public void ParseBattleSpeedSlider(System.Single newBattleSpeed) {
        battleSpeed = 1.01f - newBattleSpeed;

        // Too loud and repeating lol. prob don't need if we have click effect
        // AudioController.Instance.PlaySoundEffect("Slider Adjust");
    }

    public void StartBattleWrapper() { // for the button
        if (inBattle) {
            return;
        } else if (!CheckAllWarriorsValid()) {
            GameObject falseStart = Instantiate(falseStartPrefab, Vector3.zero, transform.rotation, GameObject.FindGameObjectWithTag("mainUI").transform);
            falseStart.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            return;
        } else if (battleFinished) {
            ResetGridButton();
        }

        AudioController.Instance.PlaySoundEffect("Start Battle");
        StartCoroutine(StartBattle());
    }

    public bool CheckAllWarriorsValid() {
        if (ProgressionController.Instance.currentLevel == 0) {
            return true;
        }

        if (allWarriorsList.Count != 0) {
            ClearWarriorLists();
        }
        // generate new warrior lists
        CreateWarriorLists();
        foreach (WarriorBehavior warrior in allWarriorsList) {
            if (!warrior.isValidStrengthAndBehaviors) {
                return false;
            }
        }
        return true;
    }

    public IEnumerator StartBattle() {
        inBattle = true;
        battleFinished = false;
        // reset warrior lists if they're full
        if (allWarriorsList.Count != 0) {
            ClearWarriorLists();
        }
        // generate new warrior lists
        CreateWarriorLists();

        // reset pause
        isPaused = false;
        TogglePauseButton(true);

        int turnCounter = 0;
        // while game hasn't been won or lost, keep looping through
        while (yourWarriorsList.Count > 0 && enemyWarriorsList.Count > 0 && turnCounter < maxTurns) {
        // for (int j = 0; j < 5; j++) { // temp set times for battle to run
            for (int i =0; i < allWarriorsList.Count; i++) { // doing this as a for loop so things can be deleted if they need to
                if (!allWarriorsList[i].isAlive) {
                    continue;
                }
                Debug.Log(allWarriorsList[i].warriorName + " at position " + i + " is up!");
                allWarriorsList[i].MarkCurrentTurn(true);

                if (!battleFinished) {
                    yield return StartCoroutine(allWarriorsList[i].Move());
                }
                if (!battleFinished) {
                    yield return StartCoroutine(allWarriorsList[i].UseWeapon());
                }
                if (!battleFinished) {
                    yield return StartCoroutine(allWarriorsList[i].UseSpecial());
                }

                allWarriorsList[i].MarkCurrentTurn(false);

                // if (yourWarriorsList.Count <= 0 && enemyWarriorsList.Count <= 0) {
                //     battleFinished = true;
                // }
            }
            if (battleFinished) {
                break;
            }
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
        // ToggleResetButton(true);
        TogglePauseButton(false);
        inBattle = false;
    }

    private IEnumerator ShowEndResults(bool battleWon, bool timedOut) {
        yield return new WaitForSeconds(ProgressionController.Instance.currentLevel == 0 ? 0.2f : 0.7f);
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
