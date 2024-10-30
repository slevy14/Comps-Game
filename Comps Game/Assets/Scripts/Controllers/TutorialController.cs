using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour {

    public int currentTutorialIndex = 0;
    public bool inTutorial = false;

    [Header("Dialog Advancing")]
    public float dialogAdvanceDelay;
    private float currentDialogTime;
    private bool skippedDialog;

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
    }

    void Update() {
        if (inTutorial) {
            currentDialogTime += Time.deltaTime;
        }

        if (inTutorial && CanAdvanceDialog() && IsValidTutorialIndex() && (Input.GetMouseButtonDown(0))) {
            Debug.Log("running tutorial at index " + currentTutorialIndex + ", advancing to next step of tutorial");
            ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].tutorialFunctionality.RunTutorialFunction(currentTutorialIndex);
            currentTutorialIndex++;
        }

        if (inTutorial && !IsValidTutorialIndex()) {
            Debug.Log("tutorial index " + currentTutorialIndex + " invalid");
            inTutorial = false;
        }
    }

    private bool IsValidTutorialIndex() {
        int count = ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].tutorialFunctionality.tutorialListItems.Count;
        return currentTutorialIndex < count;
    }

    public bool CanAdvanceDialog() {
        if (currentDialogTime >= dialogAdvanceDelay || skippedDialog) {
            currentDialogTime = 0;
            skippedDialog = false;
            return true;
        } else {
            SkipDialog();
        }
        return false;
    }

    private void SkipDialog() {
        // FIXME: skip thru dialog if button pressed before delay time
        skippedDialog = true;
    }

    public void StartTutorial() {
        Debug.Log("started tutorial");
        ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].tutorialFunctionality.InitializeLookup();
        currentTutorialIndex = 0;
        inTutorial = true;
    }


}
