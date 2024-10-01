using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class BlockData : MonoBehaviour {
    
    public enum BlockType {
        PROPERTY,
        BEHAVIOR,
        FUNCTION,
        HEADER
    }

    // make sure this is the same as behavior element
    // or does this make behavior element unnecessary?
    public enum Behavior {
        NONE,
        FACE,
        STEP,
        RUN,
        TELEPORT,
        MELEE_ATTACK,
        HEAL,
        CHARGE,
        ENTER_RANGE,
        SET_TARGET_ENEMY,
        LOOP,
        CONDITIONAL
    }

    // make sure this is the same as property element
    // or does this make property element unnecessary?
    public enum Property {
        NONE,
        NAME,
        HEALTH,
        DEFENSE,
        MOVE_SPEED,
        MELEE_ATTACK_RANGE,
        MELEE_ATTACK_POWER,
        MELEE_ATTACK_SPEED,
        DISTANCED_RANGE,
        RANGED_ATTACK_POWER,
        RANGED_ATTACK_SPEED,
        SPECIAL_POWER,
        SPECIAL_SPEED,
        HEAL_POWER,
        HEAL_SPEED
    }

    // tagging as hidden for use in custom editor
    [HideInInspector] public BlockType blockType;
    [HideInInspector] public Behavior behavior;
    [HideInInspector] public Property property;
    [HideInInspector] public List<string> values;

    public BlockDataStruct ConvertToStruct() {
        if (blockType == BlockType.PROPERTY) {
            return new BlockDataStruct(blockType, property, values);
        } else if (blockType == BlockType.BEHAVIOR) {
            return new BlockDataStruct(blockType, behavior, values);
        } else {
            return new BlockDataStruct(blockType, values);
        }
    }

    public void SetBlockDataValues(List<string> vals) {
        this.values = vals;
    }

}

// struct for pass by value
[System.Serializable]
public struct BlockDataStruct {
    public BlockData.BlockType blockType;
    public BlockData.Behavior behavior;
    public BlockData.Property property;
    public List<string> values;

     // constructor for property
    public BlockDataStruct(BlockData.BlockType blockType, BlockData.Property property, List<string> values) {
        this.blockType = blockType;
        this.behavior = BlockData.Behavior.NONE;
        this.property = property;
        this.values = values;
        Debug.Log("created property struct");
    }

    // constructor for behavior
    public BlockDataStruct(BlockData.BlockType blockType, BlockData.Behavior behavior, List<string> values) {
        this.blockType = blockType;
        this.behavior = behavior;
        this.property = BlockData.Property.NONE;
        this.values = values;
        Debug.Log("created behavior struct");
    }

    // constructor for function or header
    public BlockDataStruct(BlockData.BlockType blockType, List<string> values) {
        this.blockType = blockType;
        this.behavior = BlockData.Behavior.NONE;
        this.property = BlockData.Property.NONE;
        this.values = values;
    }
}



// EDITOR
#if UNITY_EDITOR
[CustomEditor(typeof(BlockData))]
public class BlockData_Editor : Editor {
    public override void OnInspectorGUI() {
        var script = (BlockData)target;

        script.blockType = (BlockData.BlockType)EditorGUILayout.EnumPopup("Block Type", script.blockType);

        if (script.blockType == BlockData.BlockType.PROPERTY) {
            script.property = (BlockData.Property)EditorGUILayout.EnumPopup("Property", script.property);
        } else if (script.blockType == BlockData.BlockType.BEHAVIOR){
            script.behavior = (BlockData.Behavior)EditorGUILayout.EnumPopup("Behavior", script.behavior);
        } else {
            return;
        }

        SerializedObject so = new SerializedObject(target);
        SerializedProperty stringsProperty = so.FindProperty("values");

        EditorGUILayout.PropertyField(stringsProperty, true);
        so.ApplyModifiedProperties();
    }
}
#endif
