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
        PROPERTY   = 0,
        BEHAVIOR   = 1,
        FUNCTION   = 2,
        HEADER     = 3
    }

    // ALL PROPERTIES MUST BE MANUALLY INDEXED
    // DO NOT CHANGE INDICES -- JUST ADD NEW ONES OR SKIP IF NEEDED
    // name changed from "behavior" to get it to treat this like a new thing
    [System.Serializable]
    public enum BehaviorType {
        NONE              = 0,
        TURN              = 1,
        STEP              = 2,
        RUN               = 3,
        TELEPORT          = 4,
        MELEE_ATTACK      = 5,
        SET_TARGET        = 6,
        WHILE_LOOP        = 7,
        FOR_LOOP          = 8,
        END_LOOP          = 9,
        IF                = 10,
        ELSE              = 11,
        END_IF            = 12,
        MELEE_SETTINGS    = 13,
        RANGED_SETTINGS   = 14,
        FIRE_PROJECTILE   = 15
    }

    // ALL PROPERTIES MUST BE MANUALLY INDEXED
    // DO NOT CHANGE INDICES -- JUST ADD NEW ONES OR SKIP IF NEEDED
    [System.Serializable]
    public enum Property {
        NONE                  = 0,
        NAME                  = 1,
        HEALTH                = 2,
        DEFENSE               = 3,
        MOVE_SPEED            = 4,
        MELEE_ATTACK_RANGE    = 5,
        MELEE_ATTACK_POWER    = 6,
        MELEE_ATTACK_SPEED    = 7,
        DISTANCED_RANGE       = 8,
        RANGED_ATTACK_POWER   = 9,
        RANGED_ATTACK_SPEED   = 10,
        SPECIAL_POWER         = 11,
        SPECIAL_SPEED         = 12,
        HEAL_POWER            = 13,
        HEAL_SPEED            = 14,
        MAGIC_SHIELD          = 15
    }

    // tagging as hidden for use in custom editor
     public BlockType blockType;
     public BehaviorType behavior;
     public Property property;
     public List<string> values;

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
    public BlockData.BehaviorType behavior;
    public BlockData.Property property;
    public List<string> values;

     // constructor for property
    public BlockDataStruct(BlockData.BlockType blockType, BlockData.Property property, List<string> values) {
        this.blockType = blockType;
        this.behavior = BlockData.BehaviorType.NONE;
        this.property = property;
        this.values = values;
        // Debug.Log("created property struct");
    }

    // constructor for behavior
    public BlockDataStruct(BlockData.BlockType blockType, BlockData.BehaviorType behavior, List<string> values) {
        this.blockType = blockType;
        this.behavior = behavior;
        this.property = BlockData.Property.NONE;
        this.values = values;
        // Debug.Log("created behavior struct");
    }

    // constructor for function or header
    public BlockDataStruct(BlockData.BlockType blockType, List<string> values) {
        this.blockType = blockType;
        this.behavior = BlockData.BehaviorType.NONE;
        this.property = BlockData.Property.NONE;
        this.values = values;
    }
}



// // EDITOR
// #if UNITY_EDITOR
// [CustomEditor(typeof(BlockData))]
// public class BlockData_Editor : Editor {
//     public override void OnInspectorGUI() {
//         var script = (BlockData)target;

//         script.blockType = (BlockData.BlockType)EditorGUILayout.EnumPopup("Block Type", script.blockType);

//         if (script.blockType == BlockData.BlockType.PROPERTY) {
//             script.property = (BlockData.Property)EditorGUILayout.EnumPopup("Property", script.property);
//         } else if (script.blockType == BlockData.BlockType.BEHAVIOR){
//             script.behavior = (BlockData.Behavior)EditorGUILayout.EnumPopup("Behavior", script.behavior);
//         } else {
//             return;
//         }

//         SerializedObject so = new SerializedObject(target);
//         SerializedProperty stringsProperty = so.FindProperty("values");

//         EditorGUILayout.PropertyField(stringsProperty, true);
//         so.ApplyModifiedProperties();
//     }
// }
// #endif
