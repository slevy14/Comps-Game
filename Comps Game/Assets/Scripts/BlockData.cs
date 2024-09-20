using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public class BlockData {
    
    public enum BlockType {
        PROPERTY,
        BEHAVIOR,
        FUNCTION,
        HEADER
    }

    // make sure this is the same as behavior element
    // or does this make behavior element unnecessary?
    public enum Behavior {
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
        CONDITIONAL,
        NONE
    }

    // make sure this is the same as property element
    // or does this make property element unnecessary?
    public enum Property {
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
        HEAL_SPEED,
        NONE
    }

    public BlockType blockType;
    public Behavior behavior;
    public Property property;
    public List<string> arguments;

    // full constructor
    public BlockData(BlockType blockType, Behavior behavior, Property property, List<string> arguments) {
        this.blockType = blockType;
        this.behavior = behavior;
        this.property = property;
        this.arguments = arguments;
    }

    // constructor for property
    public BlockData(BlockType blockType, Property property, List<string> arguments) {
        this.blockType = blockType;
        this.behavior = Behavior.NONE;
        this.property = property;
        this.arguments = arguments;
    }

    // constructor for behavior
    public BlockData(BlockType blockType, Behavior behavior, List<string> arguments) {
        this.blockType = blockType;
        this.behavior = behavior;
        this.property = Property.NONE;
        this.arguments = arguments;
    }

}
