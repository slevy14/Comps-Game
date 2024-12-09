using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlockListItem : MonoBehaviour, IBeginDragHandler, IDragHandler {

    // placed on all blocks in the drawer to act as thumbnails

    public Image image;
    public GameObject clonePrefab; // set in inspector, copy of this object
    [HideInInspector] public Transform parentAfterDrag;

    public void OnBeginDrag(PointerEventData eventData) {
        // do nothing if currently enemy displayed
        // not allowed to drag blocks
        if (DesignerController.Instance.isCurrentWarriorEnemy) {
            eventData.pointerDrag = null;
            return;
        }

        // duplicate this block
        GameObject clone = Instantiate(this.gameObject, this.transform.position, this.transform.rotation, transform.parent);
        // replace this script on new block with Draggable
        clone.AddComponent<Draggable>();
        Destroy(clone.gameObject.GetComponent<BlockListItem>());
        // change current dragging block to new block
        eventData.pointerDrag = clone;
        // play sound
        AudioController.Instance.PlaySoundEffect("Block Pickup");
    }

    public void OnDrag(PointerEventData eventData) {
        // not used, just needed for interface
    }

    public void InitializeBlockDraggable() {
        // used when instantiating this block directly to whiteboard
        // replace this script with Draggable
        this.gameObject.AddComponent<Draggable>();
        this.gameObject.GetComponent<Draggable>().onWhiteboard = true;
        Destroy(this.gameObject.GetComponent<BlockListItem>());
    }
}
