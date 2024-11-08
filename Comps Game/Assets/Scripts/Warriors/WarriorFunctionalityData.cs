using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// for saving!
[System.Serializable]
public class WarriorFunctionalityData {

    // referencing properties
    public string warriorName;
    public int warriorIndex;

    public int spriteIndex;

    // maximums
    public int warriorStrength;
    public int behaviorCount;

    // header positions
    public Vector2 propertiesHeaderPosition;
    public Vector2 moveHeaderPosition;
    public Vector2 useWeaponHeaderPosition;
    public Vector2 useSpecialHeaderPosition;

    // lists
    public List<BlockDataStruct> properties;
    public List<BlockDataStruct> moveFunctions;
    public List<BlockDataStruct> useWeaponFunctions;
    public List<BlockDataStruct> useSpecialFunctions;


    public WarriorFunctionalityData() { // set default values with constructor
        warriorName = "knight";

        spriteIndex = 0;

        properties = new List<BlockDataStruct>();
        moveFunctions = new List<BlockDataStruct>();
        useWeaponFunctions = new List<BlockDataStruct>();
        useSpecialFunctions = new List<BlockDataStruct>();

        SetHeaderPositions();
    }

    public WarriorFunctionalityData(int warriorIndex) { // constructor with index
        warriorName = "noname";

        this.warriorIndex = warriorIndex;
        spriteIndex = 0;

        properties = new List<BlockDataStruct>();
        moveFunctions = new List<BlockDataStruct>();
        useWeaponFunctions = new List<BlockDataStruct>();
        useSpecialFunctions = new List<BlockDataStruct>();
    }

    // with no arguments set default positions
    public void SetHeaderPositions() {
        propertiesHeaderPosition = new Vector2(-782, 895);
        moveHeaderPosition = new Vector2(-186, 895);
        useWeaponHeaderPosition = new Vector2(-782, 441);
        useSpecialHeaderPosition = new Vector2(-186, 441);
    }

    // overload for custom positions
    public void SetHeaderPositions(Vector2 newPropertiesHeaderPosition, Vector2 newMoveHeaderPosition, Vector2 newUseWeaponHeaderPosition, Vector2 newSpecialHeaderPosition) {
        propertiesHeaderPosition = newPropertiesHeaderPosition;
        moveHeaderPosition = newMoveHeaderPosition;
        useWeaponHeaderPosition = newUseWeaponHeaderPosition;
        useSpecialHeaderPosition = newSpecialHeaderPosition;
    }
}
