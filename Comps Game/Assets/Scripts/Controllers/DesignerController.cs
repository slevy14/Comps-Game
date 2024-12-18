using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DesignerController : MonoBehaviour {

    [Header("Meta")]
    [SerializeField] private bool DEBUG_MODE; // set in inspector
    [SerializeField] public bool isSandbox;

    [Space(20)]

    [Header("SAVE/LOAD")]
    [SerializeField] private int warriorToLoadIndex;
    [SerializeField] private int warriorToLoadThumbnailIndex;
    [SerializeField] private bool isLoadingWarriorEnemy = false;
    [SerializeField] public bool isCurrentWarriorEnemy = false;
    [SerializeField] public bool justSaved;
    [SerializeField] private int editingWarriorIndex;
    [SerializeField] private int editingThumbnailIndex;


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
    [SerializeField] private GameObject switchLevelButtonObject;
    [SerializeField] public GameObject overlapSpaceIndicator;
    [SerializeField] private TMP_Text strengthDisplay;
    [SerializeField] private TMP_Text maxBehaviorsDisplay;
    [SerializeField] private GameObject saveButton;
    [SerializeField] private GameObject deleteButton;
    [SerializeField] private GameObject textThe;

    
    [Header("Sprites")]
    [SerializeField] private GameObject warriorThumbnailPrefab;
    [SerializeField] public List<SpriteData> spriteDataList;
    [SerializeField] public List<SpriteData> enemySpriteDataList;
    [SerializeField] public int spriteDataIndex;

    [Space(20)]

    [Header("Blocks")]
    [SerializeField] private List<GameObject> propertyBlocks;
    [SerializeField] private List<GameObject> behaviorBlocks;
    [SerializeField] private GameObject spacer;
    [SerializeField] private GameObject sectionHeader;


    // SINGLETON
    public static DesignerController Instance = null; 

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
    }

    // INITIALIZING
    void Start() {
        // load first enemy, initialize drawers, initialize buttons
        isSandbox = ProgressionController.Instance.currentLevel == 0 ? true : false;
        LoadWarriorFile();
        LoadWarriorDrawer();
        LoadWarriorToWhiteboard(GetLastEditedWarrior(), GetLastEditedWarrior(), true, false);
        LoadEnemyDrawer();

        InitializeBlocksDrawer();

        InitializeLevelSwitchButton();

        // disable tutorial replay button if in sandbox
        if (isSandbox) {
            GameObject.Find("BearButton").SetActive(false);
        }

        // play sound
        AudioController.Instance.ChangeBGM("Coding BGM");
    }

    private void LoadWarriorFile() {
        // load warriors file based on level
        WarriorListController.Instance.FindJSON(isSandbox ? "sandbox_warriors" : "level_warriors");
    }

    private int GetLastEditedWarrior() {
        // get most recently edited warrior, as saved to progression controlelr
        if (ProgressionController.Instance.lastEditedWarrior <= WarriorListController.Instance.GetCount()) {
            return ProgressionController.Instance.lastEditedWarrior;
        }
        return 0;
    }

    private void InitializeLevelSwitchButton() {
        // set level switcher to show text based on which level
        switchLevelButtonObject.GetComponent<Button>().onClick.RemoveAllListeners();
        switchLevelButtonObject.GetComponent<Button>().onClick.AddListener(delegate {ShowLevelLoadSavePrompt();});
        if (ProgressionController.Instance.currentLevel == 0) { // make sandbox button if sandbox
            switchLevelButtonObject.transform.GetChild(0).GetComponent<TMP_Text>().text = "To Sandbox";
        } else { // make return to level button
            switchLevelButtonObject.transform.GetChild(0).GetComponent<TMP_Text>().text = "To Level";
        }
    }

    // getters

    public List<GameObject> GetPropertyBlocks() {
        return propertyBlocks;
    }

    public List<GameObject> GetBehaviorBlocks() {
        return behaviorBlocks;
    }


    // for buttons
    // these all deactivate themselves when clicked
    // and activate the others
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
    public void ToggleSavePromptMenu(bool value, bool isSwitch) {
        // show prompt menu
        switchPromptMenu.SetActive(value);
        if (value == false) {
            return;
        }

        // reset data on buttons
        GameObject saveContinueButton = switchPromptMenu.transform.GetChild(0).gameObject;
        GameObject dontSaveContinueButton = switchPromptMenu.transform.GetChild(2).gameObject;
        saveContinueButton.GetComponent<Button>().onClick.RemoveAllListeners();
        dontSaveContinueButton.GetComponent<Button>().onClick.RemoveAllListeners();
        // change how buttons function based on movement to level or switching between warriors
        if (!isSwitch) {
            saveContinueButton.GetComponent<Button>().onClick.AddListener(delegate {SaveAndContinue(ProgressionController.Instance.currentLevel == 0 ? "Sandbox" : "LevelScene");});
            saveContinueButton.GetComponent<Button>().onClick.AddListener(delegate {CancelSwitch();});
            saveContinueButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Save & Continue";
            dontSaveContinueButton.GetComponent<Button>().onClick.AddListener(delegate {NoSaveContinue(ProgressionController.Instance.currentLevel == 0 ? "Sandbox" : "LevelScene");});
            dontSaveContinueButton.GetComponent<Button>().onClick.AddListener(delegate {CancelSwitch();});
            dontSaveContinueButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Don't Save";
            switchPromptMenu.transform.GetChild(3).GetComponent<TMP_Text>().text = "Would you like to save before you move to the level?";
        } else {
            saveContinueButton.GetComponent<Button>().onClick.AddListener(delegate {SaveAndSwitch();});
            saveContinueButton.GetComponent<Button>().onClick.AddListener(delegate {CancelSwitch();});
            saveContinueButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Save & Switch";
            dontSaveContinueButton.GetComponent<Button>().onClick.AddListener(delegate {NoSaveSwitch();});
            dontSaveContinueButton.GetComponent<Button>().onClick.AddListener(delegate {CancelSwitch();});
            dontSaveContinueButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Don't Save";
            switchPromptMenu.transform.GetChild(3).GetComponent<TMP_Text>().text = "Would you like to save before you switch warriors?";
        }
    }

    public void ShowSwitchSavePrompt(int warriorIndex, int thumbnailIndex, bool isEnemy) {
        // get data for warrior to load
        warriorToLoadIndex = warriorIndex;
        warriorToLoadThumbnailIndex = thumbnailIndex;
        isLoadingWarriorEnemy = isEnemy;
        // auto switch if just saved
        if (justSaved) {
            NoSaveSwitch();
        } else { // otherwise prompt save
            ToggleSavePromptMenu(true, true);
        }
    }

    public void ShowLevelLoadSavePrompt() {
        // continue to level if just saved
        if (justSaved) {
            NoSaveContinue(ProgressionController.Instance.currentLevel == 0 ? "Sandbox" : "LevelScene");
        } else { // otherwise prompt save
            ToggleSavePromptMenu(true, false);
        }
    }

    public void CancelSwitch() {
        // hide menu on cancel
        ToggleSavePromptMenu(false, false);
    }

    public void SaveAndSwitch() {
        // check for errors before saving!
        if (SaveWarrior()) {
            LoadWarriorToWhiteboard(warriorToLoadIndex, warriorToLoadThumbnailIndex, true, isLoadingWarriorEnemy);
        } else {
            Debug.Log("saving error! couldn't switch");
        }
    }

    public void NoSaveSwitch() {
        // error check before switching
        List<string> errorsList = CheckSaveErrors(CreateDataForCurrentWarrior());
        // if no errors, load warrior
        if (errorsList.Count == 0) {
            LoadWarriorToWhiteboard(warriorToLoadIndex, warriorToLoadThumbnailIndex, true, isLoadingWarriorEnemy);
        } else { // otherwise show errors
            ShowErrorDisplay(errorsList);
        }
    }

    public void SaveAndContinue(string sceneName) {
        // attempt to save and load scene
        if (SaveWarrior()) {
            SceneController.Instance.LoadSceneByName(sceneName);
        } else { // if save unsuccessful, will stay here
            Debug.Log("saving error! couldn't load scene");
        }
    }

    public void NoSaveContinue(string sceneName) {
        // error check before continuing
        List<string> errorsList = CheckSaveErrors(CreateDataForCurrentWarrior());
        // if no errors, load scene
        if (errorsList.Count == 0) {
            SceneController.Instance.LoadSceneByName(sceneName);
        } else { // otherwise show errors
            ShowErrorDisplay(errorsList);
        }
    }

    // wrapper for button to save
    public void SaveWarriorWrapper() {
        if (SaveWarrior()) {
            // play sound on successful save
            AudioController.Instance.PlaySoundEffect("Save");
        }
    }

    // close error popup
    public void CloseErrorPopup() {
        errorPopupMenu.SetActive(false);
    }

    // snapping
    public void ToggleSnappingIndicator(GameObject overlapBlock, RectTransform blockRectTransform) {
        // show overlap indicator if over overlap space
        if ((overlapBlock != null) && !overlapSpaceIndicator.activeSelf) {
            // move indicator to correct position and show
            overlapSpaceIndicator.GetComponent<RectTransform>().sizeDelta = new Vector2 (blockRectTransform.sizeDelta.x + 20, blockRectTransform.sizeDelta.y + 20);
            overlapSpaceIndicator.transform.position = overlapBlock.transform.position - overlapBlock.GetComponent<Draggable>().blockOffset;
            overlapSpaceIndicator.SetActive(true);
        } else if ((overlapBlock == null) && overlapSpaceIndicator.activeSelf) { // hide overlap indicator if can't snap
            overlapSpaceIndicator.SetActive(false);
        }
    }

    // return the block that a currently dragged block should snap to
    // returns false if not snapping
    public GameObject FindBlockToSnapTo(PointerEventData eventData, Transform parent) {
        // loop through all objects under mouse
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        for (int i = 0; i < raycastResults.Count; i++) {
            Transform overlapParent = raycastResults[i].gameObject.transform.parent;
            if (overlapParent != parent && raycastResults[i].gameObject.tag == "overlapSpace" && overlapParent.gameObject.GetComponent<Draggable>()) {
                if (overlapParent.GetComponent<Draggable>().GetNextBlock() == null) {
                    // return found overlap space if it can be snapped to
                    return overlapParent.gameObject;
                }
            }
        }
        return null; // else return null
    }


    // WARRIOR CREATION
    public void PromptCreateNewWarrior() {
        // skip save prompt if just saved and no errors
        if (justSaved && CheckSaveErrors(CreateDataForCurrentWarrior()).Count == 0) {
            CreateNewWarrior();
            return;
        }

        // shown save prompt, update buttons to hold matchinf functions
        ToggleSavePromptMenu(true, false);
        GameObject saveContinueButton = switchPromptMenu.transform.GetChild(0).gameObject;
        GameObject dontSaveContinueButton = switchPromptMenu.transform.GetChild(2).gameObject;

        saveContinueButton.GetComponent<Button>().onClick.RemoveAllListeners();
        dontSaveContinueButton.GetComponent<Button>().onClick.RemoveAllListeners();

        saveContinueButton.GetComponent<Button>().onClick.AddListener(delegate {SaveAndCreate();});
        saveContinueButton.GetComponent<Button>().onClick.AddListener(delegate {CancelSwitch();});
        saveContinueButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Save & Create";

        dontSaveContinueButton.GetComponent<Button>().onClick.AddListener(delegate {NoSaveCreate();});
        dontSaveContinueButton.GetComponent<Button>().onClick.AddListener(delegate {CancelSwitch();});
        dontSaveContinueButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Create Without Saving";
    }

    public void SaveAndCreate() {
        // on successful save, create new warrior
        if (SaveWarrior()) {
            CreateNewWarrior();
        } else { // otherwise don't create (errors shown in save warrior step)
            Debug.Log("save error! couldn't create new warrior");
        }
    }

    public void NoSaveCreate() {
        // check for errors
        List<string> errorsList = CheckSaveErrors(CreateDataForCurrentWarrior());
        // if no errors, create new warrior
        if (errorsList.Count == 0) {
            CreateNewWarrior();
        } else { // otherwise show errors
            ShowErrorDisplay(errorsList);
        }
    }

    public void CreateNewWarrior() {
        // save active warrior first, just in case
        SaveWarrior();
        // get index
        editingWarriorIndex = WarriorListController.Instance.GetCount();
        // show delete button if making more than one
        if (WarriorListController.Instance.GetCount() <= 1) {
            deleteButton.SetActive(true);
        }
        // set team to player
        isCurrentWarriorEnemy = false;
        // reset name display
        GameObject.Find("NamePreview").GetComponent<TMP_Text>().text = "[noname],";
        // reset dropdown
        dropdown.gameObject.SetActive(true);
        dropdown.ResetSprite();
        // clear whiteboard
        ClearWhiteboard();
        // save new warrior to list
        InitializeWarrior();
    }

    public void ClearWhiteboard() {
        // destroy all non-header blocks on whiteboard
        GameObject whiteboard = GameObject.FindGameObjectWithTag("whiteboard");
        List<GameObject> allBlocks = new List<GameObject>();
        foreach (Transform child in whiteboard.transform) {
            if (!child.GetComponent<Draggable>().isHeader) {
                Destroy(child.gameObject);
            }
        }
        // reset next blocks on headers
        List<Draggable> headerDraggables = new List<Draggable> {propertiesHeaderObject.GetComponent<Draggable>(), moveHeaderObject.GetComponent<Draggable>(), useWeaponHeaderObject.GetComponent<Draggable>(), useSpecialHeaderObject.GetComponent<Draggable>()};
        foreach (Draggable headerDraggable in headerDraggables) {
            headerDraggable.SetNextBlock(null);
            headerDraggable.SetOverlapUseable();
        }
    }

    public void AddWarriorToDrawer(int index) {
        // add new thumbnail to drawer for new warrior
        Transform container = warriorDrawer.transform.GetChild(0).transform.GetChild(0);
        GameObject warriorThumbnail = Instantiate(warriorThumbnailPrefab, container);
        warriorThumbnail.GetComponent<WarriorEditorThumbnail>().isEnemy = false;
        UpdateWarriorDrawerThumbnail(index);
    }

    public void RemoveAllWarriorsFromDrawer() {
        // loop through backwards to remove each warrior
        int count = WarriorListController.Instance.GetCount() + 1;
        for (int i = count; i > 0; i--) {
            Destroy(warriorDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(i).gameObject);
        }
    }

    public void DebugGetThumbnailData() {
        // DEBUG
        // NOT IN USE
        // get data of all thumbnails in warrior drawer
        for (int i = 1; i < WarriorListController.Instance.GetCount() + 1; i++) {
            GameObject thumbnail = warriorDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(i).gameObject;
            Debug.Log("index " + i + ": setting " + thumbnail.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text + " to sprite " + thumbnail.GetComponent<Image>().sprite);
        }
    }

    public void LoadWarriorDrawer() { // loop through all warriors when scene is loaded
        for (int i=0; i < WarriorListController.Instance.GetCount(); i++) {
            AddWarriorToDrawer(i);
        }
        // if more than one warrior total, show delete button
        if (WarriorListController.Instance.GetCount() <= 1) {
            deleteButton.SetActive(false);
        }
    }

    public void UpdateWarriorDrawerThumbnail(int index) {
        // get references
        Transform container = warriorDrawer.transform.GetChild(0).transform.GetChild(0);
        WarriorFunctionalityData warrior = WarriorListController.Instance.GetWarriorAtIndex(index);
        // update sprite
        GameObject thumbnail = container.GetChild(index+1).gameObject;
        // Debug.Log("thumbnail is " + thumbnail);
        thumbnail.GetComponent<Image>().sprite = WarriorListController.Instance.spriteDataList[warrior.spriteIndex].sprite;
        // update list reference
        thumbnail.GetComponent<WarriorEditorThumbnail>().warriorIndex = index;
        // update name
        thumbnail.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = warrior.warriorName;
    }

    public void LoadEnemyDrawer() { // loop through all warriors when scene is loaded
        // if in the sandbox, load all enemies
        if (isSandbox) {
            for (int i=0; i < EnemyListController.Instance.GetCount(); i++) {
                AddEnemyToDrawer(i, i);
            }
        } else { // otherwise only load specific enemies for current level
            int level = ProgressionController.Instance.currentLevel;
            for (int i = 0; i < ProgressionController.Instance.levelDataList[level].availableEnemyIndices.Count; i++) {
                AddEnemyToDrawer(ProgressionController.Instance.levelDataList[level].availableEnemyIndices[i], i);
            }
        }
    }

    public void AddEnemyToDrawer(int warriorIndex, int thumbnailIndex) {
        // add a new thumbanil to drawer for new enemy
        Transform container = enemiesDrawer.transform.GetChild(0).transform.GetChild(0);
        GameObject enemyThumbnail = Instantiate(warriorThumbnailPrefab, container);
        enemyThumbnail.GetComponent<WarriorEditorThumbnail>().isEnemy = true;
        UpdateEnemyDrawerThumbnail(warriorIndex, thumbnailIndex);
    }

    public void UpdateEnemyDrawerThumbnail(int warriorIndex, int thumbnailIndex) {
        // get references
        Transform container = enemiesDrawer.transform.GetChild(0).transform.GetChild(0);
        WarriorFunctionalityData enemy = EnemyListController.Instance.GetWarriorAtIndex(warriorIndex);
        // update sprite
        GameObject thumbnail = container.GetChild(thumbnailIndex).gameObject;
        thumbnail.GetComponent<Image>().sprite = EnemyListController.Instance.spriteDataList[enemy.spriteIndex].sprite;
        // update list reference
        thumbnail.GetComponent<WarriorEditorThumbnail>().warriorIndex = warriorIndex;
        thumbnail.GetComponent<WarriorEditorThumbnail>().thumbnailIndex = thumbnailIndex;
        // update name
        thumbnail.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = enemy.warriorName;
    }

    public void InitializeWarrior() {
        // create new data for new warrior
        justSaved = true;
        WarriorFunctionalityData _WarriorFunctionalityData = CreateDataForCurrentWarrior();

        // set default header positions and load them to whiteboard
        _WarriorFunctionalityData.SetHeaderPositions();
        LoadDefaultHeaderPositions(_WarriorFunctionalityData);
        
        // this should only be called for player creating new warriors! (not enemies)
        // add new warrior to list and drawer
        UpdateWarriorList(_WarriorFunctionalityData, false);
        AddWarriorToDrawer(editingWarriorIndex);
    }

    private void LoadDefaultHeaderPositions(WarriorFunctionalityData warriorData) {
        // position header objects where they need to go
        propertiesHeaderObject.GetComponent<RectTransform>().anchoredPosition = warriorData.propertiesHeaderPosition;
        useWeaponHeaderObject.GetComponent<RectTransform>().anchoredPosition = warriorData.useWeaponHeaderPosition;

        if (!HelperController.Instance.GetCurrentLevelData().isMoveHeaderAvailable) {
            moveHeaderObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2100, -950);
        }
        // special header cut for scope! always load it to default far away
        useSpecialHeaderObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2100, -950);

        // if special was in use at all, we would load positions like this:
        // if (!HelperController.Instance.GetCurrentLevelData().isUseSpecialHeaderAvailable) {
        //     useSpecialHeaderObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2100, -950);
        // }
    }

    private WarriorFunctionalityData CreateDataForCurrentWarrior() {
        // create new warrior functionality data object
        WarriorFunctionalityData _WarriorFunctionalityData = new WarriorFunctionalityData(editingWarriorIndex);
        // store sprite data
        _WarriorFunctionalityData.spriteIndex = spriteDataIndex;
        // store name data
        _WarriorFunctionalityData.warriorName = ParseName();
        // store properties into list
        _WarriorFunctionalityData.properties = ParseProperties();
        // store behaviors into lists
        _WarriorFunctionalityData.moveFunctions = ParseBehaviors(moveHeaderObject);
        _WarriorFunctionalityData.useWeaponFunctions = ParseBehaviors(useWeaponHeaderObject);
        _WarriorFunctionalityData.useSpecialFunctions = ParseBehaviors(useSpecialHeaderObject);
        // calculate strength and behaviors
        _WarriorFunctionalityData.warriorStrength = CalculateCurrentStrength();
        _WarriorFunctionalityData.behaviorCount = CountBehaviors();
        // store header positions
        _WarriorFunctionalityData.SetHeaderPositions(propertiesHeaderObject.GetComponent<RectTransform>().anchoredPosition, moveHeaderObject.GetComponent<RectTransform>().anchoredPosition, useWeaponHeaderObject.GetComponent<RectTransform>().anchoredPosition, useSpecialHeaderObject.GetComponent<RectTransform>().anchoredPosition);
        // return warrior data
        return _WarriorFunctionalityData;
    }

    // Saving
    public bool SaveWarrior() {
        // create new data for current warrior
        WarriorFunctionalityData _WarriorFunctionalityData = CreateDataForCurrentWarrior();

        // check for errors
        List<string> errorsList = CheckSaveErrors(_WarriorFunctionalityData);
        if (errorsList.Count == 0) { // if no errors
            justSaved = true;
            // update warrior list and save into json
            UpdateWarriorList(_WarriorFunctionalityData, isCurrentWarriorEnemy);
            // update thumbnails and reload json files for enemies and warriors
            if (!isCurrentWarriorEnemy) {
                UpdateWarriorDrawerThumbnail(editingWarriorIndex);
                if (isSandbox) {
                    WarriorListController.Instance.FindJSON("sandbox_warriors"); // reload json file
                } else {
                    WarriorListController.Instance.FindJSON("level_warriors");
                }
            } else {
                UpdateEnemyDrawerThumbnail(editingWarriorIndex, editingThumbnailIndex);
                EnemyListController.Instance.FindJSON();
            }
            return true;
        } else { // there are errors, don't save and show errors
            justSaved = false;
            ShowErrorDisplay(errorsList);
            return false;
        }
    }

    public void ShowErrorDisplay(List<string> errorsList) {
        // add all found errors in string to popup screen
        TMP_Text errorText = errorPopupMenu.transform.GetChild(1).GetComponent<TMP_Text>(); // text component
        string errorString = "Couldn't save due to the following errors:\n\n";
        foreach (string error in errorsList) {
            errorString += error + "\n";
        }
        errorText.text = errorString;
        // show error popup
        errorPopupMenu.SetActive(true);
    }

    public void UpdateWarriorList(WarriorFunctionalityData warriorFunctionalityData, bool isEnemy) {
        // add warrior data to warrior or enemy list
        if (!isEnemy) {
            WarriorListController.Instance.AddWarrior(warriorFunctionalityData.warriorIndex, warriorFunctionalityData);
        } else {
            EnemyListController.Instance.AddWarrior(warriorFunctionalityData.warriorIndex, warriorFunctionalityData);
        }
    }

    // Loading blocks
    public void InitializeBlocksDrawer() {
        // only need to do this if we're in a level
        if (isSandbox) {
            return;
        }

        // get references to blocks containers in drawer, reset
        Transform propertyBlocksContainer = blockDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0);
        ClearAllChildren(propertyBlocksContainer);
        Transform behaviorBlocksContainer = blockDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(1);
        ClearAllChildren(behaviorBlocksContainer);
        Transform functionalBlocksContainer = blockDrawer.transform.GetChild(0).transform.GetChild(0).transform.GetChild(2);
        ClearAllChildren(functionalBlocksContainer);

        // get list of level properties
        List<int> propertyIndices = ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].availablePropertiesIndices;
        // get list of level behaviors
        // need to process behaviors differently because functional and behaviors combined
        List<int> behaviorIndices = SplitBehaviorsAndFunctional(ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].availableBehaviorsIndices, true);
        List<int> functionalIndices = SplitBehaviorsAndFunctional(ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].availableBehaviorsIndices, false);

        // Add all available blocks to containers
        // add headers and spacers for each set of blocks
        // add additional spacers after set if needed
        GameObject propertiesSectionHeader = Instantiate(sectionHeader, propertyBlocksContainer);
        propertiesSectionHeader.GetComponent<TMP_Text>().text = "PROPERTIES:";
        Instantiate(spacer, propertyBlocksContainer);
        foreach (int index in propertyIndices) {
            Instantiate(propertyBlocks[index], propertyBlocksContainer);
        }

        GameObject behaviorsSectionHeader = Instantiate(sectionHeader, behaviorBlocksContainer);
        behaviorsSectionHeader.GetComponent<TMP_Text>().text = "BEHAVIORS:";
        Instantiate(spacer, behaviorBlocksContainer);
        foreach (int index in behaviorIndices) {
            Instantiate(behaviorBlocks[index], behaviorBlocksContainer);
        }

         // early levels don't have functional, hide from player
        if (functionalIndices.Count != 0 ) {
            GameObject functionalSectionHeader = Instantiate(sectionHeader, functionalBlocksContainer);
            functionalSectionHeader.GetComponent<TMP_Text>().text = "FUNCTIONAL:";
            Instantiate(spacer, functionalBlocksContainer);
            foreach (int index in functionalIndices) {
                Instantiate(behaviorBlocks[index], functionalBlocksContainer);
            }
        }

    }

    public void ClearAllChildren(Transform parent) {
        // helper function to destory all children from parent
        foreach (Transform child in parent.transform) {
            Destroy(child.gameObject);
        }
    }

    public List<int> SplitBehaviorsAndFunctional(List<int> combinedList, bool isBehaviors) {

        // split a behavior list into behavior and functional blocks by indices
        List<int> functionalIndices = new List<int> {7, 8, 9, 10, 11, 12, 16, 18};
        List<int> behaviorIndices = new List<int> {1, 2, 3, 4, 5, 6, 13, 14, 15, 17};

        List<int> splitList = new();

        // add all from chosen list to split list
        if (isBehaviors) { // behaviors
            for (int i = 0; i < combinedList.Count; i++) {
                if (behaviorIndices.Contains(combinedList[i])) {
                    splitList.Add(combinedList[i]);
                }
            }
        } else { // functional
            for (int i = 0; i < combinedList.Count; i++) {
                if (functionalIndices.Contains(combinedList[i])) {
                    splitList.Add(combinedList[i]);
                }
            }
        }
        return splitList;
    }

    // Error Checking
    public List<string> CheckSaveErrors(WarriorFunctionalityData warriorFunctionalityData) {
        // return list of strings, each entry representing an error
        // if no errors, count is 0

        List<string> errorsToOutput = new List<string>();
        // THINGS TO CHECK:
        // must have name
        if (ParseName() == "[noname]") {
            errorsToOutput.Add("missing name! who is it??");
        }
        // must have health
        if (!CheckForHealth()) {
            errorsToOutput.Add("no health! warrior will die immediately :(");
        }

        // no missing targets
        if (!CheckTargetAssigned(warriorFunctionalityData)) {
            errorsToOutput.Add("missing target! don't forget to assign a target!");
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
    public void LoadWarriorToWhiteboard(int indexToLoad, int thumbnailIndex, bool noSave, bool isLoadingEnemy) {
        // might need to save first
        if (!noSave) {
            // save previous warrior
            SaveWarrior();
        }

        // reset whiteboard
        ClearWhiteboard();
        // update index and load sprite
        editingWarriorIndex = indexToLoad;
        editingThumbnailIndex = thumbnailIndex;

        // get data for warrior from list and load sprite
        WarriorFunctionalityData warriorData = new WarriorFunctionalityData();
        if (!isLoadingEnemy) { // WARRIOR
            warriorData = WarriorListController.Instance.GetWarriorAtIndex(indexToLoad);
        } else { // ENEMY
            warriorData = EnemyListController.Instance.GetWarriorAtIndex(indexToLoad);
        }

        // update sprite selector dropdown based on newly loaded warrior
        if (!dropdown.gameObject.activeSelf) {
            dropdown.gameObject.SetActive(true);
        }
        dropdown.UpdateSprite(warriorData.spriteIndex, isLoadingEnemy);
        if (isLoadingEnemy) {
            // hide editing options (save, dropdown) for enemies
            textThe.SetActive(false);
            saveButton.SetActive(false);
            deleteButton.SetActive(false);
            dropdown.gameObject.SetActive(false);
        } else {
            // show editing options for warriors
            textThe.SetActive(true);
            saveButton.SetActive(true);
            if (WarriorListController.Instance.GetCount() > 1) {
                deleteButton.SetActive(true);
            }
        }

        // instantiate blocks onto the whiteboard, positioned and parented
        // parent the objects and then run the update position function on each header
        // PROPERTIES
        propertiesHeaderObject.GetComponent<RectTransform>().anchoredPosition = warriorData.propertiesHeaderPosition;
        GameObject currentBlock = propertiesHeaderObject;
        // loop through all property block data
        foreach (BlockDataStruct block in warriorData.properties) {
            // instantiate block parented to whiteboard
            GameObject newBlock = Instantiate(propertyBlocks[(int)block.property], this.transform.position, this.transform.rotation, whiteboard.transform);
            // call initialize block draggable
            newBlock.GetComponent<BlockListItem>().InitializeBlockDraggable();
            // update block data
            newBlock.GetComponent<BlockData>().SetBlockDataValues(block.values);
            newBlock.GetComponent<Draggable>().SetMaskable(true);
            try {
                if (newBlock.GetComponent<BlockData>().property == BlockData.Property.NAME) {
                    newBlock.GetComponent<Draggable>().SetInputFieldValue(block.values[0]);
                } else {
                    newBlock.GetComponent<Draggable>().SetSliderValue(block.values[0]);
                }
            } catch (System.Exception) {
                Debug.Log("no value for current property");
            }
            // set current block.next to instantiated
            currentBlock.GetComponent<Draggable>().SetNextBlock(newBlock);
            newBlock.GetComponent<Draggable>().SetPrevBlock(currentBlock);

            // check if current block needs to adjust next block placement;
            BlockData.BehaviorType blockBehavior = newBlock.GetComponent<BlockData>().behavior;
            bool shiftOffsetBack = blockBehavior == BlockData.BehaviorType.END_LOOP || blockBehavior == BlockData.BehaviorType.END_FOR || blockBehavior == BlockData.BehaviorType.END_IF || blockBehavior == BlockData.BehaviorType.ELSE;
            if (shiftOffsetBack && newBlock.GetComponent<Draggable>().GetPrevBlock() != null && newBlock.GetComponent<Draggable>().GetPrevBlock().GetComponent<BlockData>().blockType != BlockData.BlockType.HEADER) {
                newBlock.GetComponent<Draggable>().GetPrevBlock().GetComponent<Draggable>().SetBlockOffset(true);
            }
            
            // update current block
            currentBlock = newBlock;
        }
        propertiesHeaderObject.GetComponent<Draggable>().UpdateBlockPositions(propertiesHeaderObject, propertiesHeaderObject.transform.position);

        
        // special header cut for scope! always load it to default far away
        useSpecialHeaderObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2100, -950);

        // if special was in use at all, we would load positions like this:
        // if (!HelperController.Instance.GetCurrentLevelData().isUseSpecialHeaderAvailable) {
        //     useSpecialHeaderObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2100, -950);
        // }

        // CONDENSE BEHAVIOR LOADING
        // loop through all behavior headers
        List<GameObject> headers = new List<GameObject> {moveHeaderObject, useWeaponHeaderObject, useSpecialHeaderObject};
        List<List<BlockDataStruct>> behaviorLists = new List<List<BlockDataStruct>> {warriorData.moveFunctions, warriorData.useWeaponFunctions, warriorData.useSpecialFunctions};
        for (int i = 0; i < headers.Count; i++) {
            // place headers
            // also hide headers if not needed for level
            switch (i) {
                case 0:
                    headers[i].GetComponent<RectTransform>().anchoredPosition = warriorData.moveHeaderPosition;
                    if (!HelperController.Instance.GetCurrentLevelData().isMoveHeaderAvailable || (isLoadingEnemy && warriorData.moveFunctions.Count == 0)) {
                        moveHeaderObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2100, -950);
                    } else if (moveHeaderObject.GetComponent<RectTransform>().anchoredPosition == new Vector2(-2100, -950)) {
                        moveHeaderObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-186, 895);
                    }
                    break;
                case 1:
                    headers[i].GetComponent<RectTransform>().anchoredPosition = warriorData.useWeaponHeaderPosition;
                    break;
                case 2:
                    headers[i].GetComponent<RectTransform>().anchoredPosition = warriorData.useSpecialHeaderPosition;

                    // special header cut for scope! always load it to default far away
                    useSpecialHeaderObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2100, -950);

                    // if special was in use at all, we would load positions like this:
                    // if (!HelperController.Instance.GetCurrentLevelData().isUseSpecialHeaderAvailable || (isLoadingEnemy && warriorData.useSpecialFunctions.Count == 0)) {
                    //     useSpecialHeaderObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-2100, -950);
                    // }  else if (useSpecialHeaderObject.GetComponent<RectTransform>().anchoredPosition == new Vector2(-2100, -950)) {
                    //     useSpecialHeaderObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-186, 441);
                    // }
                    break;
            }

            // place blocks
            currentBlock = headers[i];
            // loop through all blocks in each behavior list
            foreach (BlockDataStruct block in behaviorLists[i]) {
                // instantiate block parented to whiteboard
                GameObject newBlock = Instantiate(behaviorBlocks[(int)block.behavior], this.transform.position, this.transform.rotation, whiteboard.transform);
                // call initialize block draggable
                newBlock.GetComponent<BlockListItem>().InitializeBlockDraggable();
                // update block data
                newBlock.GetComponent<BlockData>().SetBlockDataValues(block.values);
                newBlock.GetComponent<Draggable>().SetMaskable(true);
                switch (block.behavior) {
                    // no dropdowns:
                    case BlockData.BehaviorType.MELEE_ATTACK:
                    case BlockData.BehaviorType.FIRE_PROJECTILE:
                    case BlockData.BehaviorType.ELSE:
                    case BlockData.BehaviorType.END_IF:
                    case BlockData.BehaviorType.END_LOOP:
                    case BlockData.BehaviorType.END_FOR:
                        break;
                    // one dropdown:
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
                    // two dropdowns:
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
                    // three dropdowns (NONE CURRENTLY):
                    // input field:
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
                bool shiftOffsetBack = blockBehavior == BlockData.BehaviorType.END_LOOP || blockBehavior == BlockData.BehaviorType.END_FOR || blockBehavior == BlockData.BehaviorType.END_IF || blockBehavior == BlockData.BehaviorType.ELSE;
                if (shiftOffsetBack && newBlock.GetComponent<Draggable>().GetPrevBlock() != null && newBlock.GetComponent<Draggable>().GetPrevBlock().GetComponent<BlockData>().blockType != BlockData.BlockType.HEADER) {
                    newBlock.GetComponent<Draggable>().GetPrevBlock().GetComponent<Draggable>().SetBlockOffset(true);
                }

                // iterate to next block
                currentBlock = newBlock;
            }
            headers[i].GetComponent<Draggable>().UpdateBlockPositions(headers[i], headers[i].transform.position);
        }

        // if editing a warrior, set as last edited
        isCurrentWarriorEnemy = isLoadingWarriorEnemy;
        if (!isCurrentWarriorEnemy) {
            ProgressionController.Instance.SetLastEditedWarrior(editingWarriorIndex);
        }
        // update strenght and behaviors
        UpdateStrengthDisplay();
        UpdateBehaviorsDisplay();

        // prevent unnecessary overlapping collisions
        DisableOverlapSpaces();
        // save warrior at end to make sure values are properly updated
        SaveWarrior();
    }

    public void DisableOverlapSpaces() {
        // loop through all blocks on whiteboard, disable overlap
        foreach (Transform child in whiteboard.transform) {
            if (child.GetComponent<Draggable>()) {
                child.GetComponent<Draggable>().SetOverlapUseable(false);
            }
        }
    }

    public void EnableOverlapSpaces() {
        // loop through all blocks on whiteboard
        // enable overlap if they can be overlapped
        foreach (Transform child in whiteboard.transform) {
            if (child.GetComponent<Draggable>()) {
                child.GetComponent<Draggable>().SetOverlapUseable();
            }
        }
    }


    public string ParseName() {
        // set default name
        string name = "[noname]";
        // loop through all blocks under properties header
        GameObject current = propertiesHeaderObject.GetComponent<Draggable>().GetNextBlock();
        while (current != null) {
            BlockData blockData = current.GetComponent<BlockData>();
            if (blockData.blockType == BlockData.BlockType.PROPERTY) {
                if (blockData.property == BlockData.Property.NAME && (blockData.values.Count != 0)) {
                    // if name block found, update display to show name
                    name = blockData.values[0];
                    GameObject.Find("NamePreview").GetComponent<TMP_Text>().text = blockData.values[0];
                    if (!isCurrentWarriorEnemy) {
                        // if not enemy, add comma for class title
                        GameObject.Find("NamePreview").GetComponent<TMP_Text>().text += ",";
                    }
                }
            }
            // continue iterating
            current = current.GetComponent<Draggable>().GetNextBlock();
        }
        return name;
    }

    public bool CheckForHealth() {
        // loop through all blocks under properties header
        // return true if health property found
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
        // loop through all blocks under properties header
        List<BlockDataStruct> propertiesList = new List<BlockDataStruct>();
        GameObject current = propertiesHeaderObject.GetComponent<Draggable>().GetNextBlock();
        while (current != null) {
            BlockData blockData = current.GetComponent<BlockData>();
            if (blockData.blockType == BlockData.BlockType.PROPERTY) {
                // convert block data to stuct, add to list
                BlockDataStruct blockDataStruct = blockData.ConvertToStruct();
                propertiesList.Add(blockDataStruct);
                if (blockData.property == BlockData.Property.NAME) {
                    // update name if name property found / needed
                    GameObject.Find("NamePreview").GetComponent<TMP_Text>().text = (blockData.values.Count != 0) ? blockData.values[0]  : "[noname]";
                    if (!isCurrentWarriorEnemy) {
                        GameObject.Find("NamePreview").GetComponent<TMP_Text>().text += ",";
                    }
                }
            }
            // continue iterating
            current = current.GetComponent<Draggable>().GetNextBlock();
        }
        // return list of all properties data
        return propertiesList;
    }

    public int CalculateCurrentStrength() {
        List<BlockDataStruct> propertiesList = ParseProperties();

        // FORMULA:
        // strength = attack*(range/2) + heal*(range/2) + projectilePower + maxHealth*(defense/(defense+maxHealth+1)) + speed/10

        // set default values
        int attackPower = 0;
        int attackRange = 1;
        int healPower = 0;
        int projectilePower = 0;
        int maxHealth = 1;
        int defense = 0;
        int speed = 0;

        // loop through properties to get actual values
        foreach (BlockDataStruct block in propertiesList) {
            switch (block.property) {
                case BlockData.Property.MELEE_ATTACK_POWER:
                    attackPower = int.Parse(block.values[0]);
                    break;
                case BlockData.Property.MELEE_ATTACK_RANGE:
                    attackRange = int.Parse(block.values[0]);
                    break;
                case BlockData.Property.HEAL_POWER:
                    healPower = int.Parse(block.values[0]);
                    break;
                case BlockData.Property.RANGED_ATTACK_POWER:
                    projectilePower = int.Parse(block.values[0]);
                    break;
                case BlockData.Property.HEALTH:
                    maxHealth = int.Parse(block.values[0]);
                    break;
                case BlockData.Property.DEFENSE:
                    defense = int.Parse(block.values[0]);
                    break;
                case BlockData.Property.MOVE_SPEED:
                    speed = int.Parse(block.values[0]);
                    break;
            }
        }
        // calculate strength using actual and default values
        return HelperController.Instance.CalculateWarriorStrength(attackPower, attackRange, healPower, projectilePower, speed, maxHealth, defense);
    }

    public List<BlockDataStruct> ParseBehaviors(GameObject header) {
        // loop through all behavior blocks under header
        List<BlockDataStruct> behaviorsList = new List<BlockDataStruct>();
        GameObject current = header.GetComponent<Draggable>().GetNextBlock();
        while (current != null) {
            // get current block data
            BlockData blockData = current.GetComponent<BlockData>();
            if (blockData.blockType == BlockData.BlockType.BEHAVIOR || blockData.blockType == BlockData.BlockType.FUNCTION) {
                // set default behavior values into matching value slots for each block type
                switch (blockData.behavior) {
                    // no dropdowns:
                    case BlockData.BehaviorType.MELEE_ATTACK:
                    case BlockData.BehaviorType.FIRE_PROJECTILE:
                    case BlockData.BehaviorType.ELSE:
                        if (blockData.values.Count != 2) {
                            blockData.values.Clear();
                            blockData.values.Add((-1).ToString());
                            blockData.values.Add((-1).ToString());
                        } else {
                            blockData.values[0] = (-1).ToString();
                            blockData.values[1] = (-1).ToString();
                        }
                        break;
                    case BlockData.BehaviorType.END_IF:
                        if (blockData.values.Count != 1) {
                            blockData.values.Clear();
                            blockData.values.Add((-1).ToString());
                        } else {
                            blockData.values[0] = (-1).ToString();
                        }
                        break;
                    case BlockData.BehaviorType.END_LOOP:
                    case BlockData.BehaviorType.END_FOR:
                        if (blockData.values.Count != 1) {
                            blockData.values.Clear();
                            blockData.values.Add((-1).ToString());
                        } else {
                            blockData.values[0] = (-1).ToString();
                        }
                        break;
                    // one dropdown, store value as string:
                    case BlockData.BehaviorType.TURN:
                    case BlockData.BehaviorType.STEP:
                    case BlockData.BehaviorType.RUN:
                    case BlockData.BehaviorType.TELEPORT:
                        if (blockData.values.Count != 1){
                            blockData.values.Clear();
                            blockData.values.Add(current.transform.GetChild(2).gameObject.GetComponent<TMP_Dropdown>().value.ToString());
                        } else {
                            blockData.values[0] = current.transform.GetChild(2).gameObject.GetComponent<TMP_Dropdown>().value.ToString();
                        }
                        break;
                    // two dropdowns:
                    case BlockData.BehaviorType.SET_TARGET:
                    case BlockData.BehaviorType.MELEE_SETTINGS:
                    case BlockData.BehaviorType.RANGED_SETTINGS:
                    case BlockData.BehaviorType.WHILE_LOOP:
                    case BlockData.BehaviorType.IF:
                        if (blockData.values.Count != 2) {
                            blockData.values.Clear();
                            blockData.values.Add(current.transform.GetChild(2).gameObject.GetComponent<TMP_Dropdown>().value.ToString());
                            blockData.values.Add(current.transform.GetChild(3).gameObject.GetComponent<TMP_Dropdown>().value.ToString());
                        } else {
                            blockData.values[0] = current.transform.GetChild(2).gameObject.GetComponent<TMP_Dropdown>().value.ToString();
                            blockData.values[1] = current.transform.GetChild(3).gameObject.GetComponent<TMP_Dropdown>().value.ToString();
                        }
                        break;
                    // three dropdowns: (NONE CURRENTLY)
                    // input field:
                    case BlockData.BehaviorType.FOR_LOOP:
                        if (blockData.values.Count != 1) {
                            blockData.values.Clear();
                            blockData.values.Add(current.transform.GetChild(2).gameObject.GetComponent<TMP_InputField>().text);
                        } else {
                            blockData.values[0] = current.transform.GetChild(2).gameObject.GetComponent<TMP_InputField>().text;
                        }
                        if (blockData.values[0] == "") {
                            blockData.values[0] = "0";
                        }
                        break;
                }
                // convert block data to struct, add to list of all behavior structs
                BlockDataStruct blockDataStruct = blockData.ConvertToStruct();
                behaviorsList.Add(blockDataStruct);
            }
            // continue iterating
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

                    if (behaviorsList[j].behavior == BlockData.BehaviorType.END_FOR) {
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
                // jump point is values [2]
                // storing jump point to end in else [0] if it exists

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
                    } else if (behaviorsList[j].behavior == BlockData.BehaviorType.END_LOOP || behaviorsList[j].behavior == BlockData.BehaviorType.END_FOR) {
                        inLoop = false;
                    }

                    if (behaviorsList[j].behavior == BlockData.BehaviorType.ELSE && inLoop == false) {
                        if (ignores == 0 && elseIndex == -1) {
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
                            } else {
                                behaviorsList[elseIndex].values[1] = j.ToString();
                            }
                            // assign jump point to if for error checking
                            behaviorsList[j].values[0] = i.ToString();
                            break;
                        }
                    }
                }
            }
        }

        // return final list of behaviors with set values
        return behaviorsList;
    }

    public int CountBehaviors() {
        // loop through all headers, count all behaviors in their lists that are not functional blocks
        int count = 0;
        List<GameObject> allHeaders = new() {moveHeaderObject, useWeaponHeaderObject, useSpecialHeaderObject};
        foreach (GameObject header in allHeaders) {
            GameObject current = header.GetComponent<Draggable>().GetNextBlock();
            while (current != null) {
                // Debug.Log("looking at " + current.gameObject.name);
                BlockData blockData = current.GetComponent<BlockData>();
                if (blockData.blockType == BlockData.BlockType.BEHAVIOR) {
                    if (blockData.behavior != BlockData.BehaviorType.IF && blockData.behavior != BlockData.BehaviorType.ELSE && blockData.behavior != BlockData.BehaviorType.END_IF && blockData.behavior != BlockData.BehaviorType.WHILE_LOOP && blockData.behavior != BlockData.BehaviorType.FOR_LOOP && blockData.behavior != BlockData.BehaviorType.END_LOOP && blockData.behavior != BlockData.BehaviorType.END_FOR) {
                        count++;
                    }
                }
                current = current.GetComponent<Draggable>().GetNextBlock();
            }
        }
        return count;
    }

    public List<string> CheckLoopingConditionalErrors(string headerName, List<BlockDataStruct> blocks) {
        // create list of errors
        List<string> errorsList = new List<string>();

        // loop through all block data, found error if any values hold -1 (mismatched)
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
                        errorsList.Add("mismatched END WHILE");
                    }
                    break;
                case BlockData.BehaviorType.END_FOR:
                    if (block.values[0] == "-1") {
                        errorsList.Add("mismatched END FOR");
                    }
                    break;
                case BlockData.BehaviorType.IF:
                    if (block.values[2] == "-1") {
                        errorsList.Add("mismatched IF statement!");
                    }
                    break;
                case BlockData.BehaviorType.ELSE:
                    if (block.values[0] == "-1" || block.values[1] == "-1") { // handle missing if and endif
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

    public bool CheckTargetAssigned(WarriorFunctionalityData warriorFunctionalityData) {
        // loop through all behavior lists
        // first, if find target, return true
        // then, if find need target but target not assigned, return false
        // end, return true by default
        List<List<BlockDataStruct>> behaviorLists = new List<List<BlockDataStruct>> {warriorFunctionalityData.moveFunctions, warriorFunctionalityData.useWeaponFunctions, warriorFunctionalityData.useSpecialFunctions};
        foreach (List<BlockDataStruct> behaviorList in behaviorLists) {
            foreach (BlockDataStruct block in behaviorList) {
                if (block.behavior == BlockData.BehaviorType.SET_TARGET) {
                    return true;
                } else if (block.behavior == BlockData.BehaviorType.TELEPORT || block.behavior == BlockData.BehaviorType.FIRE_PROJECTILE || block.behavior == BlockData.BehaviorType.IF || block.behavior == BlockData.BehaviorType.WHILE_LOOP) {
                    // we only hit this case if we haven't already found a target
                    return false;
                }
            }
        }
        return true;
    }

    public bool CheckUnderBehaviorLimit(WarriorFunctionalityData warriorFunctionalityData) {
        // DEPRECATED
        // old behavior counting method
        int count = 0;
        List<List<BlockDataStruct>> behaviorLists = new List<List<BlockDataStruct>> {warriorFunctionalityData.moveFunctions, warriorFunctionalityData.useWeaponFunctions, warriorFunctionalityData.useSpecialFunctions};
        List<int> behaviorIndices = new List<int> {1, 2, 3, 4, 5, 6, 13, 14, 15};
        foreach (List<BlockDataStruct> behaviorList in behaviorLists) {
            foreach (BlockDataStruct block in behaviorList) {
                if (behaviorIndices.Contains((int)block.behavior)) {
                    count += 1;
                }
            }
        }
        return count <= ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxBlocks;
    }

    public void UpdateStrengthDisplay() {
        // calculate strength
        int newStrength = CalculateCurrentStrength();
        // show strength in green or red if it's valid or not, respectively
        // always valid in sandbox
        if (newStrength <= ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxTotalStrength || ProgressionController.Instance.currentLevel == 0) {
            strengthDisplay.color = new Color(104f/255f, 241f/255f, 104f/255f);
        } else {
            strengthDisplay.color = new Color(241f/255f, 104f/255f, 104f/255f);
        }

        // update display text
        strengthDisplay.text = "Strength\n" + newStrength + " / ";
        if (ProgressionController.Instance.currentLevel == 0) { // sandbox, infinite allowed
            strengthDisplay.text += "inf.";
        } else {
            strengthDisplay.text += ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxTotalStrength;
        }
    }

    public void UpdateBehaviorsDisplay() {
        // count behaviors
        int behaviorCount = CountBehaviors();
        // show count in green or red if it's valid or not, respectively
        // always valid in sandbox
        if (behaviorCount <= ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxBlocks || ProgressionController.Instance.currentLevel == 0) {
            maxBehaviorsDisplay.color = new Color(104f/255f, 241f/255f, 104f/255f);
        } else {
            maxBehaviorsDisplay.color = new Color(241f/255f, 104f/255f, 104f/255f);
        }

        // update display text
        maxBehaviorsDisplay.text = "Behaviors\n" + behaviorCount + " / ";
        if (ProgressionController.Instance.currentLevel == 0) { // sandbox, infinite allowed
            maxBehaviorsDisplay.text += "inf.";
        } else {
            maxBehaviorsDisplay.text += ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxBlocks;
        }
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

        // play sound
        AudioController.Instance.PlaySoundEffect("Delete");

        // delete, but delay to prevent reloading errors
        StartCoroutine(RemoveWarriorsDelay());
    }

    private IEnumerator RemoveWarriorsDelay() {
        // remove warrior from list
        // renumber indices within list
        WarriorListController.Instance.RemoveWarrior(editingWarriorIndex);
        yield return new WaitForSeconds(.01f);
        // clear warrior drawer
        RemoveAllWarriorsFromDrawer();
        yield return new WaitForSeconds(.01f);

        // load warrior at last index --> also clears whiteboard
            // if list now empty, create a new blank one and load it
        // reload warrior drawer
        if (WarriorListController.Instance.GetCount() == 0) {
            WarriorListController.Instance.AddWarrior(0, new WarriorFunctionalityData());
        }
        LoadWarriorDrawer();
        LoadWarriorToWhiteboard(editingWarriorIndex-1, editingWarriorIndex-1, true, false);

        // reload json file
        if (isSandbox) {
            WarriorListController.Instance.FindJSON("sandbox_warriors");
        } else {
            WarriorListController.Instance.UpdateJSON("level_warriors");
        }
        // if deletion, will have to reset grid
        ProgressionController.Instance.madeListEdits = true;
    }

}

