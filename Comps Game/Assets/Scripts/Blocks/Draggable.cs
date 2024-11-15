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
    // private GameObject inputField;
    // private GameObject dropdownOne;
    // private GameObject dropdownTwo;

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
        image = this.gameObject.GetComponent<Image>();
        text = this.transform.GetChild(0).GetComponent<TMP_Text>();
        overlapBox = this.transform.GetChild(1).GetComponent<Image>();
        SetBlockOffset(false);
        // Debug.Log("offset: " + blockOffset.y);
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
            
            if (transform.childCount >= 3) {
                if (this.transform.GetChild(2).GetComponent<Slider>()) { // if has slider
                    SetValueFromSlider();
                } else if (this.transform.GetChild(2).GetComponent<TMP_InputField>()) { // if has input field
                    SetValueFromInputField();
                }
            }
        }
        // transform.SetAsLastSibling();
        // image.raycastTarget = false;

        // come back to this -- not good practice to find by name!
        // TMP_Text namePreview = GameObject.Find("NamePreview").GetComponent<TMP_Text>();
    }


    public void OnBeginDrag(PointerEventData eventData) {
        // check if actually over object, not including overlap box
        List<RaycastResult> uiUnderMouseRaycasts = HelperController.Instance.OverUI();
        List<GameObject> uiUnderMouseObjects = new();
        foreach (RaycastResult raycastResult in uiUnderMouseRaycasts) {
            uiUnderMouseObjects.Add(raycastResult.gameObject);
        }
        if (!uiUnderMouseObjects.Contains(this.gameObject)) {
            Debug.Log("shouldn't drag!");
            eventData.pointerDrag = null;
            return;
        }
    
        onWhiteboard = false;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        SetMaskable(false);

        if (prevBlock != null) {
            prevBlock.GetComponent<Draggable>().SetNextBlock(null); // reset next block on previous
            prevBlock.GetComponent<Draggable>().SetOverlapUseable();
            DesignerController.Instance.UpdateStrengthDisplay();
            DesignerController.Instance.UpdateBehaviorsDisplay();
            // Debug.Log("updated prev block next");
            prevBlock = null;
        }
        initialPos = transform.position;

        AudioController.Instance.PlaySoundEffect("Block Pickup");
    }

    public void OnDrag(PointerEventData eventData) {
        UpdateBlockPositions(this.gameObject, Input.mousePosition);

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
            // Debug.Log("next block not null");
            nextBlock.GetComponent<Draggable>().UpdateBlockPositions(nextBlock, newPosition - blockOffset);
        }
    }

    public void SetOverlapUseable() {
        if (nextBlock != null) {
            overlapBox.gameObject.SetActive(false);
        } else {
            overlapBox.gameObject.SetActive(true);
        }
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

        GameObject whiteboard = OverWhiteboard(eventData);
        if (whiteboard != null) {
            onWhiteboard = true;
            parentAfterDrag = whiteboard.transform;
        }

        if (!isHeader) {
            // Debug.Log("enddrag called");

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
                // prevBlock.GetComponent<Draggable>().nextBlock = null; // reset next block on previous
                AudioController.Instance.PlaySoundEffect("Block Drop");
                // Debug.Log("not snapping");
            }

            transform.SetParent(parentAfterDrag);
            // Debug.Log("set parent to " + parentAfterDrag.name);
            SetMaskable(true); 
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
        GameObject blockToSnapTo = DesignerController.Instance.FindBlockToSnapTo(eventData, this.transform);

        if (blockToSnapTo != null && !isHeader) {
            // GameObject blockToSnapTo = eventData.pointerCurrentRaycast.gameObject.transform.parent.gameObject;
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
                DesignerController.Instance.ToggleSnappingIndicator(null, gameObject.GetComponent<RectTransform>()); // disable the snapping indicator
                DesignerController.Instance.UpdateStrengthDisplay();
                DesignerController.Instance.UpdateBehaviorsDisplay();
                // Debug.Log(this.prevBlock.name);
                AudioController.Instance.PlaySoundEffect("Block Snap");
                return true; // snapped 
            }
        }
        return false; // didn't snap
    }

    
    private GameObject OverWhiteboard(PointerEventData eventData) {
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        // Debug.Log("hit " + raycastResults.Count + " objects: ");
        for (int i = 0; i < raycastResults.Count; i++) {
            // Debug.Log("found object with tag" + raycastResults[i].gameObject.tag);
            if (raycastResults[i].gameObject.tag == "whiteboard") {
                // Debug.Log("over whiteboard!");
                return raycastResults[i].gameObject;
            }
            // Debug.Log(i);
        }
        // Debug.Log("not over whiteboard!");
        return null;
    }
}
