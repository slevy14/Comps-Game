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

    private int counter = 0;

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



    public void SaveTower() {
        // // debug code -- for testing!!
        // if (DEBUG_MODE) {
        //     GameObject tempObject = new GameObject();
        //     tempObject.name = "tempObject_" + counter;
        //     tempObject.AddComponent<Rigidbody2D>();
        //     bool prefabSuccess;
        //     PrefabUtility.SaveAsPrefabAssetAndConnect(tempObject, $"Assets/SavedAssets/{tempObject.name}.prefab", InteractionMode.UserAction, out prefabSuccess);
        //     if (prefabSuccess == true) {
        //         Debug.Log("Prefab was saved successfully");
        //     } else {
        //         Debug.Log("Prefab failed to save" + prefabSuccess);
        //     }
        //     counter++;
        //     Destroy(tempObject);
        // }

        SaveIntoJSON();
    }


    public void SaveIntoJSON() {
        WarriorProperties _WarriorProperties = ParseProperties();
        string warriorPropertiesJSON = JsonUtility.ToJson(_WarriorProperties);
        string filePath = Application.persistentDataPath + $"/{_WarriorProperties.warriorName}.json";
        System.IO.File.WriteAllText(filePath, warriorPropertiesJSON);
        print("saving json at " + filePath);
    }


    public WarriorProperties ParseProperties() {
        GameObject current = propertiesHeaderObject.GetComponent<Draggable>().GetNextBlock();
        WarriorProperties warriorProperties = new WarriorProperties();
        while (current != null) {
            BlockData blockData = current.GetComponent<BlockData>();
            if (blockData.blockType == BlockData.BlockType.PROPERTY && blockData.values.Count != 0) {
                switch (blockData.property) {
                    // FIXME: add some kind of output if parsing doesn't work
                    case BlockData.Property.NAME:
                        warriorProperties.warriorName = blockData.values[0];
                        break;
                    case BlockData.Property.HEALTH:
                        float.TryParse(blockData.values[0], out warriorProperties.health);
                        break;
                    case BlockData.Property.DEFENSE:
                        float.TryParse(blockData.values[0], out warriorProperties.defense);
                        break;
                    case BlockData.Property.MOVE_SPEED:
                        float.TryParse(blockData.values[0], out warriorProperties.moveSpeed);
                        break;
                    case BlockData.Property.MELEE_ATTACK_RANGE:
                        float.TryParse(blockData.values[0], out warriorProperties.meleeAttackRange);
                        break;
                    case BlockData.Property.MELEE_ATTACK_POWER:
                        float.TryParse(blockData.values[0], out warriorProperties.meleeAttackPower);
                        break;
                    case BlockData.Property.MELEE_ATTACK_SPEED:
                        float.TryParse(blockData.values[0], out warriorProperties.meleeAttackSpeed);
                        break;
                    case BlockData.Property.DISTANCED_RANGE:
                        float.TryParse(blockData.values[0], out warriorProperties.distancedRange);
                        break;
                    case BlockData.Property.RANGED_ATTACK_POWER:
                        float.TryParse(blockData.values[0], out warriorProperties.rangedAttackPower);
                        break;
                    case BlockData.Property.RANGED_ATTACK_SPEED:
                        float.TryParse(blockData.values[0], out warriorProperties.rangedAttackSpeed);
                        break;
                    case BlockData.Property.SPECIAL_POWER:
                        float.TryParse(blockData.values[0], out warriorProperties.specialPower);
                        break;
                    case BlockData.Property.SPECIAL_SPEED:
                        float.TryParse(blockData.values[0], out warriorProperties.specialSpeed);
                        break;
                    case BlockData.Property.HEAL_POWER:
                        float.TryParse(blockData.values[0], out warriorProperties.healPower);
                        break;
                    case BlockData.Property.HEAL_SPEED:
                        float.TryParse(blockData.values[0], out warriorProperties.healSpeed);
                        break;
                }
            }
            current = current.GetComponent<Draggable>().GetNextBlock();
        }
        return warriorProperties;
    }


}

// for saving!
[System.Serializable]
public class WarriorProperties {
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

    public WarriorProperties() { // set default values with constructor
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
    }
}
