using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialController : MonoBehaviour {

    public int currentTutorialIndex = 0;
    public bool inTutorial = false;

    [Header("Dialog Advancing")]
    public float dialogAdvanceDelay;
    private float currentDialogTime;
    private bool skippedDialog;
    private bool inTransition;

    [Header("Highlights")]
    [SerializeField] private GameObject highlight;
    [SerializeField] private GameObject drawerHighlight;
    [SerializeField] private GameObject whiteboardHighlight;
    [SerializeField] private GameObject strengthHighlight;
    [SerializeField] private GameObject saveHighlight;
    [SerializeField] private GameObject levelDataHighlight;
    [SerializeField] private GameObject battleSpeedHighlight;
    [SerializeField] private GameObject spriteDropdownHighlight;
    [SerializeField] private GameObject toLevelHighlight;

    [Header("Tutorial GameObjects")]
    [SerializeField] private GameObject tutorialMask;
    [SerializeField] private GameObject bear;
    [SerializeField] private TMP_Text talkingTextBox;
    [SerializeField] private GameObject blockShowingParent;

    [Header("Other")]
    [SerializeField] private List<int> unlockedPropertyBlocks;
    [SerializeField] private List<int> unlockedBehaviorBlocks;
    private Vector2 bearTextboxOffset;

    // SINGLETON
    public static TutorialController Instance = null;

    public void CheckSingleton() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void Awake() {
        CheckSingleton();
        ToggleTutorialUIObjects(false);
        dialogAdvanceDelay = 1f;
        unlockedPropertyBlocks = new();
        unlockedBehaviorBlocks = new();
        bearTextboxOffset = talkingTextBox.transform.parent.GetComponent<RectTransform>().anchoredPosition - bear.GetComponent<RectTransform>().anchoredPosition;
    }

    // void Update() {
    //     if (inTutorial) {
    //         currentDialogTime += Time.deltaTime;
    //     }

    //     if (Input.GetMouseButtonDown(0) && !PauseMenuController.Instance.isPaused) {
    //         if (inTutorial && CanAdvanceDialog()) {
    //             NextStep();
    //         }
    //     }
    // }



    public void ResetTutorialStates() {
        // destroy any showing blocks
        if (blockShowingParent.transform.childCount != 0) {
            foreach (Transform child in blockShowingParent.transform) {
                Destroy(child.gameObject);
            }
        } else {
            Debug.Log("no children to erase");
        }
    }

    public void NextStep() {
        if (IsValidTutorialIndex()) {
            // Debug.Log("running tutorial at index " + currentTutorialIndex + ", advancing to next step of tutorial");
            ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].tutorialFunctionality.RunTutorialFunction(currentTutorialIndex);
            talkingTextBox.text = ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].tutorialFunctionality.tutorialListItems[currentTutorialIndex].tutorialDialog;
            currentTutorialIndex++;
            Debug.Log("progressing to next text");
        } else {
            EndTutorial();
        }
    }

    private bool IsValidTutorialIndex() {
        int count = ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].tutorialFunctionality.tutorialListItems.Count;
        return currentTutorialIndex < count;
    }

    public bool CanAdvanceDialog() {
        if (inTransition) {
            Debug.Log("in transition");
            return false;
        }

        // if (currentDialogTime >= dialogAdvanceDelay || skippedDialog) {
        //     currentDialogTime = 0;
        //     skippedDialog = false;
        //     Debug.Log("can advance");
        //     return true;
        // } else {
        //     SkipDialog();
        // }
        return true;
    }

    private void SkipDialog() {
        // FIXME: skip thru dialog if button pressed before delay time
        Debug.Log("skipping dialog");
        skippedDialog = true;
    }

    private void GetUnlockedBlockIndices() {
        // get all property blocks unlocked thus far, not including the new ones in this level
        for (int i = 1; i < ProgressionController.Instance.currentLevel; i++) {
            foreach (int index in ProgressionController.Instance.levelDataList[i].availablePropertiesIndices) {
                if (!unlockedPropertyBlocks.Contains(index)) {
                    unlockedPropertyBlocks.Add(index);
                }
            }
        }
        // get all behavior blockes unlocked thus far, not including the new ones in this level
        for (int i = 1; i < ProgressionController.Instance.currentLevel; i++) {
            foreach (int index in ProgressionController.Instance.levelDataList[i].availableBehaviorsIndices) {
                if (!unlockedBehaviorBlocks.Contains(index)) {
                    unlockedBehaviorBlocks.Add(index);
                }
            }
        }
    }

    public void StartTutorial() {
        Debug.Log("started tutorial");
        ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].tutorialFunctionality.InitializeLookup();
        ToggleTutorialUIObjects(true);
        currentTutorialIndex = 0;
        inTutorial = true;
        ResetBearAndTextboxPositions();
        NextStep();
    }

    public void EndTutorial() {
        ResetTutorialStates();
        inTutorial = false;
        ToggleTutorialUIObjects(false);
    }

    private void ToggleTutorialUIObjects(bool value) {
        // Debug.Log("toggling ui " + value);
        bear.SetActive(value);
        tutorialMask.SetActive(value);
        talkingTextBox.transform.parent.gameObject.SetActive(value);
        drawerHighlight.SetActive(value);
        blockShowingParent.SetActive(value);

        HideHighlight();
        highlight.SetActive(value);
    }

    public void ResetBearAndTextboxPositions() {
        MoveBear(new Vector2(-675, -405), false);
        talkingTextBox.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(-146.86f, -261.00f);
        Debug.Log(talkingTextBox.transform.parent.GetComponent<RectTransform>().anchoredPosition);
    }

    public void MoveBear(Vector2 bearPos, bool faceLeft) {
        // set bear pos
        Vector2 bearPrevPos = bear.GetComponent<RectTransform>().anchoredPosition;
        bear.GetComponent<RectTransform>().anchoredPosition = bearPos;

        // set text box position
        // Debug.Log("textbox offset is " + (bearPos - bearPrevPos));
        // Vector2 newTextboxPosition = talkingTextBox.transform.parent.GetComponent<RectTransform>().anchoredPosition + (bearPos - bearPrevPos);

        if (faceLeft && bear.GetComponent<RectTransform>().eulerAngles.y != 180) {
            bear.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 180, 0);
            // Debug.Log("new x before shift is " + newTextboxPosition.x);
            // newTextboxPosition.x = newTextboxPosition.x - (-bear.GetComponent<RectTransform>().sizeDelta.x - talkingTextBox.transform.parent.GetComponent<RectTransform>().sizeDelta.x);
            // Debug.Log("facing left, shifting back by " + (bear.GetComponent<RectTransform>().sizeDelta.x - talkingTextBox.transform.parent.GetComponent<RectTransform>().sizeDelta.x));
            // Debug.Log("new x after shift is " + newTextboxPosition.x);
            talkingTextBox.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(bearPos.x - bearTextboxOffset.x, bearPos.y + bearTextboxOffset.y);
        } else if (!faceLeft) {
            bear.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0);
            talkingTextBox.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(bearPos.x + bearTextboxOffset.x, bearPos.y + bearTextboxOffset.y);
        }
        // Debug.Log("new textbox position: " + newTextboxPosition);
        // talkingTextBox.transform.parent.GetComponent<RectTransform>().anchoredPosition = newTextboxPosition;
    }


    public void MoveHighlight(Vector2 highlightPos) {
        highlight.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        highlight.GetComponent<RectTransform>().anchoredPosition = highlightPos;
    }

    public void HideHighlight() {
        highlight.GetComponent<RectTransform>().anchoredPosition = new Vector2(960, 540);
        highlight.GetComponent<RectTransform>().localScale = new Vector3(0.001f, 0.001f, 0.001f);

        drawerHighlight.SetActive(false);
        whiteboardHighlight.SetActive(false);
        strengthHighlight.SetActive(false);
        saveHighlight.SetActive(false);
        battleSpeedHighlight.SetActive(false);
        levelDataHighlight.SetActive(false);
        spriteDropdownHighlight.SetActive(false);
        toLevelHighlight.SetActive(false);
    }

    public void ToggleHighlight(string highlight, bool value) {
        HideHighlight();
        switch(highlight) {
            case "drawer":
                drawerHighlight.SetActive(value);
                break;
            case "whiteboard":
                whiteboardHighlight.SetActive(value);
                break;
            case "save":
                saveHighlight.SetActive(value);
                break;
            case "strength":
                strengthHighlight.SetActive(value);
                break;
            case "battleSpeed":
                battleSpeedHighlight.SetActive(value);
                break;
            case "levelData":
                levelDataHighlight.SetActive(value);
                break;
            case "spriteDropdown":
                spriteDropdownHighlight.SetActive(value);
                break;
            case "toLevel":
                toLevelHighlight.SetActive(value);
                break;
        }
    }

    public void ShowNewLevelBlocks() {
        GetUnlockedBlockIndices();
        foreach (int propertyIndex in ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].availablePropertiesIndices) {
            if (!unlockedPropertyBlocks.Contains(propertyIndex)) {
                Instantiate(DesignerController.Instance.GetPropertyBlocks()[propertyIndex], Vector3.zero, transform.rotation, blockShowingParent.transform);
            }
        }
        foreach (int behaviorIndex in ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].availableBehaviorsIndices) {
            if (!unlockedBehaviorBlocks.Contains(behaviorIndex)) {
                Instantiate(DesignerController.Instance.GetBehaviorBlocks()[behaviorIndex], Vector3.zero, transform.rotation, blockShowingParent.transform);
            }
        }
    }

    public void TutorialChangeSceneWithDelay(string name) {
        SceneController.Instance.LoadSceneByName(name);
        HideHighlight();
        StartCoroutine(SceneChangeDelay());
    }

    private IEnumerator SceneChangeDelay() {
        inTransition = true;
        yield return new WaitForSeconds(2f);
        inTransition = false;
    }


}
