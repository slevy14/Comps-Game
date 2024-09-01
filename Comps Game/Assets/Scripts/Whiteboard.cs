using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Whiteboard : MonoBehaviour, IDropHandler {

    public void OnDrop(PointerEventData eventData) {
        Debug.Log("ondrop called");
        GameObject droppedObject = eventData.pointerDrag;
        Draggable draggable = droppedObject.GetComponent<Draggable>();
        draggable.parentAfterDrag = transform;
        draggable.onWhiteboard = true;
    }

}
