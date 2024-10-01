using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Whiteboard : MonoBehaviour, IDropHandler {

    public void OnDrop(PointerEventData eventData) {
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject.GetComponent<Draggable>() != null) {
            Draggable draggable = droppedObject.GetComponent<Draggable>();
            draggable.parentAfterDrag = transform;
            draggable.onWhiteboard = true;
        }
    }

}
