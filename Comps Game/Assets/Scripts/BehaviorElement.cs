using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class BehaviorElement : MonoBehaviour {

    public enum Property {
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

    public Property property;
    public string value;
}
