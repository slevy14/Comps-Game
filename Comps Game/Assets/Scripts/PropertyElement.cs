using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class PropertyElement : MonoBehaviour {

    public enum Property {
        towerName,
        targetingRange,
        rotationSpeed,
        bps
    };

    public Property property;
    public string value;
}
