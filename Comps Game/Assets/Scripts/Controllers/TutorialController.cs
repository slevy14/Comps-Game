using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialController : MonoBehaviour {

    // placed on tutorial controller object
    // holds references to all objects related to the tutorials

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


    public void ResetTutorialStates() {
        // destroy any showing blocks
        if (blockShowingParent.transform.childCount != 0) {
            foreach (Transform child in blockShowingParent.transform) {
                Destroy(child.gameObject);
            }
        }
    }

    public void NextStep() {
        // if tutorial should keep going, run next function
        if (IsValidTutorialIndex()) {
            ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].tutorialFunctionality.RunTutorialFunction(currentTutorialIndex);
            talkingTextBox.text = ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].tutorialFunctionality.tutorialListItems[currentTutorialIndex].tutorialDialog;
            currentTutorialIndex++;
        } else { // otherwise, end tutorial
            EndTutorial();
        }
    }

    private bool IsValidTutorialIndex() {
        // check that tutorial should keep going
        int count = ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].tutorialFunctionality.tutorialListItems.Count;
        return currentTutorialIndex < count;
    }

    public bool CanAdvanceDialog() {
        // dialog can be advanced if not in transition
        if (inTransition) {
            return false;
        }
        return true;
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
        // get all behavior blocks unlocked thus far, not including the new ones in this level
        for (int i = 1; i < ProgressionController.Instance.currentLevel; i++) {
            foreach (int index in ProgressionController.Instance.levelDataList[i].availableBehaviorsIndices) {
                if (!unlockedBehaviorBlocks.Contains(index)) {
                    unlockedBehaviorBlocks.Add(index);
                }
            }
        }
    }

    public void StartTutorial() {
        // initialize data to start new level tutorial
        ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].tutorialFunctionality.InitializeLookup();
        ToggleTutorialUIObjects(true);
        currentTutorialIndex = 0;
        inTutorial = true;
        ResetBearAndTextboxPositions();
        NextStep();
    }

    public void EndTutorial() {
        // reset all tutorial data, hide anything shown
        ResetTutorialStates();
        inTutorial = false;
        ToggleTutorialUIObjects(false);
    }

    private void ToggleTutorialUIObjects(bool value) {
        // activate or deactivate all highlights, bear, etc.
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
    }

    public void MoveBear(Vector2 bearPos, bool faceLeft) {
        // set bear pos
        bear.GetComponent<RectTransform>().anchoredPosition = bearPos;

        // set text box position
        // flip bear and text box if needed
        if (faceLeft) {
            bear.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 180, 0);
            talkingTextBox.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(bearPos.x - bearTextboxOffset.x, bearPos.y + bearTextboxOffset.y);
        } else if (!faceLeft) {
            bear.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0);
            talkingTextBox.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(bearPos.x + bearTextboxOffset.x, bearPos.y + bearTextboxOffset.y);
        }
        Debug.Log("moved bear and textbox");
    }


    public void MoveHighlight(Vector2 highlightPos) {
        // move highlight to position
        highlight.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        highlight.GetComponent<RectTransform>().anchoredPosition = highlightPos;
    }

    public void HideHighlight() {
        // disable all highlights
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
        // hide previous highlights
        HideHighlight();
        // show chosen highlight
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
        // loop through all behavior and property blocks not previously seen and show them to the player
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
        HideHighlight();
    }

    // change scene with a delay to prevent skipping through text prematurely
    public void TutorialChangeSceneWithDelay(string name) {
        SceneController.Instance.LoadSceneByName(name);
        HideHighlight();
        StartCoroutine(SceneChangeDelay());
    }

    private IEnumerator SceneChangeDelay() {
        inTransition = true;
        yield return new WaitForSeconds(1.25f);
        inTransition = false;
    }


}
