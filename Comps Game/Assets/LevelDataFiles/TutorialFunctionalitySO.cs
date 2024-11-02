using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Objects/TutorialFunctionality")]
public class TutorialFunctionalitySO : ScriptableObject {

    [SerializeField] public List<TutorialListItem> tutorialListItems;

    [System.Serializable]
    // infuriatingly, these are in no specific order. if you try to change them unity will kill you
    public enum FunctionOptions {
        HighlightWarriorDrawer = 0,
        HighlightBlocksDrawer = 1,
        HighlightEnemyDrawer = 2,
        LoadFirstWarrior = 3,
        LoadLastWarrior = 4,
        LoadFirstEnemy = 5,
        ShowArrow = 6,
        None = 7,
        HighlightBlocksArea = 8,
        HighlightSaveButton = 9,
        HighlightStrength = 10,
        Highlight = 11,
        SwitchToLevelScene = 12,
        SwitchToCodingEditor = 13,
        HighlightDrawer = 14,
        HighlightLevelOneEnemy = 15,
        HighlightPlayerFirstWarrior = 16
    };

    [SerializeField]
    private FunctionOptions _selectedFunction;
    private Dictionary<FunctionOptions, System.Action> _functionLookup;


    void Awake() {
        InitializeLookup();
    }

    public void InitializeLookup() {
        _functionLookup = new Dictionary<FunctionOptions, System.Action>()
        {
            { FunctionOptions.HighlightWarriorDrawer, HighlightWarriorDrawer },
            { FunctionOptions.HighlightBlocksDrawer, HighlightBlocksDrawer },
            { FunctionOptions.HighlightEnemyDrawer, HighlightEnemyDrawer },
            { FunctionOptions.LoadFirstWarrior, LoadFirstWarrior },
            { FunctionOptions.LoadLastWarrior, LoadLastWarrior },
            { FunctionOptions.LoadFirstEnemy, LoadFirstEnemy },
            { FunctionOptions.ShowArrow, ShowArrow },
            { FunctionOptions.None, None },
            { FunctionOptions.SwitchToLevelScene, SwitchToLevelScene },
            { FunctionOptions.SwitchToCodingEditor, SwitchToCodeEditor },
            { FunctionOptions.HighlightDrawer, HighlightDrawer },
            { FunctionOptions.HighlightLevelOneEnemy, HighlightLevelOneEnemy },
            { FunctionOptions.HighlightPlayerFirstWarrior, HighlightPlayerFirstWarrior }
        };
    }

    // public void SetSelectedFunction(FunctionOption function) {
    //     _selectedFunction = function;
    // }

    public void RunTutorialFunction(int index) {
        // _selectedFunction = tutorialListItems[index].functionOption;
        // ActivateSelectedFunction();
        Debug.Log("TUTORIAL DIALOG: " + tutorialListItems[index].tutorialDialog);
        // Debug.Log(_functionLookup);
        _functionLookup[tutorialListItems[index].functionOption].Invoke();
    }


    public void ActivateSelectedFunction() {
        _functionLookup[_selectedFunction].Invoke();
    }


    private void HighlightWarriorDrawer() {
        Debug.Log("TUTORIAL FUNC: " + "HighlightWarriorsDrawer");
        TutorialController.Instance.MoveHighlight(new Vector2(-612, 178));
        TutorialController.Instance.MoveBear(new Vector2(696, -375), true);
    }

    private void HighlightBlocksDrawer() {
        Debug.Log("TUTORIAL FUNC: " + "HighlightBlocksDrawer");
        TutorialController.Instance.MoveHighlight(new Vector2(-843, 178));
        TutorialController.Instance.MoveBear(new Vector2(696, -375), true);
    }

    private void HighlightEnemyDrawer() {
        Debug.Log("TUTORIAL FUNC: " + "HighlightEnemyDrawer");
        TutorialController.Instance.MoveHighlight(new Vector2(-374, 178));
        TutorialController.Instance.MoveBear(new Vector2(696, -375), true);
    }

    private void HighlightDrawer() {
        Debug.Log("TUTORIAL FUNC: " + "HighlightDrawer (full drawer area)");
        TutorialController.Instance.ToggleDrawerHighlight(true);
    }

    private void HighlightLevelOneEnemy() {
        Debug.Log("TUTORIAL FUNC: " + "HighlightLevelOneEnemy");
        TutorialController.Instance.MoveHighlight(new Vector2(68, 113));
    }

    private void HighlightPlayerFirstWarrior() {
        Debug.Log("TUTORIAL FUNC: " + "HighlightPlayerFirstWarrior");
        TutorialController.Instance.MoveHighlight(new Vector2(-834, -440));
        TutorialController.Instance.MoveBear(new Vector2(595, 216), true);
    }

    private void HighlightWhiteboard() {
        Debug.Log("TUTORIAL FUNC: " + "HighlightWhiteBoard");
        TutorialController.Instance.MoveBear(new Vector2(39, -403), false);
    }

    private void LoadFirstWarrior() {}
    private void LoadLastWarrior() {}
    private void LoadFirstEnemy() {}
    private void ShowArrow() {}

    private void SwitchToLevelScene() {
        Debug.Log("TUTORIAL FUNC: " + "SwitchToLevelScene");
        TutorialController.Instance.TutorialChangeSceneWithDelay("LevelScene");
    }

    private void SwitchToCodeEditor() {
        Debug.Log("TUTORIAL FUNC: " + "SwitchToCodeEditor");
        TutorialController.Instance.TutorialChangeSceneWithDelay("CodeEditor");
    }

    // still need to define none for lookup table
    private void None() {
        TutorialController.Instance.HideHighlight();
    }

}

[System.Serializable]
public struct TutorialListItem {
    public string tutorialDialog;
    public TutorialFunctionalitySO.FunctionOptions functionOption;
}