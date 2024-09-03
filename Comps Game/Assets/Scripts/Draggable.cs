using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    private Image image;
    private TMP_Text text;

    public bool onWhiteboard;
    public Transform parentAfterDrag;

    public void OnBeginDrag(PointerEventData eventData) {
        // Debug.Log("begindrag called");
        parentAfterDrag = transform.parent;
        onWhiteboard = false;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        SetMaskable(false);
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
        SetMaskable(true);
        if (!onWhiteboard) {
            Destroy(this.gameObject);
        }
    }

    void Awake() {
        image = this.gameObject.GetComponent<Image>();
        text = this.transform.GetChild(0).GetComponent<TMP_Text>();
        // Debug.Log("awakened");

        // when this script is instantiated,
        // duplicate behavior of begin drag

        // transform.SetParent(transform.root);
        // Debug.Log("root");
        parentAfterDrag = transform.parent;
        onWhiteboard = false;
        transform.SetAsLastSibling();
        SetMaskable(false);
        // transform.SetAsLastSibling();
        // image.raycastTarget = false;
    }

    private void SetMaskable(bool value) {
        image.maskable = value;
        image.raycastTarget = value;
        text.maskable = value;
    }
}
