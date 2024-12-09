using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Whiteboard : MonoBehaviour, IDropHandler {

    // script placed on the actual whiteboard object
    // this is mostly deprecated with the current drag and drop system
    // current system drops onto overlap boxes

    public void OnDrop(PointerEventData eventData) {
        // check if block is dropped on the whiteboard
        // and update that block's on whiteboard status
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject.GetComponent<Draggable>() != null) {
            // Debug.Log("dropped Object: " + droppedObject.name);
            Draggable draggable = droppedObject.GetComponent<Draggable>();
            draggable.parentAfterDrag = transform;
            draggable.onWhiteboard = true;
        }
    }

}
