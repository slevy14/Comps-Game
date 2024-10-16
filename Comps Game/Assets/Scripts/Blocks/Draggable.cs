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
    private GameObject dropdownOne;
    private GameObject dropdownTwo;
    private GameObject whiteboard;

    // serialized for debug
    [SerializeField] private GameObject prevBlock;
    [SerializeField] private GameObject nextBlock;
    [SerializeField] private Vector3 initialPos;

    // parenting
    public bool onWhiteboard;
    public Transform parentAfterDrag;
    public Vector3 blockOffset;
    public bool isHeader;
    
    // visuals
    [SerializeField] private TMP_Text namePreview;

    void Awake() {
        int childCount = this.gameObject.transform.childCount;
        image = this.gameObject.GetComponent<Image>();
        text = this.transform.GetChild(0).GetComponent<TMP_Text>();
        overlapBox = this.transform.GetChild(1).GetComponent<Image>();
        if (childCount == 3 && this.transform.GetChild(2).name == "InputField") {
            inputField = this.transform.GetChild(2).gameObject;
            inputField.GetComponent<TMP_InputField>().onEndEdit.AddListener(delegate{SetValue();});
        }
        if (childCount >= 3 && this.transform.GetChild(2).gameObject.GetComponent<TMP_Dropdown>() != null) {
            dropdownOne = this.transform.GetChild(2).gameObject;
        }
        if (childCount == 4 && this.transform.GetChild(3).gameObject.GetComponent<TMP_Dropdown>() != null) {
            dropdownTwo = this.transform.GetChild(3).gameObject;
        }

        // Debug.Log(gameObject.name + " height: " + gameObject.GetComponent<RectTransform>().rect.height);
        // SET OFFSET
        SetBlockOffset(false);
        // Debug.Log("offset: " + blockOffset.y);
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

        // come back to this -- not good practice to find by name!
        // TMP_Text namePreview = GameObject.Find("NamePreview").GetComponent<TMP_Text>();
    }


    public void OnBeginDrag(PointerEventData eventData) {
        // if (!isHeader) {
            // Debug.Log("begindrag called");
            parentAfterDrag = transform.parent;
            onWhiteboard = false;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();
            SetMaskable(false);

            // remove from linked list
            if (prevBlock != null) {
                prevBlock.GetComponent<Draggable>().SetNextBlock(null); // reset next block on previous
                // Debug.Log("updated prev block next");
                prevBlock = null;
            }
            // SetBlockRaycasts(false);
        // } else {
            initialPos = transform.position;
        // }
    }

    public void OnDrag(PointerEventData eventData) {
        // if (!isHeader) {
            // move all other blocks in function
            UpdateBlockPositions(this.gameObject, Input.mousePosition);
        // }
    }

    public void UpdateBlockPositions(GameObject block, Vector3 newPosition) {
        // if (!isHeader) {
            // Debug.Log(block.gameObject.name + "should set to: " + newPosition.y);
            block.transform.position = newPosition;
            // Debug.Log(block.gameObject.name + " actual y pos: " + block.transform.position.y);
            if (nextBlock != null) {
                nextBlock.GetComponent<Draggable>().UpdateBlockPositions(nextBlock, newPosition - blockOffset);
            }
        // }
    }

    public void DestroyStack(GameObject block) {
        // Debug.Log("Destroying " + this.gameObject.name + " from destroy stack");
        if (this.nextBlock != null) {
            this.nextBlock.GetComponent<Draggable>().DestroyStack(nextBlock);
        }
        // Debug.Log("destroying " + this.gameObject.name);
        Destroy(block);
    }

    public void OnEndDrag(PointerEventData eventData) {
        // if (MouseOverWhiteboard()) {
        //     onWhiteboard = true;
        // }
        // Debug.Log("ended drag over " + eventData.pointerCurrentRaycast.gameObject.name);
        if (!isHeader) {
            // Debug.Log("enddrag called");

            if (!onWhiteboard) {
                DestroyStack(this.gameObject);
            }

            // need to do this here because if instantiated, on awake offset set to 0 for some reason!
            SetBlockOffset(false);

            if (!SnapToBlock(eventData)) { // attempt to snap, but if not:
                // prevBlock.GetComponent<Draggable>().nextBlock = null; // reset next block on previous
            }

            transform.SetParent(parentAfterDrag);
            SetMaskable(true);
            // SetBlockRaycasts(true);
        } else { // is header
            transform.SetParent(parentAfterDrag);
            SetMaskable(true);
            if (!onWhiteboard) {
                // Debug.Log("header no longer on whiteboard");
                UpdateBlockPositions(this.gameObject, initialPos);
            }
        }
        DesignerController.Instance.justSaved = false;
    }

    public void SetValue() {
        BlockData blockData = this.gameObject.GetComponent<BlockData>();
        if (blockData.values.Count == 0) {
            blockData.values.Add(inputField.GetComponent<TMP_InputField>().text);
        } else {
            blockData.values[0] = inputField.GetComponent<TMP_InputField>().text;
        }

        // if (blockData.property == BlockData.Property.NAME) {
        //     GameObject.Find("NamePreview").GetComponent<TMP_Text>().text = inputField.GetComponent<TMP_InputField>().text + ":";
        // }
    }

    public void SetInputFieldValue(string val) {
        inputField.GetComponent<TMP_InputField>().text = val;
    }

    public void SetDropdownValue(string val, int childIndex) {
        transform.GetChild(childIndex).gameObject.GetComponent<TMP_Dropdown>().value = int.Parse(val);
    }

    public void SetNextBlock(GameObject nextBlock) {
        this.nextBlock = nextBlock;
        // if (nextBlock != null) {
        //     Debug.Log(this.gameObject.name + " has next " + nextBlock.name);
        // } else {
        //     Debug.Log("next block set to null");
        // }
    }

    public void SetPrevBlock(GameObject prevBlock) {
        this.prevBlock = prevBlock;
    }

    public GameObject GetNextBlock() {
        return this.nextBlock;
    }

    public GameObject GetPrevBlock() {
        return this.prevBlock;
    }

    public void SetMaskable(bool value) {
        image.maskable = value;
        image.raycastTarget = value;
        text.maskable = value;
        overlapBox.raycastTarget = value;

        //input field
        if (inputField != null) {
            inputField.GetComponent<Image>().raycastTarget = value;
            // inputField.GetComponent<Image>().maskable = value;
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
        // Dropdown One
        if (dropdownOne != null) {
            // Debug.Log("setting values of dropdown one to " + value);
            dropdownOne.GetComponent<Image>().raycastTarget = value;
            // dropdownOne.GetComponent<Image>().maskable = value;
            // GameObject textArea = dropdownOne.transform.GetChild(0).gameObject;
            int count = dropdownOne.transform.childCount;
            dropdownOne.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().raycastTarget = value;
            // dropdownOne.transform.GetChild(0).gameObject.GetComponent<Image>().raycastTarget = value;
            dropdownOne.transform.GetChild(1).gameObject.GetComponent<Image>().raycastTarget = value;
        }
        // Dropdown Two
        if (dropdownTwo != null) {
            // Debug.Log("setting values of dropdown two to " + value);
            dropdownTwo.GetComponent<Image>().raycastTarget = value;
            // dropdownOne.GetComponent<Image>().maskable = value;
            // GameObject textArea = dropdownOne.transform.GetChild(0).gameObject;
            int count = dropdownTwo.transform.childCount;
            dropdownTwo.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().raycastTarget = value;
            // dropdownTwo.transform.GetChild(0).gameObject.GetComponent<Image>().raycastTarget = value;
            dropdownTwo.transform.GetChild(1).gameObject.GetComponent<Image>().raycastTarget = value;
        }
    }

    private void SetBlockRaycasts(bool value) {
        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = value;
        canvasGroup.ignoreParentGroups = value;
    }

    private bool MouseOverWhiteboard() {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        for (int i = 0; i < raycastResults.Count; i++) {
            // Debug.Log(raycastResults[i].gameObject.name);
            if (raycastResults[i].gameObject.tag != "whiteboard") { // ui layer
                raycastResults.RemoveAt(i);
                i--;
            }
        }
        return raycastResults.Count > 0;
    }

    private GameObject MouseOverOverlapBox() {
        // Debug.Log("testing overlap");
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        foreach (RaycastResult raycastResult in raycastResults) {
            Debug.Log("hit " + raycastResult.gameObject.transform.parent.name);
        }
        for (int i = 0; i < raycastResults.Count; i++) {
            // Debug.Log(raycastResults[i].gameObject.name);
            if (raycastResults[i].gameObject.tag != "overlapBox") { // ui layer
                raycastResults.RemoveAt(i);
                i--;
            }
        }
        foreach (RaycastResult raycastResult in raycastResults) {
            Debug.Log("hit overlap box of " + raycastResult.gameObject.transform.parent.name);
        }
        if (raycastResults.Count == 0) {
            return null;
        } else {
            return raycastResults[0].gameObject.transform.parent.gameObject;
        }
    }

    public void SetBlockOffset(bool shiftBack) {
        // shift back if placing an ELSE, END IF, or END LOOP
        if (shiftBack) {
            blockOffset = new Vector3(50, gameObject.GetComponent<RectTransform>().rect.height, 0);
            return;
        }
        // set x val if FOR, WHILE, IF, or ELSE
        BlockData.BehaviorType blockBehavior = this.gameObject.GetComponent<BlockData>().behavior;
        if (blockBehavior == BlockData.BehaviorType.FOR_LOOP || blockBehavior == BlockData.BehaviorType.WHILE_LOOP || blockBehavior == BlockData.BehaviorType.IF || blockBehavior == BlockData.BehaviorType.ELSE) {
            blockOffset = new Vector3(-50, gameObject.GetComponent<RectTransform>().rect.height, 0);
        } else {
            blockOffset = new Vector3(0, gameObject.GetComponent<RectTransform>().rect.height, 0);
        }
    }

    // boolean, returns false if didn't snap
    private bool SnapToBlock(PointerEventData eventData) {
        // List<RaycastResult> result = new List<RaycastResult>();
        // EventSystem.current.RaycastAll(eventData.pointer)
        if (eventData.pointerCurrentRaycast.gameObject.tag == "overlapSpace") {
            GameObject blockToSnapTo = eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject;
        // GameObject blockToSnapTo = MouseOverOverlapBox();
        // if (MouseOverOverlapBox() != null) {
            if (blockToSnapTo.GetComponent<Draggable>().GetNextBlock() == null) {
                // Debug.Log("snapping!");
                onWhiteboard = true; // make sure still set to true to snap

                if (this.prevBlock != null) {
                    // Debug.Log("setting " + this.prevBlock.name + " next to null");
                    this.prevBlock.GetComponent<Draggable>().SetNextBlock(null);
                }

                this.prevBlock = blockToSnapTo;
                prevBlock.GetComponent<Draggable>().SetNextBlock(this.gameObject);

                BlockData.BehaviorType thisBlockBehavior = gameObject.GetComponent<BlockData>().behavior;
                if (thisBlockBehavior == BlockData.BehaviorType.END_LOOP || thisBlockBehavior == BlockData.BehaviorType.END_IF || thisBlockBehavior == BlockData.BehaviorType.ELSE) {
                    prevBlock.GetComponent<Draggable>().SetBlockOffset(true);
                } else {
                    prevBlock.GetComponent<Draggable>().SetBlockOffset(false);
                }
                UpdateBlockPositions(this.gameObject, prevBlock.transform.position - prevBlock.GetComponent<Draggable>().blockOffset);
                // Debug.Log(this.prevBlock.name);
                return true; // snapped 
            }
        }
        return false; // didn't snap
    }
}
