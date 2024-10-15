using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DesignerController : MonoBehaviour {

    [Header("DEBUG")]
    [SerializeField] private bool DEBUG_MODE; // set in inspector

    [Space(20)]

    [Header("SAVE/LOAD")]
    [SerializeField] private WarriorListController warriorListController;
    [SerializeField] private EnemyListController enemyListController;
    [SerializeField] private int warriorToLoadIndex;
    [SerializeField] private bool isLoadingWarriorEnemy = false;
    [SerializeField] private bool isCurrentWarriorEnemy = false;
    [SerializeField] public bool justSaved;

    [Space(20)]

    [Header("REFERENCES")]
    [Header("Headers")]
    [SerializeField] private GameObject propertiesHeaderObject;
    [SerializeField] private GameObject moveHeaderObject;
    [SerializeField] private GameObject useWeaponHeaderObject;
    [SerializeField] private GameObject useSpecialHeaderObject;

    [Header("Objects")]
    [SerializeField] private GameObject blockDrawer;
    [SerializeField] private GameObject warriorDrawer;
    [SerializeField] private GameObject enemiesDrawer;
    [SerializeField] private DropdownOptions dropdown;
    [SerializeField] private GameObject whiteboard;
    [SerializeField] private GameObject deleteMenu;
    [SerializeField] private GameObject switchPromptMenu;
    [SerializeField] private GameObject errorPopupMenu;
    
    [Header("Sprites")]
    [SerializeField] private GameObject warriorThumbnailPrefab;
    [SerializeField] public List<SpriteData> spriteDataList;
    [SerializeField] public List<SpriteData> enemySpriteDataList;
    [SerializeField] public int spriteDataIndex;

    [Space(20)]

    [Header("Vars")]
    [Header("Property Blocks")]
    [SerializeField] private List<GameObject> propertyBlocks;
    [SerializeField] private List<GameObject> behaviorBlocks;
    [SerializeField] private int editingIndex;

    // SINGLETON
    public static DesignerController Instance = null; // for persistent

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
        // levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
    }

    // INITIALIZING
    void Start() {
        if (warriorListController == null) {
            warriorListController = WarriorListController.Instance;
        }
        if (enemyListController == null) {
            enemyListController = EnemyListController.Instance;
        }
        LoadWarriorDrawer();
        LoadWarriorToWhiteboard(editingIndex, true, false);

        LoadEnemyDrawer();
    }


    // for buttons
    public void ShowBlockDrawer() {
        if (!blockDrawer.activeSelf) {
            blockDrawer.SetActive(true);
        }
        if (warriorDrawer.activeSelf) {
            warriorDrawer.SetActive(false);
        }
        if (enemiesDrawer.activeSelf) {
            enemiesDrawer.SetActive(false);
        }
    }

    public void ShowWarriorDrawer() {
        if (!warriorDrawer.activeSelf) {
            warriorDrawer.SetActive(true);
        }
        if (blockDrawer.activeSelf) {
            blockDrawer.SetActive(false);
        }
        if (enemiesDrawer.activeSelf) {
            enemiesDrawer.SetActive(false);
        }
    }

    public void ShowEnemiesDrawer() {
        if (!enemiesDrawer.activeSelf) {
            enemiesDrawer.SetActive(true);
        }
        if (blockDrawer.activeSelf) {
            blockDrawer.SetActive(false);
        }
        if (warriorDrawer.activeSelf) {
            warriorDrawer.SetActive(false);
        }
    }

    // switch save confirm buttons
    public void ShowSavePrompt(int warriorIndex, bool isEnemy) {
        warriorToLoadIndex = warriorIndex;
        isLoadingWarriorEnemy = isEnemy;
        if (justSaved) {
            NoSaveSwitch();
        } else {
            switchPromptMenu.SetActive(true);
        }
    }

    public void CancelSwitch() {
        switchPromptMenu.SetActive(false);
    }

    public void SaveAndSwitch() {
        // check for errors before saving!
        if (SaveWarrior()) {
            LoadWarriorToWhiteboard(warriorToLoadIndex, true, isLoadingWarriorEnemy);
        } else {
            Debug.Log("saving error! couldn't switch");
        }
    }

    public void NoSaveSwitch() {
        LoadWarriorToWhiteboard(warriorToLoadIndex, true, isLoadingWarriorEnemy);
    }

    public void SaveWarriorWrapper() {
        SaveWarrior();
    }

    // close error popup
    public void CloseErrorPopup() {
        errorPopupMenu.SetActive(false);
    }


    // WARRIOR CREATION
    public void CreateNewWarrior() {
        // save active warrior first, just in case
        SaveWarrior();
        // get index
        editingIndex = warriorListController.GetCount();
        // reset name display
        GameObject.Find("NamePreview").GetComponent<TMP_Text>().text = "[noname]";
        // reset dropdown
        dropdown.ResetSprite();
        // clear whiteboard
        ClearWhiteboard();
        // save new warrior to list
        InitializeWarrior();
    }

    public void ClearWhiteboard() {
        GameObject whiteboard = GameObject.FindGameObjectWithTag("whiteboard");
        List<GameObject> allBlocks = new List<GameObject>();
        foreach (Transform child in whiteboard.transform) {
            if (!child.GetComponent<Draggable>().isHeader) {
                Destroy(child.gameObject);
            }
        }
        propertiesHeaderObject.GetComponent<Draggable>().SetNextBlock(null);
        moveHeaderObject.GetComponent<Draggable>().SetNextBlock(null);
        useWeaponHeaderObject.GetComponent<Draggable>().SetNextBlock(null);
        useSpecialHeaderObject.GetComponent<Draggable>().SetNextBlock(null);
    }

    public void AddWarriorToDrawer(int index) {
        Transform container = warriorDrawer.transform.GetChild(0).transform.GetChild(0);
        GameObject warriorThumbnail = Instantiate(warriorThumbnailPrefab, container);
        warriorThumbnail.GetComponent<WarriorEditorThumbnail>().isEnemy = false;
        UpdateWarriorDrawerThumbnail(index);
    }

    public void RemoveAllWarriorsFromDrawer() {
        // loop through backwards to remove each warrior
        int count = warriorListController.GetCount() + 1;
        for (int i = count; i > 0; i--) {
            Destroy(warriorDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(i).gameObject);
        }
    }

    public void DebugGetThumbnailData() {
        for (int i = 1; i < warriorListController.GetCount() + 1; i++) {
            GameObject thumbnail = warriorDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(i).gameObject;
            Debug.Log("index " + i + ": setting " + thumbnail.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text + " to sprite " + thumbnail.GetComponent<Image>().sprite);
        }
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
        thumbnail.GetComponent<Image>().sprite = warriorListController.spriteDataList[warrior.spriteIndex].sprite;
        // update list reference
        thumbnail.GetComponent<WarriorEditorThumbnail>().warriorIndex = index;
        // update name
        thumbnail.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = warrior.warriorName;

        // Debug.Log("index " + index + ": setting " + thumbnail.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text + " to sprite " + warrior.spriteIndex);
    }

    public void LoadEnemyDrawer() { // loop through all warriors when scene is loaded
        for (int i=0; i < enemyListController.GetCount(); i++) {
            AddEnemyToDrawer(i);
        }
    }

    public void AddEnemyToDrawer(int index) {
        Transform container = enemiesDrawer.transform.GetChild(0).transform.GetChild(0);
        GameObject enemyThumbnail = Instantiate(warriorThumbnailPrefab, container);
        enemyThumbnail.GetComponent<WarriorEditorThumbnail>().isEnemy = true;
        UpdateEnemyDrawerThumbnail(index);
    }

    public void UpdateEnemyDrawerThumbnail(int index) {
        // get references
        Transform container = enemiesDrawer.transform.GetChild(0).transform.GetChild(0);
        WarriorFunctionalityData enemy = enemyListController.GetWarriorAtIndex(index);
        // update sprite
        GameObject thumbnail = container.GetChild(index).gameObject;
        thumbnail.GetComponent<Image>().sprite = enemyListController.spriteDataList[enemy.spriteIndex].sprite;
        // update list reference
        thumbnail.GetComponent<WarriorEditorThumbnail>().warriorIndex = index;
        // update name
        thumbnail.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = enemy.warriorName;

        // Debug.Log("index " + index + ": setting " + thumbnail.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text + " to sprite " + warrior.spriteIndex);
    }

    public void InitializeWarrior() {
        justSaved = true;
        WarriorFunctionalityData _WarriorFunctionalityData = new WarriorFunctionalityData(editingIndex);
        _WarriorFunctionalityData.spriteIndex = spriteDataIndex;
        _WarriorFunctionalityData.warriorName = ParseName();
        _WarriorFunctionalityData.properties = ParseProperties();
        _WarriorFunctionalityData.moveFunctions = ParseBehaviors(moveHeaderObject);
        _WarriorFunctionalityData.useWeaponFunctions = ParseBehaviors(useWeaponHeaderObject);
        _WarriorFunctionalityData.useSpecialFunctions = ParseBehaviors(useSpecialHeaderObject);
        // SaveIntoJSON(_WarriorFunctionalityData);
        UpdateWarriorList(_WarriorFunctionalityData, isCurrentWarriorEnemy);
        AddWarriorToDrawer(editingIndex);
    }


    // Saving
    public bool SaveWarrior() {
        WarriorFunctionalityData _WarriorFunctionalityData = new WarriorFunctionalityData(editingIndex);
        _WarriorFunctionalityData.spriteIndex = spriteDataIndex;
        _WarriorFunctionalityData.warriorName = ParseName();
        _WarriorFunctionalityData.properties = ParseProperties();
        _WarriorFunctionalityData.moveFunctions = ParseBehaviors(moveHeaderObject);
        _WarriorFunctionalityData.useWeaponFunctions = ParseBehaviors(useWeaponHeaderObject);
        _WarriorFunctionalityData.useSpecialFunctions = ParseBehaviors(useSpecialHeaderObject);

        List<string> errorsList = CheckSaveErrors(_WarriorFunctionalityData);
        if (errorsList.Count == 0) { // if no errors
            justSaved = true;
            // SaveIntoJSON(_WarriorFunctionalityData);
            UpdateWarriorList(_WarriorFunctionalityData, isCurrentWarriorEnemy);
            if (!isLoadingWarriorEnemy) {
                UpdateWarriorDrawerThumbnail(editingIndex);
                warriorListController.FindJSON(); // reload json file
            } else {
                UpdateEnemyDrawerThumbnail(editingIndex);
                enemyListController.FindJSON();
            }
            return true;
        } else {
            justSaved = false;

            TMP_Text errorText = errorPopupMenu.transform.GetChild(1).GetComponent<TMP_Text>(); // text component
            string errorString = "Couldn't save due to the following errors:\n\n";
            foreach (string error in errorsList) {
                errorString += error + "\n";
            }
            errorText.text = errorString;
            errorPopupMenu.SetActive(true);

            return false;
        }
    }


    public void SaveIntoJSON(WarriorFunctionalityData warriorFunctionalityData) {
        string warriorPropertiesJSON = JsonUtility.ToJson(warriorFunctionalityData);
        string filePath = Application.persistentDataPath + $"/{warriorFunctionalityData.warriorName}.json";
        System.IO.File.WriteAllText(filePath, warriorPropertiesJSON);
        Debug.Log("saving json at " + filePath);
    }

    public void UpdateWarriorList(WarriorFunctionalityData warriorFunctionalityData, bool isEnemy) {
        if (!isEnemy) {
            warriorListController.AddWarrior(warriorFunctionalityData.warriorIndex, warriorFunctionalityData);
        } else {
            enemyListController.AddWarrior(warriorFunctionalityData.warriorIndex, warriorFunctionalityData);
        }
    }

    // Error Checking
    public List<string> CheckSaveErrors(WarriorFunctionalityData warriorFunctionalityData) {
        // return true if no errors
        List<string> errorsToOutput = new List<string>();
        // THINGS TO CHECK:
        // must have name
        if (ParseName() == "noname") {
            errorsToOutput.Add("missing name!");
        }
        // must have health
        if (!CheckForHealth()) {
            errorsToOutput.Add("no health! warrior will die immediately :(");
        }
        
        // no unclosed loops/conditionals
            // can check this by seeing if there are any ignores left when parsing loops/conditionals
        // no extraneous endloop/endif/else
        List<string> loopingAndConditionalErrors = new List<string>();
        loopingAndConditionalErrors.AddRange(CheckLoopingConditionalErrors("Move: ", warriorFunctionalityData.moveFunctions));
        loopingAndConditionalErrors.AddRange(CheckLoopingConditionalErrors("Use Weapon: ", warriorFunctionalityData.useWeaponFunctions));
        loopingAndConditionalErrors.AddRange(CheckLoopingConditionalErrors("Use Special: ", warriorFunctionalityData.useSpecialFunctions));
        foreach (string error in loopingAndConditionalErrors) {
            errorsToOutput.Add(error);
        }

        return errorsToOutput;
    }

    // Loading
    public void LoadWarriorToWhiteboard(int index, bool noSave, bool isLoadingEnemy) { 
        if (!noSave) {
            // save previous warrior and clear whiteboard
            SaveWarrior();
        }
        ClearWhiteboard();
        // update index and load sprite
        editingIndex = index;
        // get data for warrior from list and load sprite
        WarriorFunctionalityData warriorData = new WarriorFunctionalityData();

        if (!isLoadingEnemy) {
            warriorData = warriorListController.GetWarriorAtIndex(index);
        } else { // ENEMY
            warriorData = EnemyListController.Instance.GetWarriorAtIndex(index);
        }

        if (!dropdown.gameObject.activeSelf) {
            dropdown.gameObject.SetActive(true);
        }
        dropdown.UpdateSprite(warriorData.spriteIndex, isLoadingEnemy);

        // instantiate blocks onto the whiteboard, positioned and parented
        // might be best to do this by just parenting the objects and then running the update position function on each header
        GameObject currentBlock = propertiesHeaderObject;
        foreach (BlockDataStruct block in warriorData.properties) {
            // instantiate block parented to whiteboard
            GameObject newBlock = Instantiate(propertyBlocks[(int)block.property], this.transform.position, this.transform.rotation, whiteboard.transform);
            // call initialize block draggable
            newBlock.GetComponent<BlockListItem>().InitializeBlockDraggable();
            // update block data
            newBlock.GetComponent<BlockData>().SetBlockDataValues(block.values);
            newBlock.GetComponent<Draggable>().SetMaskable(true);
            try {
                newBlock.GetComponent<Draggable>().SetInputFieldValue(block.values[0]);
            } catch (System.Exception) {
                Debug.Log("no value for current property");
            }
            // set current block.next to instantiated
            currentBlock.GetComponent<Draggable>().SetNextBlock(newBlock);
            newBlock.GetComponent<Draggable>().SetPrevBlock(currentBlock);

            // check if current block needs to adjust next block placement;
            BlockData.BehaviorType blockBehavior = newBlock.GetComponent<BlockData>().behavior;
            bool shiftOffsetBack = blockBehavior == BlockData.BehaviorType.END_LOOP || blockBehavior == BlockData.BehaviorType.END_IF || blockBehavior == BlockData.BehaviorType.ELSE;
            // Debug.Log(shiftOffsetBack);
            if (shiftOffsetBack && newBlock.GetComponent<Draggable>().GetPrevBlock() != null && newBlock.GetComponent<Draggable>().GetPrevBlock().GetComponent<BlockData>().blockType != BlockData.BlockType.HEADER) {
                // Debug.Log("need to shift back");
                newBlock.GetComponent<Draggable>().GetPrevBlock().GetComponent<Draggable>().SetBlockOffset(true);
                // SetBlockOffset(true);
            }
            
            // update current block
            currentBlock = newBlock;
        }
        propertiesHeaderObject.GetComponent<Draggable>().UpdateBlockPositions(propertiesHeaderObject, propertiesHeaderObject.transform.position);

        // repeat for move
        currentBlock = moveHeaderObject;
        foreach (BlockDataStruct block in warriorData.moveFunctions) {
            // instantiate block parented to whiteboard
            GameObject newBlock = Instantiate(behaviorBlocks[(int)block.behavior], this.transform.position, this.transform.rotation, whiteboard.transform);
            // call initialize block draggable
            newBlock.GetComponent<BlockListItem>().InitializeBlockDraggable();
            // update block data
            newBlock.GetComponent<BlockData>().SetBlockDataValues(block.values);
            newBlock.GetComponent<Draggable>().SetMaskable(true);

            switch (block.behavior) {
                // no dropdowns
                case BlockData.BehaviorType.MELEE_ATTACK:
                case BlockData.BehaviorType.FIRE_PROJECTILE:
                case BlockData.BehaviorType.ELSE:
                case BlockData.BehaviorType.END_IF:
                case BlockData.BehaviorType.END_LOOP:
                    break;
                // one dropdown
                case BlockData.BehaviorType.TURN:
                case BlockData.BehaviorType.STEP:
                case BlockData.BehaviorType.RUN:
                case BlockData.BehaviorType.TELEPORT:
                    try {
                        newBlock.GetComponent<Draggable>().SetDropdownValue(block.values[0], 2);
                    } catch (System.Exception) {
                        Debug.Log("couldn't set value, likely an old warrior!");
                    }
                    break;
                // two dropdowns
                case BlockData.BehaviorType.SET_TARGET:
                case BlockData.BehaviorType.MELEE_SETTINGS:
                case BlockData.BehaviorType.RANGED_SETTINGS:
                case BlockData.BehaviorType.WHILE_LOOP:
                case BlockData.BehaviorType.IF:
                    try {
                        newBlock.GetComponent<Draggable>().SetDropdownValue(block.values[0], 2);
                        newBlock.GetComponent<Draggable>().SetDropdownValue(block.values[1], 3);
                    } catch (System.Exception) {
                        Debug.Log("couldn't set value, likely an old warrior!");
                    }
                    break;
                // three dropdowns
                // input field
                case BlockData.BehaviorType.FOR_LOOP:
                    try {
                        newBlock.GetComponent<Draggable>().SetInputFieldValue(block.values[0]);
                    } catch (System.Exception) {
                        Debug.Log("couldn't set value, likely an old warrior!");
                    }
                    break;
            }

            currentBlock.GetComponent<Draggable>().SetNextBlock(newBlock);
            newBlock.GetComponent<Draggable>().SetPrevBlock(currentBlock);

            // check if current block needs to adjust next block placement;
            BlockData.BehaviorType blockBehavior = newBlock.GetComponent<BlockData>().behavior;
            bool shiftOffsetBack = blockBehavior == BlockData.BehaviorType.END_LOOP || blockBehavior == BlockData.BehaviorType.END_IF || blockBehavior == BlockData.BehaviorType.ELSE;
            // Debug.Log(shiftOffsetBack);
            if (shiftOffsetBack && newBlock.GetComponent<Draggable>().GetPrevBlock() != null && newBlock.GetComponent<Draggable>().GetPrevBlock().GetComponent<BlockData>().blockType != BlockData.BlockType.HEADER) {
                // Debug.Log("need to shift back");
                newBlock.GetComponent<Draggable>().GetPrevBlock().GetComponent<Draggable>().SetBlockOffset(true);
                // SetBlockOffset(true);
            }

            // update current block
            currentBlock = newBlock;
        }
        moveHeaderObject.GetComponent<Draggable>().UpdateBlockPositions(moveHeaderObject, moveHeaderObject.transform.position);

        // repeat for weapon
        currentBlock = useWeaponHeaderObject;
        foreach (BlockDataStruct block in warriorData.useWeaponFunctions) {
            // instantiate block parented to whiteboard
            GameObject newBlock = Instantiate(behaviorBlocks[(int)block.behavior], this.transform.position, this.transform.rotation, whiteboard.transform);
            // call initialize block draggable
            newBlock.GetComponent<BlockListItem>().InitializeBlockDraggable();
            // update block data
            newBlock.GetComponent<BlockData>().SetBlockDataValues(block.values);
            newBlock.GetComponent<Draggable>().SetMaskable(true);

            switch (block.behavior) {
                // no dropdowns
                case BlockData.BehaviorType.MELEE_ATTACK:
                case BlockData.BehaviorType.FIRE_PROJECTILE:
                case BlockData.BehaviorType.ELSE:
                case BlockData.BehaviorType.END_IF:
                case BlockData.BehaviorType.END_LOOP:
                    break;
                // one dropdown
                case BlockData.BehaviorType.TURN:
                case BlockData.BehaviorType.STEP:
                case BlockData.BehaviorType.RUN:
                case BlockData.BehaviorType.TELEPORT:
                    try {
                        newBlock.GetComponent<Draggable>().SetDropdownValue(block.values[0], 2);
                    } catch (System.Exception) {
                        Debug.Log("couldn't set value, likely an old warrior!");
                    }
                    break;
                // two dropdowns
                case BlockData.BehaviorType.SET_TARGET:
                case BlockData.BehaviorType.MELEE_SETTINGS:
                case BlockData.BehaviorType.RANGED_SETTINGS:
                case BlockData.BehaviorType.WHILE_LOOP:
                case BlockData.BehaviorType.IF:
                    try {
                        newBlock.GetComponent<Draggable>().SetDropdownValue(block.values[0], 2);
                        newBlock.GetComponent<Draggable>().SetDropdownValue(block.values[1], 3);
                    } catch (System.Exception) {
                        Debug.Log("couldn't set value, likely an old warrior!");
                    }
                    break;
                // three dropdowns
                // input field
                case BlockData.BehaviorType.FOR_LOOP:
                    try {
                        newBlock.GetComponent<Draggable>().SetInputFieldValue(block.values[0]);
                    } catch (System.Exception) {
                        Debug.Log("couldn't set value, likely an old warrior!");
                    }
                    break;
            }


            // set current block.next to instantiated
            currentBlock.GetComponent<Draggable>().SetNextBlock(newBlock);
            newBlock.GetComponent<Draggable>().SetPrevBlock(currentBlock);

            // check if current block needs to adjust next block placement;
            BlockData.BehaviorType blockBehavior = newBlock.GetComponent<BlockData>().behavior;
            bool shiftOffsetBack = blockBehavior == BlockData.BehaviorType.END_LOOP || blockBehavior == BlockData.BehaviorType.END_IF || blockBehavior == BlockData.BehaviorType.ELSE;
            // Debug.Log(shiftOffsetBack);
            if (shiftOffsetBack && newBlock.GetComponent<Draggable>().GetPrevBlock() != null && newBlock.GetComponent<Draggable>().GetPrevBlock().GetComponent<BlockData>().blockType != BlockData.BlockType.HEADER) {
                // Debug.Log("need to shift back");
                newBlock.GetComponent<Draggable>().GetPrevBlock().GetComponent<Draggable>().SetBlockOffset(true);
                // SetBlockOffset(true);
            }

            // update current block
            currentBlock = newBlock;
        }
        useWeaponHeaderObject.GetComponent<Draggable>().UpdateBlockPositions(useWeaponHeaderObject, useWeaponHeaderObject.transform.position);

        // repeat for special
        currentBlock = useSpecialHeaderObject;
        foreach (BlockDataStruct block in warriorData.useSpecialFunctions) {
            // instantiate block parented to whiteboard
            GameObject newBlock = Instantiate(behaviorBlocks[(int)block.behavior], this.transform.position, this.transform.rotation, whiteboard.transform);
            // call initialize block draggable
            newBlock.GetComponent<BlockListItem>().InitializeBlockDraggable();
            // update block data
            newBlock.GetComponent<BlockData>().SetBlockDataValues(block.values);
            newBlock.GetComponent<Draggable>().SetMaskable(true);


            switch (block.behavior) {
                // no dropdowns
                case BlockData.BehaviorType.MELEE_ATTACK:
                case BlockData.BehaviorType.FIRE_PROJECTILE:
                case BlockData.BehaviorType.ELSE:
                case BlockData.BehaviorType.END_IF:
                case BlockData.BehaviorType.END_LOOP:
                    break;
                // one dropdown
                case BlockData.BehaviorType.TURN:
                case BlockData.BehaviorType.STEP:
                case BlockData.BehaviorType.RUN:
                case BlockData.BehaviorType.TELEPORT:
                    try {
                        newBlock.GetComponent<Draggable>().SetDropdownValue(block.values[0], 2);
                    } catch (System.Exception) {
                        Debug.Log("couldn't set value, likely an old warrior!");
                    }
                    break;
                // two dropdowns
                case BlockData.BehaviorType.SET_TARGET:
                case BlockData.BehaviorType.MELEE_SETTINGS:
                case BlockData.BehaviorType.RANGED_SETTINGS:
                case BlockData.BehaviorType.WHILE_LOOP:
                case BlockData.BehaviorType.IF:
                    try {
                        newBlock.GetComponent<Draggable>().SetDropdownValue(block.values[0], 2);
                        newBlock.GetComponent<Draggable>().SetDropdownValue(block.values[1], 3);
                    } catch (System.Exception) {
                        Debug.Log("couldn't set value, likely an old warrior!");
                    }
                    break;
                // three dropdowns
                // input field
                case BlockData.BehaviorType.FOR_LOOP:
                    try {
                        newBlock.GetComponent<Draggable>().SetInputFieldValue(block.values[0]);
                    } catch (System.Exception) {
                        Debug.Log("couldn't set value, likely an old warrior!");
                    }
                    break;
            }


            // set current block.next to instantiated
            currentBlock.GetComponent<Draggable>().SetNextBlock(newBlock);
            newBlock.GetComponent<Draggable>().SetPrevBlock(currentBlock);

            // check if current block needs to adjust next block placement;
            BlockData.BehaviorType blockBehavior = newBlock.GetComponent<BlockData>().behavior;
            bool shiftOffsetBack = blockBehavior == BlockData.BehaviorType.END_LOOP || blockBehavior == BlockData.BehaviorType.END_IF || blockBehavior == BlockData.BehaviorType.ELSE;
            // Debug.Log(shiftOffsetBack);
            if (shiftOffsetBack && newBlock.GetComponent<Draggable>().GetPrevBlock() != null && newBlock.GetComponent<Draggable>().GetPrevBlock().GetComponent<BlockData>().blockType != BlockData.BlockType.HEADER) {
                // Debug.Log("need to shift back");
                newBlock.GetComponent<Draggable>().GetPrevBlock().GetComponent<Draggable>().SetBlockOffset(true);
                // SetBlockOffset(true);
            }

            // update current block
            currentBlock = newBlock;
        }
        useSpecialHeaderObject.GetComponent<Draggable>().UpdateBlockPositions(useSpecialHeaderObject, useSpecialHeaderObject.transform.position);

        isCurrentWarriorEnemy = isLoadingWarriorEnemy;
        // save warrior at end to make sure values are properly updated
        SaveWarrior();
    }

    public string ParseName() {
        string name = "noname";
        GameObject current = propertiesHeaderObject.GetComponent<Draggable>().GetNextBlock();
        while (current != null) {
            BlockData blockData = current.GetComponent<BlockData>();
            if (blockData.blockType == BlockData.BlockType.PROPERTY) {
                if (blockData.property == BlockData.Property.NAME && (blockData.values.Count != 0)) {
                    name = blockData.values[0];
                    GameObject.Find("NamePreview").GetComponent<TMP_Text>().text = blockData.values[0];
                }
            }
            current = current.GetComponent<Draggable>().GetNextBlock();
        }
        return name;
    }

    public bool CheckForHealth() {
        GameObject current = propertiesHeaderObject.GetComponent<Draggable>().GetNextBlock();
        while (current != null) {
            BlockData blockData = current.GetComponent<BlockData>();
            if (blockData.blockType == BlockData.BlockType.PROPERTY && blockData.property == BlockData.Property.HEALTH) {
                if (blockData.values.Count != 0) {
                    return true;
                } else {
                    return false;
                }
            }
            current = current.GetComponent<Draggable>().GetNextBlock();
        }
        return false;
    }

    public List<BlockDataStruct> ParseProperties() {
        List<BlockDataStruct> propertiesList = new List<BlockDataStruct>();
        GameObject current = propertiesHeaderObject.GetComponent<Draggable>().GetNextBlock();
        while (current != null) {
            BlockData blockData = current.GetComponent<BlockData>();
            if (blockData.blockType == BlockData.BlockType.PROPERTY) {
                BlockDataStruct blockDataStruct = blockData.ConvertToStruct();
                propertiesList.Add(blockDataStruct);
                // Debug.Log("added property");
                if (blockData.property == BlockData.Property.NAME) {
                    GameObject.Find("NamePreview").GetComponent<TMP_Text>().text = (blockData.values.Count != 0) ? blockData.values[0] : "[noname]";
                }
            }
            current = current.GetComponent<Draggable>().GetNextBlock();
        }
        return propertiesList;
    }

    public List<BlockDataStruct> ParseBehaviors(GameObject header) {
        List<BlockDataStruct> behaviorsList = new List<BlockDataStruct>();
        GameObject current = header.GetComponent<Draggable>().GetNextBlock();
        while (current != null) {
            BlockData blockData = current.GetComponent<BlockData>();
            if (blockData.blockType == BlockData.BlockType.BEHAVIOR || blockData.blockType == BlockData.BlockType.FUNCTION) {
                switch (blockData.behavior) {
                    // no dropdowns
                    case BlockData.BehaviorType.MELEE_ATTACK:
                    case BlockData.BehaviorType.FIRE_PROJECTILE:
                    case BlockData.BehaviorType.ELSE:
                        if (blockData.values.Count != 0) {
                            blockData.values[0] = (-1).ToString();
                        } else {
                            blockData.values.Add((-1).ToString());
                        }
                        break;
                    case BlockData.BehaviorType.END_IF:
                        if (blockData.values.Count != 0) {
                            blockData.values[0] = (-1).ToString();
                        } else {
                            blockData.values.Add((-1).ToString());
                        }
                        break;
                    case BlockData.BehaviorType.END_LOOP:
                        if (blockData.values.Count != 0) {
                            blockData.values[0] = (-1).ToString();
                        } else {
                            blockData.values.Add((-1).ToString());
                        }
                        break;
                    // one dropdown, store value as string
                    case BlockData.BehaviorType.TURN:
                    case BlockData.BehaviorType.STEP:
                    case BlockData.BehaviorType.RUN:
                    case BlockData.BehaviorType.TELEPORT:
                        if (blockData.values.Count != 0){
                            blockData.values[0] = current.transform.GetChild(2).gameObject.GetComponent<TMP_Dropdown>().value.ToString();
                        } else {
                            blockData.values.Add(current.transform.GetChild(2).gameObject.GetComponent<TMP_Dropdown>().value.ToString());
                        }
                        break;
                    // two dropdowns
                    case BlockData.BehaviorType.SET_TARGET:
                    case BlockData.BehaviorType.MELEE_SETTINGS:
                    case BlockData.BehaviorType.RANGED_SETTINGS:
                    case BlockData.BehaviorType.WHILE_LOOP:
                    case BlockData.BehaviorType.IF:
                        if (blockData.values.Count != 0) {
                            blockData.values[0] = current.transform.GetChild(2).gameObject.GetComponent<TMP_Dropdown>().value.ToString();
                            blockData.values[1] = current.transform.GetChild(3).gameObject.GetComponent<TMP_Dropdown>().value.ToString();
                        } else {
                            blockData.values.Add(current.transform.GetChild(2).gameObject.GetComponent<TMP_Dropdown>().value.ToString());
                            blockData.values.Add(current.transform.GetChild(3).gameObject.GetComponent<TMP_Dropdown>().value.ToString());
                        }
                        break;
                    // three dropdowns
                    // input field
                    case BlockData.BehaviorType.FOR_LOOP:
                        if (blockData.values.Count != 0) {
                            blockData.values[0] = current.transform.GetChild(2).gameObject.GetComponent<TMP_InputField>().text;
                        } else {
                            blockData.values.Add(current.transform.GetChild(2).gameObject.GetComponent<TMP_InputField>().text);
                        }
                        break;
                }
                BlockDataStruct blockDataStruct = blockData.ConvertToStruct();
                behaviorsList.Add(blockDataStruct);
                // Debug.Log("added use special function");
            }
            current = current.GetComponent<Draggable>().GetNextBlock();
        }


        // SET JUMP POINTS FOR LOOPS AND CONDITIONALS
        
        // setup tracker for what we're currently in
        // this is needed to make sure we don't end loops/conditionals at weird times!
        bool inLoop;

        for (int i = 0; i < behaviorsList.Count; i++) {
            int ignores = 0;
            // if WHILE or FOR found:
                // save current index
                // keep iterating to find an END LOOP block
                // if another loop is found along the way, add to list of ignores
                // if END found:
                    // if ignores left, subtract from list of ignores
                    // else, set jump point to that block index + 1
                        // set jump point on that END block to this block index
                // restore index

            if (behaviorsList[i].behavior == BlockData.BehaviorType.WHILE_LOOP) {
                inLoop = true; // track if we are in a loop or a conditional

                int jumpIndex = -1;
                if (behaviorsList[i].values.Count > 2) {
                    behaviorsList[i].values[2] = jumpIndex.ToString();
                } else {
                    behaviorsList[i].values.Add(jumpIndex.ToString());
                }
                // jump point for while loop is values [2]
                // jump point for end loop is values [0]
                for (int j = i+1; j < behaviorsList.Count; j++) {
                    if (behaviorsList[j].behavior == BlockData.BehaviorType.WHILE_LOOP || behaviorsList[j].behavior == BlockData.BehaviorType.FOR_LOOP) {
                        ignores += 1;
                        inLoop = true;
                    } else if (behaviorsList[j].behavior == BlockData.BehaviorType.IF) {
                        inLoop = false;
                    } else if (behaviorsList[j].behavior == BlockData.BehaviorType.END_IF) {
                        inLoop = true;
                    }

                    if (behaviorsList[j].behavior == BlockData.BehaviorType.END_LOOP && inLoop) {
                        if (ignores != 0) {
                            ignores--;
                        } else {
                            // set jump points by adding to list, or updating if they exist
                            behaviorsList[i].values[2] = (j+1).ToString();

                            // set jump point for end loop
                            behaviorsList[j].values[0] = i.ToString();
                            break;
                        }
                    }
                }
            }

            if (behaviorsList[i].behavior == BlockData.BehaviorType.FOR_LOOP) {
                inLoop = true;
                // jump point for for loop is values [1]
                // jump point for end loop is values [0]

                // set default jump point
                int jumpIndex = -1;
                if (behaviorsList[i].values.Count > 1) {
                    behaviorsList[i].values[1] = jumpIndex.ToString();
                } else {
                    behaviorsList[i].values.Add(jumpIndex.ToString());
                }

                for (int j = i+1; j < behaviorsList.Count; j++) {
                    if (behaviorsList[j].behavior == BlockData.BehaviorType.WHILE_LOOP || behaviorsList[j].behavior == BlockData.BehaviorType.FOR_LOOP) {
                        ignores += 1;
                    }  else if (behaviorsList[j].behavior == BlockData.BehaviorType.IF) {
                        inLoop = false;
                    } else if (behaviorsList[j].behavior == BlockData.BehaviorType.END_IF) {
                        inLoop = true;
                    }

                    if (behaviorsList[j].behavior == BlockData.BehaviorType.END_LOOP) {
                        if (ignores != 0) {
                            ignores--;
                        } else {
                            // set jump points by adding to list, or updating if they exist
                            behaviorsList[i].values[1] = (j+1).ToString();

                            // set jump point for end loop
                            behaviorsList[j].values[0] = i.ToString();
                            break;
                        }
                    }
                }
            }
            
            // if IF block found:
                // save current index
                // keep iterating to find an ELSE or an END IF block
                // if another conditional is found along the way, add to list of ignores
                // if ELSE found:
                    // if ignores left, move on
                    // else, set ELSE jump point on this block
                // if END IF found:
                    // if ignores left, subtract from list of ignores
                    // else, set END jump point on this block
                // restore index
            if (behaviorsList[i].behavior == BlockData.BehaviorType.IF) {
                inLoop = false;
                // ONLY NEED 1 JUMP INDEX:
                // will always jump to else if it exists, otherwise to end
                // but that can be stored as the same point! it does not actually matter
                // jump point is values [2]

                // STORING JUMP POINTS IN THE ELSE AND ENDIF BLOCKS ANYWAYS!!
                // helps for error checking

                // set default value in case no else
                int elseIndex = -1;
                if (behaviorsList[i].values.Count > 2) {
                    behaviorsList[i].values[2] = elseIndex.ToString();
                } else {
                    behaviorsList[i].values.Add(elseIndex.ToString());
                }

                for (int j = i+1; j < behaviorsList.Count; j++) {
                    if (behaviorsList[j].behavior == BlockData.BehaviorType.IF) {
                        ignores += 1;
                        inLoop = false;
                    }  else if (behaviorsList[j].behavior == BlockData.BehaviorType.WHILE_LOOP || behaviorsList[j].behavior == BlockData.BehaviorType.FOR_LOOP) {
                        inLoop = true;
                    } else if (behaviorsList[j].behavior == BlockData.BehaviorType.END_LOOP) {
                        inLoop = false;
                    }

                    if (behaviorsList[j].behavior == BlockData.BehaviorType.ELSE && inLoop == false) {
                        if (ignores == 0) {
                            elseIndex = j;
                            behaviorsList[i].values[2] = elseIndex.ToString();
                            behaviorsList[j].values[0] = i.ToString();
                        }
                    }

                    if (behaviorsList[j].behavior == BlockData.BehaviorType.END_IF && inLoop == false) {
                        if (ignores != 0) {
                            ignores--;
                        } else {
                            // set jump point to end if need, otherwise break
                            if (elseIndex == -1) {
                                behaviorsList[i].values[2] = (j+1).ToString();
                            }
                            // assign jump point to endif for error checking
                            behaviorsList[j].values[0] = i.ToString();
                            break;
                        }
                    }
                }
            }
        }

        return behaviorsList;
    }

    public List<string> CheckLoopingConditionalErrors(string headerName, List<BlockDataStruct> blocks) {
        List<string> errorsList = new List<string>();

        foreach (BlockDataStruct block in blocks) {
            switch(block.behavior) {
                case BlockData.BehaviorType.WHILE_LOOP:
                    if (block.values[2] == "-1") {
                        errorsList.Add("mismatched WHILE loop!");
                    }
                    break;
                case BlockData.BehaviorType.FOR_LOOP:   
                    if (block.values[1] == "-1") {
                        errorsList.Add("mismatched FOR loop!");
                    }
                    break;
                case BlockData.BehaviorType.END_LOOP:
                    if (block.values[0] == "-1") {
                        errorsList.Add("mismatched END LOOP");
                    }
                    break;
                case BlockData.BehaviorType.IF:
                    if (block.values[2] == "-1") {
                        errorsList.Add("mismatched IF statement!");
                    }
                    break;
                case BlockData.BehaviorType.ELSE:
                    if (block.values[0] == "-1") {
                        errorsList.Add("mismatched ELSE statement!");
                    }
                    break;
                case BlockData.BehaviorType.END_IF:
                    if (block.values[0] == "-1") {
                        errorsList.Add("mismatched END IF!");
                    }
                    break;
                default:
                    break;
            }
        }
        return errorsList;
    }

    // Deleting
    // first prompt the user with the button to delete the currently selected warrior
    public void PromptDelete() {
        // show object that has the permanent delete button
        deleteMenu.SetActive(true);
    }

    public void DontDelete() {
        deleteMenu.SetActive(false);
    }

    public void DeleteWarrior() {
        // hide object with permanent delete button
        deleteMenu.SetActive(false);

        StartCoroutine(RemoveWarriorsDelay());

        // // ALTERNATIVELY
        // SceneController.Instance.LoadSceneByName("CodeEditor");
    }

    private IEnumerator RemoveWarriorsDelay() {
        // remove warrior from list
        // renumber indices within list
        warriorListController.RemoveWarrior(editingIndex);
        yield return new WaitForSeconds(.01f);
        // clear warrior drawer
        RemoveAllWarriorsFromDrawer();
        yield return new WaitForSeconds(.01f);

        // load warrior at last index --> also clears whiteboard
            // if list now empty, create a new blank one and load it
        // reload warrior drawer
        if (warriorListController.GetCount() == 0) {
            warriorListController.AddWarrior(0, new WarriorFunctionalityData());
        }
        // editingIndex = editingIndex != 0 ? editingIndex -1 : 0;
        LoadWarriorDrawer();
        LoadWarriorToWhiteboard(editingIndex-1, true, false);
        DebugGetThumbnailData();

        warriorListController.FindJSON(); // reload json file
    }

}

