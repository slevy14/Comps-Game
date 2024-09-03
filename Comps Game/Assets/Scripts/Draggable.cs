using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public Image image;
    public bool onWhiteboard;
    public Transform parentAfterDrag;

    public void OnBeginDrag(PointerEventData eventData) {
        // Debug.Log("begindrag called");
        parentAfterDrag = transform.parent;
        onWhiteboard = false;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData) {
        // if (transform.parent != transform.root) {
        //     transform.SetParent(transform.root);
        // }
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
        // Debug.Log("enddrag called");
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;
        if (!onWhiteboard) {
            Destroy(this.gameObject);
        }
    }

    void Awake() {
        image = this.gameObject.GetComponent<Image>();
        // Debug.Log("awakened");

        // when this script is instantiated,
        // duplicate behavior of begin drag

        // transform.SetParent(transform.root);
        // Debug.Log("root");
        parentAfterDrag = transform.parent;
        onWhiteboard = false;
        transform.SetAsLastSibling();
        image.raycastTarget = false;
        // transform.SetAsLastSibling();
        // image.raycastTarget = false;
    }
}
