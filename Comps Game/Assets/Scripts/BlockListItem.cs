using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlockListItem : MonoBehaviour, IBeginDragHandler, IDragHandler {

    public Image image;
    [HideInInspector] public Transform parentAfterDrag;

    public void OnBeginDrag(PointerEventData eventData) {
        GameObject clone = Instantiate(this.gameObject,eventData.position, this.transform.rotation, transform.parent);
        Debug.Log("scroll parent");
        clone.AddComponent<Draggable>();
        Destroy(clone.gameObject.GetComponent<BlockListItem>());
        eventData.pointerDrag = clone;

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
