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

    // references to prev and next block used as doubly linked list
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
        // set object references
        image = this.gameObject.GetComponent<Image>();
        text = this.transform.GetChild(0).GetComponent<TMP_Text>();
        overlapBox = this.transform.GetChild(1).GetComponent<Image>();
        SetBlockOffset(false);

        // if this isn't a header, update parenting and data setters
        if (!isHeader) {
            parentAfterDrag = transform.parent;
            onWhiteboard = false;
            transform.SetAsLastSibling();
            SetMaskable(false);
            
            if (transform.childCount >= 3) {
                if (this.transform.GetChild(2).GetComponent<Slider>()) { // if has slider
                    SetValueFromSlider();
                } else if (this.transform.GetChild(2).GetComponent<TMP_InputField>()) { // if has input field
                    SetValueFromInputField();
                }
            }
        }
    }

    void Start() {
        // prevent enemies from being edited
        if (DesignerController.Instance.isCurrentWarriorEnemy) {
            SetInteractable(false);
        }
    }


    public void OnBeginDrag(PointerEventData eventData) {
        // check if actually over object, not including overlap box
        List<RaycastResult> uiUnderMouseRaycasts = HelperController.Instance.OverUI();
        List<GameObject> uiUnderMouseObjects = new();
        foreach (RaycastResult raycastResult in uiUnderMouseRaycasts) {
            uiUnderMouseObjects.Add(raycastResult.gameObject);
        }
        // don't drag if grabbing overlap space
        if (!uiUnderMouseObjects.Contains(this.gameObject)) {
            Debug.Log("shouldn't drag! grabbing overlap space");
            eventData.pointerDrag = null;
            return;
        }
        // don't drag if currently looking at enemy
        if (DesignerController.Instance.isCurrentWarriorEnemy) {
            Debug.Log("shouldn't drag! is enemy");
            eventData.pointerDrag = null;
            return;
        }
    
        // remove from whiteboard
        onWhiteboard = false;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        SetMaskable(false);

        // remove this object from linked list if connected to another block
        if (prevBlock != null) {
            prevBlock.GetComponent<Draggable>().SetNextBlock(null); // reset next block on previous
            prevBlock.GetComponent<Draggable>().SetOverlapUseable();
            DesignerController.Instance.UpdateStrengthDisplay();
            DesignerController.Instance.UpdateBehaviorsDisplay();
            prevBlock = null;
        }
        initialPos = transform.position;

        // play sound
        AudioController.Instance.PlaySoundEffect("Block Pickup");
    }

    public void OnDrag(PointerEventData eventData) {
        // move this block and attached stack to the mouse
        UpdateBlockPositions(this.gameObject, Input.mousePosition);

        // make blocks able to be overlapped
        DesignerController.Instance.EnableOverlapSpaces();

        // show overlap space if need to
        if (!isHeader) {
            GameObject overlapBlock = DesignerController.Instance.FindBlockToSnapTo(eventData, this.transform);
            DesignerController.Instance.ToggleSnappingIndicator(overlapBlock, gameObject.GetComponent<RectTransform>());
        }
    }

    public void UpdateBlockPositions(GameObject block, Vector3 newPosition) {
        // recursively update positions of each block based on the one before it
        block.transform.position = newPosition;
        SetOverlapUseable();
        if (nextBlock != null) {
            nextBlock.GetComponent<Draggable>().UpdateBlockPositions(nextBlock, newPosition - blockOffset);
        }
    }

    public void SetOverlapUseable() {
        // overlap space useable if doesn't have anything next
        if (nextBlock != null) {
            overlapBox.gameObject.SetActive(false);
        } else {
            overlapBox.gameObject.SetActive(true);
        }
    }

    // overload for direct value setting
    public void SetOverlapUseable(bool value) {
        overlapBox.gameObject.SetActive(value);
    }

    public void DestroyStack(GameObject block) {
        // destroy this block and all blocks after it recursively
        if (this.nextBlock != null) {
            this.nextBlock.GetComponent<Draggable>().DestroyStack(nextBlock);
        }
        Destroy(block);
    }

    public void OnEndDrag(PointerEventData eventData) {
        // check if this block is on whiteboard
        GameObject whiteboard = OverWhiteboard(eventData);
        if (whiteboard != null) {
            onWhiteboard = true;
            parentAfterDrag = whiteboard.transform;
        }

        // if not header, try to place
        if (!isHeader) {
            // if not on whiteboard, delete this block stack and update warrior data
            if (!onWhiteboard && !DesignerController.Instance.FindBlockToSnapTo(eventData, this.transform)) {
                AudioController.Instance.PlaySoundEffect("Delete");
                DesignerController.Instance.UpdateStrengthDisplay();
                DesignerController.Instance.UpdateBehaviorsDisplay();
                DestroyStack(this.gameObject);
            }

            // need to do this here because if instantiated, on awake offset set to 0 for some reason!
            SetBlockOffset(false);

            if (SnapToBlock(eventData)) { // attempt to snap
                parentAfterDrag = whiteboard.transform;
            } else { // didn't snap
                AudioController.Instance.PlaySoundEffect("Block Drop");
            }

            // parent this block to the whiteboard if it can
            transform.SetParent(parentAfterDrag);
            SetMaskable(true); 
        } else { // is header, return to whiteboard
            transform.SetParent(parentAfterDrag);
            SetMaskable(true);
            if (!onWhiteboard) {
                UpdateBlockPositions(this.gameObject, initialPos);
            }
        }

        // prevent overlap spaces from interfering with drag and drop
        DesignerController.Instance.DisableOverlapSpaces();
        // value changed! need to save manually now
        DesignerController.Instance.justSaved = false;
    }

    // SET VALUES TO AND FROM INPUTS
    // update values on both the warrior data and the block

    public void SetValueFromSlider() {
        BlockData blockData = this.gameObject.GetComponent<BlockData>();
        if (blockData.values.Count == 0) {
            blockData.values.Add(Mathf.RoundToInt(transform.GetChild(2).GetComponent<Slider>().value) + "");
        } else {
            blockData.values[0] = Mathf.RoundToInt(transform.GetChild(2).GetComponent<Slider>().value) + "";
        }
    }

    public void SetValueFromInputField() {
        BlockData blockData = this.gameObject.GetComponent<BlockData>();
        if (blockData.values.Count == 0) {
            blockData.values.Add(transform.GetChild(2).GetComponent<TMP_InputField>().text);
        } else {
            blockData.values[0] = transform.GetChild(2).GetComponent<TMP_InputField>().text;
        }
        if (blockData.property == BlockData.Property.NAME) {
            GameObject.Find("NamePreview").GetComponent<TMP_Text>().text = blockData.values[0] + ",";
        }
    }

    public void SetInputFieldValue(string val) {
        TMP_InputField inputField = transform.GetChild(2).GetComponent<TMP_InputField>();
        inputField.GetComponent<TMP_InputField>().text = val;
        SetValueFromInputField();
    }

    public void SetSliderValue(string val) {
        Slider slider = transform.GetChild(2).GetComponent<Slider>();
        slider.value = float.Parse(val);
        slider.GetComponent<PropertySlider>().DynamicUpdateValueText(slider.value);
        SetValueFromSlider();
    }

    public void SetDropdownValue(string val, int childIndex) {
        transform.GetChild(childIndex).gameObject.GetComponent<TMP_Dropdown>().value = int.Parse(val);
    }

    // GET AND SET BLOCKS IN DOUBLY LINKED LIST

    public void SetNextBlock(GameObject nextBlock) {
        this.nextBlock = nextBlock;
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

    // CHANGE BLOCK INTERACTIBILITY

    public void SetMaskable(bool value) {
        // disable components of blocks from interfering with drag and drop
        image.maskable = value;
        image.raycastTarget = value;
        text.maskable = value;
        overlapBox.raycastTarget = value;

        // loop through rest of children and handle cases
        int childCount = this.gameObject.transform.childCount;
        // POSSIBLE CASES: dropdown, slider, input field, text
        for (int i = 2; i < childCount; i++) {
            GameObject child = this.transform.GetChild(i).gameObject;
            // DROPDOWN CASE
            if (child.GetComponent<TMP_Dropdown>()) {
                child.GetComponent<Image>().raycastTarget = value;
                child.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().raycastTarget = value;
                child.transform.GetChild(1).gameObject.GetComponent<Image>().raycastTarget = value;
                continue;
            }
            // INPUT FIELD CASE
            if (child.GetComponent<TMP_InputField>()) {
                child.GetComponent<Image>().raycastTarget = value;
                GameObject textArea = child.transform.GetChild(0).gameObject;
                int count = textArea.transform.childCount;
                for (int j = 0; j < count; j++) {
                    if (textArea.transform.GetChild(i).gameObject.tag == "disableCast") {
                        textArea.transform.GetChild(i).gameObject.GetComponent<TMP_Text>().raycastTarget = value;
                    } else { // caret
                        textArea.transform.GetChild(i).gameObject.GetComponent<TMP_SelectionCaret>().raycastTarget = value;
                    }
                }
                continue;
            }
            // SLIDER CASE
            if (child.GetComponent<Slider>()) {
                child.GetComponent<Slider>().interactable = value;
                child.transform.GetChild(0).gameObject.GetComponent<Image>().raycastTarget = value;
                child.transform.GetChild(0).gameObject.GetComponent<Image>().maskable = value;
                child.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().raycastTarget = value;
                child.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().maskable = value;
                child.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().raycastTarget = value;
                child.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().maskable = value;
                continue;
            }
            // TEXT CASE
            if (child.GetComponent<TMP_Text>()) {
                child.GetComponent<TMP_Text>().maskable = value;
                continue;
            }

        }
    }

    public void SetInteractable(bool value) { // mostly used for enemies
        // disable components of blocks from being interacted with at all
        image.raycastTarget = true; // leave this true to allow tooltips 
        overlapBox.raycastTarget = value;

        // loop through rest of children and handle cases
        int childCount = this.gameObject.transform.childCount;
        // POSSIBLE CASES: dropdown, slider, input field, text
        for (int i = 2; i < childCount; i++) {
            GameObject child = this.transform.GetChild(i).gameObject;
            // DROPDOWN CASE
            if (child.GetComponent<TMP_Dropdown>()) {
                child.GetComponent<Image>().raycastTarget = value;
                child.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().raycastTarget = value;
                child.transform.GetChild(1).gameObject.GetComponent<Image>().raycastTarget = value;
                child.GetComponent<TMP_Dropdown>().interactable = value;
                continue;
            }
            // INPUT FIELD CASE
            if (child.GetComponent<TMP_InputField>()) {
                child.GetComponent<Image>().raycastTarget = value;
                GameObject textArea = child.transform.GetChild(0).gameObject;
                int count = textArea.transform.childCount;
                for (int j = 0; j < count; j++) {
                    if (textArea.transform.GetChild(i).gameObject.tag == "disableCast") {
                        textArea.transform.GetChild(i).gameObject.GetComponent<TMP_Text>().raycastTarget = value;
                    } else { // caret
                        textArea.transform.GetChild(i).gameObject.GetComponent<TMP_SelectionCaret>().raycastTarget = value;
                    }
                }
                child.GetComponent<TMP_InputField>().interactable = value;
                continue;
            }
            // SLIDER CASE
            if (child.GetComponent<Slider>()) {
                child.GetComponent<Slider>().interactable = value;
                child.transform.GetChild(0).gameObject.GetComponent<Image>().raycastTarget = value;
                child.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().raycastTarget = value;
                child.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().raycastTarget = value;
                continue;
            }
            // TEXT CASE
            if (child.GetComponent<TMP_Text>()) {
                continue;
            }

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
        // find a block to snap to
        GameObject blockToSnapTo = DesignerController.Instance.FindBlockToSnapTo(eventData, this.transform);

        // if found a block to snap to (and this block is not a header), snap!
        if (blockToSnapTo != null && !isHeader) {
            // confirm that previous block does not have a next
            if (blockToSnapTo.GetComponent<Draggable>().GetNextBlock() == null) {
                onWhiteboard = true; // make sure still set to true to snap

                // update linked list references
                if (this.prevBlock != null) {
                    this.prevBlock.GetComponent<Draggable>().SetNextBlock(null);
                }

                this.prevBlock = blockToSnapTo;
                prevBlock.GetComponent<Draggable>().SetNextBlock(this.gameObject);

                // snap to correct position, based on offsets
                BlockData.BehaviorType thisBlockBehavior = gameObject.GetComponent<BlockData>().behavior;
                if (thisBlockBehavior == BlockData.BehaviorType.END_LOOP || thisBlockBehavior == BlockData.BehaviorType.END_FOR || thisBlockBehavior == BlockData.BehaviorType.END_IF || thisBlockBehavior == BlockData.BehaviorType.ELSE) {
                    prevBlock.GetComponent<Draggable>().SetBlockOffset(true);
                } else {
                    prevBlock.GetComponent<Draggable>().SetBlockOffset(false);
                }
                UpdateBlockPositions(this.gameObject, prevBlock.transform.position - prevBlock.GetComponent<Draggable>().blockOffset);
                DesignerController.Instance.ToggleSnappingIndicator(null, gameObject.GetComponent<RectTransform>()); // disable the snapping indicator

                // update strength and behavior counts
                DesignerController.Instance.UpdateStrengthDisplay();
                DesignerController.Instance.UpdateBehaviorsDisplay();
                
                // play sound
                AudioController.Instance.PlaySoundEffect("Block Snap");

                return true; // snapped 
            }
        }
        return false; // didn't snap
    }

    
    private GameObject OverWhiteboard(PointerEventData eventData) {
        // check over whiteboard
        // loop through all objects under mouse
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        for (int i = 0; i < raycastResults.Count; i++) {
            if (raycastResults[i].gameObject.tag == "whiteboard") {
                // return whiteboard object if over it
                return raycastResults[i].gameObject;
            }
        }

        // not over whiteboard
        return null;
    }
}
