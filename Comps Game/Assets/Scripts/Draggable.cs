using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    // things that get in the way of raycasts
    private Image image;
    private TMP_Text text;
    private Image overlapBox;
    private GameObject inputField;
    private GameObject whiteboard;

    // debug
    [SerializeField] private GameObject prevBlock;
    [SerializeField] private GameObject nextBlock;

    // parenting
    public bool onWhiteboard;
    public Transform parentAfterDrag;
    public Vector3 blockOffset;
    public bool isHeader;

    // ENUMS
    public enum BlockType {
        PROPERTY,
        BEHAVIOR,
        FUNCTION
    }


    public void OnBeginDrag(PointerEventData eventData) {
        if (!isHeader) {
            // Debug.Log("begindrag called");
            parentAfterDrag = transform.parent;
            onWhiteboard = false;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();
            SetMaskable(false);
            // SetBlockRaycasts(false);

        }
    }

    public void OnDrag(PointerEventData eventData) {
        if (!isHeader) {
            // move all other blocks in function
            UpdateBlockPositions(this.gameObject, Input.mousePosition);
        }
    }

    public void UpdateBlockPositions(GameObject block, Vector3 newPosition) {
        if (!isHeader) {
            block.transform.position = newPosition;
            if (nextBlock != null) {
                nextBlock.GetComponent<Draggable>().UpdateBlockPositions(nextBlock, newPosition - blockOffset);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (!isHeader) {
            // Debug.Log("enddrag called");

            if (!onWhiteboard) {
                Destroy(this.gameObject);
            }

            if (!SnapToBlock(eventData) && prevBlock != null) { // attempt to snap, but if not:
                prevBlock.GetComponent<Draggable>().nextBlock = null; // reset next block on previous
                prevBlock = null;
            }

            transform.SetParent(parentAfterDrag);
            SetMaskable(true);
            // SetBlockRaycasts(true);
        }
    }

    public void SetValue() {
        this.gameObject.GetComponent<PropertyElement>().value = inputField.GetComponent<TMP_InputField>().text;
    }

    void Awake() {
        int childCount = this.gameObject.transform.childCount;
        image = this.gameObject.GetComponent<Image>();
        text = this.transform.GetChild(0).GetComponent<TMP_Text>();
        overlapBox = this.transform.GetChild(1).GetComponent<Image>();
        if (childCount > 2 && this.transform.GetChild(2).name == "InputField") {
            inputField = this.transform.GetChild(2).gameObject;
            inputField.GetComponent<TMP_InputField>().onEndEdit.AddListener(delegate{SetValue();});
        }

        blockOffset = new Vector3(0, 50, 0);
        whiteboard = GameObject.FindGameObjectWithTag("whiteboard");
        // Debug.Log("awakened");

        // when this script is instantiated,
        // duplicate behavior of begin drag

        // transform.SetParent(transform.root);
        // Debug.Log("root");
        if (!isHeader) {
            parentAfterDrag = transform.parent;
            onWhiteboard = false;
            transform.SetAsLastSibling();
            SetMaskable(false);
            // SetBlockRaycasts(false);
        }
        // transform.SetAsLastSibling();
        // image.raycastTarget = false;
    }

    public void SetNextBlock(GameObject nextBlock) {
        this.nextBlock = nextBlock;
        Debug.Log(this.gameObject.name + " has next " + nextBlock.name);
    }

    public GameObject GetNextBlock() {
        return this.nextBlock;
    }

    public void SetMaskable(bool value) {
        image.maskable = value;
        image.raycastTarget = value;
        text.maskable = value;
        overlapBox.raycastTarget = value;

        //input field
        if (inputField != null) {
            inputField.GetComponent<Image>().raycastTarget = value;
            GameObject textArea = inputField.transform.GetChild(0).gameObject;
            int count = textArea.transform.childCount;
            for (int i = 0; i < count; i++) {
                if (textArea.transform.GetChild(i).gameObject.tag == "disableCast") {
                    textArea.transform.GetChild(i).gameObject.GetComponent<TMP_Text>().raycastTarget = value;
                } else { // caret
                    textArea.transform.GetChild(i).gameObject.GetComponent<TMP_SelectionCaret>().raycastTarget = value;
                }
            }
        }
    }

    private void SetBlockRaycasts(bool value) {
        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = value;
        canvasGroup.ignoreParentGroups = value;
    }

    // boolean, returns false if didn't snap
    private bool SnapToBlock(PointerEventData eventData) {
        // List<RaycastResult> result = new List<RaycastResult>();
        // EventSystem.current.RaycastAll(eventData.pointer)
        if (eventData.pointerCurrentRaycast.gameObject.tag == "overlapSpace") {
            GameObject blockToSnapTo = eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject;
            if (blockToSnapTo.GetComponent<Draggable>().GetNextBlock() == null) {
                Debug.Log("snapping!");
                onWhiteboard = true; // make sure still set to true to snap

                if (this.prevBlock != null) {
                    this.prevBlock.GetComponent<Draggable>().SetNextBlock(null);
                }

                this.prevBlock = blockToSnapTo;
                prevBlock.GetComponent<Draggable>().SetNextBlock(this.gameObject);
                UpdateBlockPositions(this.gameObject, prevBlock.transform.position - blockOffset);
                Debug.Log(this.prevBlock.name);
                return true; // snapped 
            }
        }
        return false; // didn't snap
    }
}
