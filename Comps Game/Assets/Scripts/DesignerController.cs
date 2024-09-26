using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DesignerController : MonoBehaviour {

    [Header("DEBUG")]
    [SerializeField] private bool DEBUG_MODE; // set in inspector

    [Header("REFERENCES")]
    [Header("Headers")]
    [SerializeField] private GameObject propertiesHeaderObject;
    [SerializeField] private GameObject moveHeaderObject;
    [SerializeField] private GameObject useWeaponHeaderObject;
    [SerializeField] private GameObject useSpecialHeaderObject;

    [Header("Drawers")]
    [SerializeField] private GameObject blockDrawer;
    [SerializeField] private GameObject warriorDrawer;
    
    [Header("Sprites")]
    [SerializeField] private GameObject warriorThumbnailPrefab;
    [SerializeField] public List<SpriteData> spriteDataList;
    [SerializeField] public int spriteDataIndex;

    [Header("Property Blocks")]
    [SerializeField] private List<GameObject> propertyBlocks;

    void Start() {

    }

    void Update() {

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


    // Saving
    public void SaveWarrior() {
        WarriorFunctionalityData _WarriorFunctionalityData = new WarriorFunctionalityData();
        _WarriorFunctionalityData.spriteIndex = spriteDataIndex;
        _WarriorFunctionalityData.warriorName = ParseName();
        _WarriorFunctionalityData.properties = ParseProperties();
        _WarriorFunctionalityData.moveFunctions = ParseMove();
        _WarriorFunctionalityData.useWeaponFunctions = ParseUseWeapon();
        _WarriorFunctionalityData.useSpecialFunctions = ParseUseSpecial();
        SaveIntoJSON(_WarriorFunctionalityData);
    }


    public void SaveIntoJSON(WarriorFunctionalityData warriorFunctionalityData) {
        string warriorPropertiesJSON = JsonUtility.ToJson(warriorFunctionalityData);
        string filePath = Application.persistentDataPath + $"/{warriorFunctionalityData.warriorName}.json";
        System.IO.File.WriteAllText(filePath, warriorPropertiesJSON);
        Debug.Log("saving json at " + filePath);
    }

    public string ParseName() {
        string name = "noname";
        GameObject current = propertiesHeaderObject.GetComponent<Draggable>().GetNextBlock();
        while (current != null) {
            BlockData blockData = current.GetComponent<BlockData>();
            if (blockData.blockType == BlockData.BlockType.PROPERTY) {
                if (blockData.property == BlockData.Property.NAME && (blockData.values.Count != 0)) {
                    GameObject.Find("NamePreview").GetComponent<TMP_Text>().text = blockData.values[0];
                    name = blockData.values[0];
                }
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
                Debug.Log("added property");
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
                Debug.Log("added move function");
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
                Debug.Log("added use weapon function");
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
                Debug.Log("added use special function");
            }
            current = current.GetComponent<Draggable>().GetNextBlock();
        }
        return useSpecialFunctions;
    }



    // LOADING



}

// for saving!
[System.Serializable]
public class WarriorFunctionalityData {
    public string warriorName;

    public int spriteIndex;

    public List<BlockDataStruct> properties;
    public List<BlockDataStruct> moveFunctions;
    public List<BlockDataStruct> useWeaponFunctions;
    public List<BlockDataStruct> useSpecialFunctions;


    public WarriorFunctionalityData() { // set default values with constructor
        warriorName = "noname";

        spriteIndex = 0;

        properties = new List<BlockDataStruct>();
        moveFunctions = new List<BlockDataStruct>();
        useWeaponFunctions = new List<BlockDataStruct>();
        useSpecialFunctions = new List<BlockDataStruct>();
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

