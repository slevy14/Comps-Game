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
            { FunctionOptions.SwitchToCodingEditor, SwitchToCodeEditor }
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
        TutorialController.Instance.MoveHighlight(new Vector2(-843, 178));
    }

    private void HighlightBlocksDrawer() {
        Debug.Log("TUTORIAL FUNC: " + "HighlightBlocksDrawer");
        TutorialController.Instance.MoveHighlight(new Vector2(-612, 178));
    }

    private void HighlightEnemyDrawer() {
        Debug.Log("TUTORIAL FUNC: " + "HighlightEnemyDrawer");
        TutorialController.Instance.MoveHighlight(new Vector2(-374, 178));
    }

    private void LoadFirstWarrior() {}
    private void LoadLastWarrior() {}
    private void LoadFirstEnemy() {}
    private void ShowArrow() {}

    private void SwitchToLevelScene() {
        SceneController.Instance.LoadSceneByName("LevelScene");
        TutorialController.Instance.DisableHighlight();
    }

    private void SwitchToCodeEditor() {
        SceneController.Instance.LoadSceneByName("CodeEditor");
        TutorialController.Instance.DisableHighlight();
    }

    // still need to define none for lookup table
    private void None() {
        TutorialController.Instance.DisableHighlight();
    }

}

[System.Serializable]
public struct TutorialListItem {
    public string tutorialDialog;
    public TutorialFunctionalitySO.FunctionOptions functionOption;
}