using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public Image image;
    [HideInInspector] public Transform parentAfterDrag;
    private Transform initialParent;

    public void OnBeginDrag(PointerEventData eventData) {
        if (parentAfterDrag.tag != "whiteboard" ) {
            parentAfterDrag = transform.parent;
        } else {
            parentAfterDrag = initialParent;
        }
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
        transform.SetParent(parentAfterDrag);
        // if (parentAfterDrag.tag != "whiteboard") {
        //     Destroy(this.gameObject);
        // }
        image.raycastTarget = true;
    }

    void Start() {
        initialParent = transform.parent;
        parentAfterDrag = initialParent;
    }
}
