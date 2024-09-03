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
        GameObject clone = Instantiate(clonePrefab, this.transform.position, this.transform.rotation, transform.root);
        // Debug.Log("scroll parent");
        clone.AddComponent<Draggable>();
        Destroy(clone.gameObject.GetComponent<BlockListItem>());
        eventData.pointerDrag = clone;
        // clone.gameObject.GetComponent<Draggable>().OnBeginDrag(eventData);

        // if (parentAfterDrag.tag != "whiteboard" ) {
        //     parentAfterDrag = transform.parent;
        // } else {
        //     parentAfterDrag = initialParent;
        // }
        // transform.SetParent(transform.root);
        // transform.SetAsLastSibling();
        // image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData) {
        throw new System.NotImplementedException();
    }
}
