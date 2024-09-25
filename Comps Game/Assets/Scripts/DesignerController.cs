using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DesignerController : MonoBehaviour {

    [Header("DEBUG")]
    [SerializeField] private bool DEBUG_MODE; // set in inspector

    [Header("References")]
    [SerializeField] private GameObject propertiesHeaderObject;
    [SerializeField] private GameObject moveHeaderObject;
    [SerializeField] private GameObject useWeaponHeaderObject;
    [SerializeField] private GameObject useSpecialHeaderObject;
    [SerializeField] private GameObject blockDrawer;
    [SerializeField] private GameObject warriorDrawer;

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
    public void SaveTower() {
        WarriorPropertiesAndBehavior _WarriorPropertiesAndBehavior = ParseProperties();
        _WarriorPropertiesAndBehavior.moveFunctions = ParseMove();
        _WarriorPropertiesAndBehavior.useWeaponFunctions = ParseUseWeapon();
        _WarriorPropertiesAndBehavior.useSpecialFunctions = ParseUseSpecial();
        SaveIntoJSON(_WarriorPropertiesAndBehavior);
    }


    public void SaveIntoJSON(WarriorPropertiesAndBehavior warriorPropertiesAndBehavior) {
        string warriorPropertiesJSON = JsonUtility.ToJson(warriorPropertiesAndBehavior);
        string filePath = Application.persistentDataPath + $"/{warriorPropertiesAndBehavior.warriorName}.json";
        System.IO.File.WriteAllText(filePath, warriorPropertiesJSON);
        Debug.Log("saving json at " + filePath);
    }


    public WarriorPropertiesAndBehavior ParseProperties() {
        GameObject current = propertiesHeaderObject.GetComponent<Draggable>().GetNextBlock();
        WarriorPropertiesAndBehavior warriorPropertiesAndBehavior = new WarriorPropertiesAndBehavior();
        while (current != null) {
            BlockData blockData = current.GetComponent<BlockData>();
            if (blockData.blockType == BlockData.BlockType.PROPERTY && blockData.values.Count != 0) {
                switch (blockData.property) {
                    // FIXME: add some kind of output if parsing doesn't work
                    case BlockData.Property.NAME:
                        warriorPropertiesAndBehavior.warriorName = blockData.values[0];
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
                }
            }
            current = current.GetComponent<Draggable>().GetNextBlock();
        }
        return warriorPropertiesAndBehavior;
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


}

// for saving!
[System.Serializable]
public class WarriorPropertiesAndBehavior {
    public string warriorName;
    public float health;
    public float defense;
    public float moveSpeed;
    public float meleeAttackPower;
    public float meleeAttackSpeed;
    public float meleeAttackRange;
    public float rangedAttackPower;
    public float rangedAttackSpeed;
    public float distancedRange;
    public float specialPower;
    public float specialSpeed;
    public float healPower;
    public float healSpeed;

    public List<BlockDataStruct> moveFunctions;
    public List<BlockDataStruct> useWeaponFunctions;
    public List<BlockDataStruct> useSpecialFunctions;


    public WarriorPropertiesAndBehavior() { // set default values with constructor
        warriorName = "noname";
        health = 0;
        defense = 0;
        moveSpeed = 0;
        meleeAttackPower = 0;
        meleeAttackSpeed = 0;
        meleeAttackRange = 0;
        rangedAttackPower = 0;
        rangedAttackSpeed = 0;
        distancedRange = 0;
        specialPower = 0;
        specialSpeed = 0;
        healPower = 0;
        healSpeed = 0;

        moveFunctions = new List<BlockDataStruct>();
        useWeaponFunctions = new List<BlockDataStruct>();
        useSpecialFunctions = new List<BlockDataStruct>();
    }
}
