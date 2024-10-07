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

    [Header("SAVE/LOAD")]
    [SerializeField] private WarriorListController warriorListController; 

    [Header("REFERENCES")]
    [Header("Headers")]
    [SerializeField] private GameObject propertiesHeaderObject;
    [SerializeField] private GameObject moveHeaderObject;
    [SerializeField] private GameObject useWeaponHeaderObject;
    [SerializeField] private GameObject useSpecialHeaderObject;

    [Header("Objects")]
    [SerializeField] private GameObject blockDrawer;
    [SerializeField] private GameObject warriorDrawer;
    [SerializeField] private DropdownOptions dropdown;
    [SerializeField] private GameObject whiteboard;
    [SerializeField] private GameObject deleteMenu;
    
    [Header("Sprites")]
    [SerializeField] private GameObject warriorThumbnailPrefab;
    [SerializeField] public List<SpriteData> spriteDataList;
    [SerializeField] public int spriteDataIndex;

    [Header("Vars")]
    [Header("Property Blocks")]
    [SerializeField] private List<GameObject> propertyBlocks;
    [SerializeField] private List<GameObject> behaviorBlocks;
    [SerializeField] private int editingIndex;

    // INITIALIZING
    void Start() {
        if (warriorListController == null) {
            warriorListController = GameObject.Find("WarriorListPersistent").GetComponent<WarriorListController>();
        }
        LoadWarriorDrawer();
        LoadWarriorToWhiteboard(editingIndex, true);
    }


    // for buttons
    public void ShowBlockDrawer() {
        if (!blockDrawer.activeSelf) {
            blockDrawer.SetActive(true);
        }
        if (warriorDrawer.activeSelf) {
            warriorDrawer.SetActive(false);
        }
    }

    public void ShowWarriorDrawer() {
        if (!warriorDrawer.activeSelf) {
            warriorDrawer.SetActive(true);
        }
        if (blockDrawer.activeSelf) {
            blockDrawer.SetActive(false);
        }
    }

    // warrior creation
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
        Instantiate(warriorThumbnailPrefab, container);
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
        thumbnail.GetComponent<Image>().sprite = spriteDataList[warrior.spriteIndex].sprite;
        // update list reference
        thumbnail.GetComponent<WarriorEditorThumbnail>().warriorIndex = index;
        // update name
        thumbnail.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = warrior.warriorName;

        // Debug.Log("index " + index + ": setting " + thumbnail.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text + " to sprite " + warrior.spriteIndex);
    }

    public void InitializeWarrior() {
        WarriorFunctionalityData _WarriorFunctionalityData = new WarriorFunctionalityData(editingIndex);
        _WarriorFunctionalityData.spriteIndex = spriteDataIndex;
        _WarriorFunctionalityData.warriorName = ParseName();
        _WarriorFunctionalityData.properties = ParseProperties();
        _WarriorFunctionalityData.moveFunctions = ParseMove();
        _WarriorFunctionalityData.useWeaponFunctions = ParseUseWeapon();
        _WarriorFunctionalityData.useSpecialFunctions = ParseUseSpecial();
        // SaveIntoJSON(_WarriorFunctionalityData);
        UpdateWarriorList(_WarriorFunctionalityData);
        AddWarriorToDrawer(editingIndex);
    }


    // Saving
    public void SaveWarrior() {
        WarriorFunctionalityData _WarriorFunctionalityData = new WarriorFunctionalityData(editingIndex);
        _WarriorFunctionalityData.spriteIndex = spriteDataIndex;
        _WarriorFunctionalityData.warriorName = ParseName();
        _WarriorFunctionalityData.properties = ParseProperties();
        _WarriorFunctionalityData.moveFunctions = ParseMove();
        _WarriorFunctionalityData.useWeaponFunctions = ParseUseWeapon();
        _WarriorFunctionalityData.useSpecialFunctions = ParseUseSpecial();
        // SaveIntoJSON(_WarriorFunctionalityData);
        UpdateWarriorList(_WarriorFunctionalityData);
        UpdateWarriorDrawerThumbnail(editingIndex);
        warriorListController.FindJSON(); // reload json file
    }


    public void SaveIntoJSON(WarriorFunctionalityData warriorFunctionalityData) {
        string warriorPropertiesJSON = JsonUtility.ToJson(warriorFunctionalityData);
        string filePath = Application.persistentDataPath + $"/{warriorFunctionalityData.warriorName}.json";
        System.IO.File.WriteAllText(filePath, warriorPropertiesJSON);
        Debug.Log("saving json at " + filePath);
    }

    public void UpdateWarriorList(WarriorFunctionalityData warriorFunctionalityData) {
        warriorListController.AddWarrior(warriorFunctionalityData.warriorIndex, warriorFunctionalityData);
    }

    // Loading
    public void LoadWarriorToWhiteboard(int index, bool init) { // check if initializing
        if (!init) {
            // save previous warrior and clear whiteboard
            SaveWarrior();
        }
        ClearWhiteboard();
        // update index and load sprite
        editingIndex = index;
        // get data for warrior from list and load sprite
        WarriorFunctionalityData warriorData = warriorListController.GetWarriorAtIndex(index);
        dropdown.UpdateSprite(warriorData.spriteIndex);
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
            // try {
            //     newBlock.GetComponent<Draggable>().SetInputFieldValue(block.values[0]);
            // } catch (System.Exception) {
            //     Debug.Log("no value for current property");
            // }
            // set current block.next to instantiated
            currentBlock.GetComponent<Draggable>().SetNextBlock(newBlock);
            newBlock.GetComponent<Draggable>().SetPrevBlock(currentBlock);
            // update current block
            currentBlock = newBlock;
        }
        moveHeaderObject.GetComponent<Draggable>().UpdateBlockPositions(moveHeaderObject, moveHeaderObject.transform.position);

        // repeat for weapon
        currentBlock = useWeaponHeaderObject;
        foreach (BlockDataStruct block in warriorData.useWeaponFunctions) {
            Debug.Log(block.behavior + ": " + (int)block.behavior);
            // instantiate block parented to whiteboard
            GameObject newBlock = Instantiate(behaviorBlocks[(int)block.behavior], this.transform.position, this.transform.rotation, whiteboard.transform);
            // call initialize block draggable
            newBlock.GetComponent<BlockListItem>().InitializeBlockDraggable();
            // update block data
            newBlock.GetComponent<BlockData>().SetBlockDataValues(block.values);
            newBlock.GetComponent<Draggable>().SetMaskable(true);
            // try {
            //     newBlock.GetComponent<Draggable>().SetInputFieldValue(block.values[0]);
            // } catch (System.Exception) {
            //     Debug.Log("no value for current property");
            // }
            // set current block.next to instantiated
            currentBlock.GetComponent<Draggable>().SetNextBlock(newBlock);
            newBlock.GetComponent<Draggable>().SetPrevBlock(currentBlock);
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
            // try {
            //     newBlock.GetComponent<Draggable>().SetInputFieldValue(block.values[0]);
            // } catch (System.Exception) {
            //     Debug.Log("no value for current property");
            // }
            // set current block.next to instantiated
            currentBlock.GetComponent<Draggable>().SetNextBlock(newBlock);
            newBlock.GetComponent<Draggable>().SetPrevBlock(currentBlock);
            // update current block
            currentBlock = newBlock;
        }
        useSpecialHeaderObject.GetComponent<Draggable>().UpdateBlockPositions(useSpecialHeaderObject, useSpecialHeaderObject.transform.position);

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
                }
                GameObject.Find("NamePreview").GetComponent<TMP_Text>().text = blockData.values[0];
            }
            current = current.GetComponent<Draggable>().GetNextBlock();
        }
        return name;
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

    public List<BlockDataStruct> ParseMove() {
        List<BlockDataStruct> moveFunctions = new List<BlockDataStruct>();
        GameObject current = moveHeaderObject.GetComponent<Draggable>().GetNextBlock();
        while (current != null) {
            BlockData blockData = current.GetComponent<BlockData>();
            if (blockData.blockType == BlockData.BlockType.BEHAVIOR || blockData.blockType == BlockData.BlockType.FUNCTION) {
                BlockDataStruct blockDataStruct = blockData.ConvertToStruct();
                moveFunctions.Add(blockDataStruct);
                // Debug.Log("added move function");
            }
            current = current.GetComponent<Draggable>().GetNextBlock();
        }
        return moveFunctions;
    }

    public List<BlockDataStruct> ParseUseWeapon() {
        List<BlockDataStruct> useWeaponFunctions = new List<BlockDataStruct>();
        GameObject current = useWeaponHeaderObject.GetComponent<Draggable>().GetNextBlock();
        while (current != null) {
            BlockData blockData = current.GetComponent<BlockData>();
            if (blockData.blockType == BlockData.BlockType.BEHAVIOR || blockData.blockType == BlockData.BlockType.FUNCTION) {
                BlockDataStruct blockDataStruct = blockData.ConvertToStruct();
                useWeaponFunctions.Add(blockDataStruct);
                // Debug.Log("added use weapon function");
            }
            current = current.GetComponent<Draggable>().GetNextBlock();
        }
        return useWeaponFunctions;
    }

    public List<BlockDataStruct> ParseUseSpecial() {
        List<BlockDataStruct> useSpecialFunctions = new List<BlockDataStruct>();
        GameObject current = useSpecialHeaderObject.GetComponent<Draggable>().GetNextBlock();
        while (current != null) {
            BlockData blockData = current.GetComponent<BlockData>();
            if (blockData.blockType == BlockData.BlockType.BEHAVIOR || blockData.blockType == BlockData.BlockType.FUNCTION) {
                BlockDataStruct blockDataStruct = blockData.ConvertToStruct();
                useSpecialFunctions.Add(blockDataStruct);
                // Debug.Log("added use special function");
            }
            current = current.GetComponent<Draggable>().GetNextBlock();
        }
        return useSpecialFunctions;
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

        // remove warrior from list
        // renumber indices within list
        warriorListController.RemoveWarrior(editingIndex);

        StartCoroutine(RemoveWarriorsDelay());

        // // ALTERNATIVELY
        // SceneController.Instance.LoadSceneByName("CodeEditor");
    }

    private IEnumerator RemoveWarriorsDelay() {
        yield return new WaitForSeconds(.001f);
        // clear warrior drawer
        RemoveAllWarriorsFromDrawer();
        yield return new WaitForSeconds(.001f);

        // load warrior at last index --> also clears whiteboard
            // if list now empty, create a new blank one and load it
        // reload warrior drawer
        if (warriorListController.GetCount() == 0) {
            warriorListController.AddWarrior(0, new WarriorFunctionalityData());
        }
        // editingIndex = editingIndex != 0 ? editingIndex -1 : 0;
        LoadWarriorDrawer();
        LoadWarriorToWhiteboard(editingIndex-1, true);
        DebugGetThumbnailData();

        warriorListController.FindJSON(); // reload json file
    }

}




