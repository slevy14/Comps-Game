using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlockListItem : MonoBehaviour, IBeginDragHandler, IDragHandler {

    public Image image;
    public GameObject clonePrefab; // set in inspector, copy of this object
    [HideInInspector] public Transform parentAfterDrag;

    public void OnBeginDrag(PointerEventData eventData) {
        GameObject clone = Instantiate(this.gameObject, this.transform.position, this.transform.rotation, transform.parent);
        // Debug.Log("scroll parent");
        clone.AddComponent<Draggable>();
        Destroy(clone.gameObject.GetComponent<BlockListItem>());
        eventData.pointerDrag = clone;
    }

    public void OnDrag(PointerEventData eventData) {
        throw new System.NotImplementedException();
    }

    public void InitializeBlockDraggable() {
        this.gameObject.AddComponent<Draggable>();
        Destroy(this.gameObject.GetComponent<BlockListItem>());
    }
}
