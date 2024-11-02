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

    [Header("Tutorial GameObjects")]
    [SerializeField] private GameObject highlight;
    [SerializeField] private GameObject drawerHighlight;
    [SerializeField] private GameObject tutorialMask;
    [SerializeField] private GameObject bear;
    [SerializeField] private TMP_Text talkingTextBox;

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
    }

    void Update() {
        if (inTutorial) {
            currentDialogTime += Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0)) {
            if (inTutorial && CanAdvanceDialog()) {
                NextStep();
            }
        }
    }

    private void NextStep() {
        if (IsValidTutorialIndex()) {
            // Debug.Log("running tutorial at index " + currentTutorialIndex + ", advancing to next step of tutorial");
            ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].tutorialFunctionality.RunTutorialFunction(currentTutorialIndex);
            talkingTextBox.text = ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].tutorialFunctionality.tutorialListItems[currentTutorialIndex].tutorialDialog;
            currentTutorialIndex++;
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

        if (currentDialogTime >= dialogAdvanceDelay || skippedDialog) {
            currentDialogTime = 0;
            skippedDialog = false;
            Debug.Log("can advance");
            return true;
        } else {
            SkipDialog();
        }
        return false;
    }

    private void SkipDialog() {
        // FIXME: skip thru dialog if button pressed before delay time
        Debug.Log("skipping dialog");
        skippedDialog = true;
    }

    public void StartTutorial() {
        Debug.Log("started tutorial");
        ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].tutorialFunctionality.InitializeLookup();
        ToggleTutorialUIObjects(true);
        currentTutorialIndex = 0;
        inTutorial = true;
        NextStep();
    }

    public void EndTutorial() {
        inTutorial = false;
        ToggleTutorialUIObjects(false);
    }

    private void ToggleTutorialUIObjects(bool value) {
        // Debug.Log("toggling ui " + value);
        bear.SetActive(value);
        tutorialMask.SetActive(value);
        talkingTextBox.transform.parent.gameObject.SetActive(value);
        drawerHighlight.SetActive(value);

        HideHighlight();
        highlight.SetActive(value);
    }

    public void MoveBear(Vector2 bearPos, bool faceLeft) {
        // set bear pos
        Vector2 bearPrevPos = bear.GetComponent<RectTransform>().anchoredPosition;
        bear.GetComponent<RectTransform>().anchoredPosition = bearPos;

        // set text box position
        Debug.Log("textbox offset is " + (bearPos - bearPrevPos));
        Vector2 newTextboxPosition = talkingTextBox.transform.parent.GetComponent<RectTransform>().anchoredPosition + (bearPos - bearPrevPos);

        if (faceLeft && bear.GetComponent<RectTransform>().eulerAngles.y != 180) {
            bear.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 180, 0);
            Debug.Log("new x before shift is " + newTextboxPosition.x);
            newTextboxPosition.x = newTextboxPosition.x - (-bear.GetComponent<RectTransform>().sizeDelta.x - talkingTextBox.transform.parent.GetComponent<RectTransform>().sizeDelta.x);
            Debug.Log("facing left, shifting back by " + (bear.GetComponent<RectTransform>().sizeDelta.x - talkingTextBox.transform.parent.GetComponent<RectTransform>().sizeDelta.x));
            Debug.Log("new x after shift is " + newTextboxPosition.x);
        } else if (!faceLeft) {
            bear.GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0);
        }
        Debug.Log("new textbox position: " + newTextboxPosition);
        talkingTextBox.transform.parent.GetComponent<RectTransform>().anchoredPosition = newTextboxPosition;
    }


    public void MoveHighlight(Vector2 highlightPos) {
        highlight.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        highlight.GetComponent<RectTransform>().anchoredPosition = highlightPos;
    }

    public void HideHighlight() {
        highlight.GetComponent<RectTransform>().anchoredPosition = new Vector2(960, 540);
        highlight.GetComponent<RectTransform>().localScale = new Vector3(0.001f, 0.001f, 0.001f);

        drawerHighlight.SetActive(false);
    }

    public void ToggleDrawerHighlight(bool value) {
        drawerHighlight.SetActive(value);
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
