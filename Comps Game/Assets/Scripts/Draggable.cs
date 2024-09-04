using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    // things that get in the way of raycasts
    private Image image;
    private TMP_Text text;
    private Image overlapBox;

    // debug
    [SerializeField] private GameObject prevBlock;
    [SerializeField] private GameObject nextBlock;

    // parenting
    public bool onWhiteboard;
    public Transform parentAfterDrag;
    public Vector3 blockOffset;


    public void OnBeginDrag(PointerEventData eventData) {
        // Debug.Log("begindrag called");
        parentAfterDrag = transform.parent;
        onWhiteboard = false;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        SetMaskable(false);
    }

    public void OnDrag(PointerEventData eventData) {
        // move all other blocks in function
        UpdateBlockPositions(this.gameObject, Input.mousePosition);
    }

    public void UpdateBlockPositions(GameObject block, Vector3 newPosition) {
        block.transform.position = newPosition;
        if (nextBlock != null) {
            nextBlock.GetComponent<Draggable>().UpdateBlockPositions(nextBlock, newPosition - blockOffset);
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        // Debug.Log("enddrag called");
        if (!SnapToBlock(eventData) && (prevBlock != null)) { // attempt to snap, but if not:
            prevBlock.GetComponent<Draggable>().nextBlock = null; // reset next block on previous
            prevBlock = null;
        }

        if (!onWhiteboard) {
            Destroy(this.gameObject);
        }

        transform.SetParent(parentAfterDrag);
        SetMaskable(true);
    }

    void Awake() {
        image = this.gameObject.GetComponent<Image>();
        text = this.transform.GetChild(0).GetComponent<TMP_Text>();
        overlapBox = this.transform.GetChild(1).GetComponent<Image>();
        blockOffset = new Vector3(0, 50, 0);
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

    public void SetNextBlock(GameObject nextBlock) {
        this.nextBlock = nextBlock;
    }

    private void SetMaskable(bool value) {
        image.maskable = value;
        image.raycastTarget = value;
        text.maskable = value;
        overlapBox.raycastTarget = value;
    }

    // boolean, returns false if didn't snap
    private bool SnapToBlock(PointerEventData eventData) {
        // List<RaycastResult> result = new List<RaycastResult>();
        // EventSystem.current.RaycastAll(eventData.pointer)
        if (eventData.pointerCurrentRaycast.gameObject.tag == "overlapSpace") {
            Debug.Log("snapping!");
            onWhiteboard = true; // make sure still set to true to snap

            prevBlock = eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject;
            prevBlock.GetComponent<Draggable>().SetNextBlock(this.gameObject);
            UpdateBlockPositions(this.gameObject, prevBlock.transform.position - blockOffset);
            return true; // snapped
        }
        return false; // didn't snap
    }
}
