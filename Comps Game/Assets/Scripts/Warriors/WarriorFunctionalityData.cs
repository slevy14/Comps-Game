using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// for saving!
[System.Serializable]
public class WarriorFunctionalityData {
    public string warriorName;
    public int warriorIndex;

    public int spriteIndex;

    public int warriorStrength;
    public int behaviorCount;

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
}
