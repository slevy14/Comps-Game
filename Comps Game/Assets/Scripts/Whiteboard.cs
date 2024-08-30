using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Whiteboard : MonoBehaviour, IDropHandler {

    public void OnDrop(PointerEventData eventData) {
        GameObject dropped = eventData.pointerDrag;
        Draggable draggable = dropped.GetComponent<Draggable>();
        draggable.parentAfterDrag = transform;
    }
    
}
