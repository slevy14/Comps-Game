using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class PropertyElement : MonoBehaviour {

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
        HEAL_SPEED
    }

    public Property property;
    public string value;
}