/* OLD CODE -- SAVING FOR LATER


while (current != null) {
            BlockData blockData = current.GetComponent<BlockData>();
            if (blockData.blockType == BlockData.BlockType.PROPERTY && blockData.values.Count != 0) {
                switch (blockData.property) {
                    // FIXME: add some kind of output if parsing doesn't work
                    case BlockData.Property.NAME:
                        warriorPropertiesAndBehavior.warriorName = blockData.values[0];
                        GameObject.Find("NamePreview").GetComponent<TMP_Text>().text = warriorPropertiesAndBehavior.warriorName;
                        break;
                    case BlockData.Property.HEALTH:
                        float.TryParse(blockData.values[0], out warriorPropertiesAndBehavior.health);
                        break;
                    case BlockData.Property.DEFENSE:
                        float.TryParse(blockData.values[0], out warriorPropertiesAndBehavior.defense);
                        break;
                    case BlockData.Property.MOVE_SPEED:
                        float.TryParse(blockData.values[0], out warriorPropertiesAndBehavior.moveSpeed);
                        break;
                    case BlockData.Property.MELEE_ATTACK_RANGE:
                        float.TryParse(blockData.values[0], out warriorPropertiesAndBehavior.meleeAttackRange);
                        break;
                    case BlockData.Property.MELEE_ATTACK_POWER:
                        float.TryParse(blockData.values[0], out warriorPropertiesAndBehavior.meleeAttackPower);
                        break;
                    case BlockData.Property.MELEE_ATTACK_SPEED:
                        float.TryParse(blockData.values[0], out warriorPropertiesAndBehavior.meleeAttackSpeed);
                        break;
                    case BlockData.Property.DISTANCED_RANGE:
                        float.TryParse(blockData.values[0], out warriorPropertiesAndBehavior.distancedRange);
                        break;
                    case BlockData.Property.RANGED_ATTACK_POWER:
                        float.TryParse(blockData.values[0], out warriorPropertiesAndBehavior.rangedAttackPower);
                        break;
                    case BlockData.Property.RANGED_ATTACK_SPEED:
                        float.TryParse(blockData.values[0], out warriorPropertiesAndBehavior.rangedAttackSpeed);
                        break;
                    case BlockData.Property.SPECIAL_POWER:
                        float.TryParse(blockData.values[0], out warriorPropertiesAndBehavior.specialPower);
                        break;
                    case BlockData.Property.SPECIAL_SPEED:
                        float.TryParse(blockData.values[0], out warriorPropertiesAndBehavior.specialSpeed);
                        break;
                    case BlockData.Property.HEAL_POWER:
                        float.TryParse(blockData.values[0], out warriorPropertiesAndBehavior.healPower);
                        break;
                    case BlockData.Property.HEAL_SPEED:
                        float.TryParse(blockData.values[0], out warriorPropertiesAndBehavior.healSpeed);
                        break;

*/

