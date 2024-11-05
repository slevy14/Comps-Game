using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Objects/TutorialFunctionality")]
public class TutorialFunctionalitySO : ScriptableObject {

    [SerializeField] public List<TutorialListItem> tutorialListItems;

    [System.Serializable]
    // infuriatingly, these are in no specific order. if you try to change them unity will kill you
    // some of these may be labeled as not in use! to the same point above, unity will kill you if you remove them
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
        HighlightPlayerFirstWarrior = 16,
        HighlightWhiteboard = 17,
        ShowcaseBlock = 18,
        HighlightLevelDataBox = 19,
        HighlightBattleSpeedSlider = 20,
        HighlightSpriteDropdown = 21,
        HighlightToLevel = 22
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
            { FunctionOptions.ShowArrow, ShowArrow }, // NOT IN USE
            { FunctionOptions.None, None },
            { FunctionOptions.SwitchToLevelScene, SwitchToLevelScene },
            { FunctionOptions.SwitchToCodingEditor, SwitchToCodeEditor },
            { FunctionOptions.HighlightDrawer, HighlightDrawer },
            { FunctionOptions.HighlightLevelOneEnemy, HighlightLevelOneEnemy },
            { FunctionOptions.HighlightPlayerFirstWarrior, HighlightPlayerFirstWarrior },
            { FunctionOptions.HighlightWhiteboard, HighlightWhiteboard },
            { FunctionOptions.ShowcaseBlock, ShowcaseBlock },
            { FunctionOptions.HighlightStrength, HighlightStrength },
            { FunctionOptions.HighlightSaveButton, HighlightSaveButton },
            { FunctionOptions.HighlightBlocksArea, HighlightBlocksArea }, // NOT IN USE
            { FunctionOptions.Highlight, Highlight }, // NOT IN USE
            { FunctionOptions.HighlightLevelDataBox, HighlightLevelDataBox },
            { FunctionOptions.HighlightBattleSpeedSlider, HighlightBattleSpeedSlider },
            { FunctionOptions.HighlightSpriteDropdown, HighlightSpriteDropdown },
            { FunctionOptions.HighlightToLevel, HighlightToLevelButton }
        };
    }

    // public void SetSelectedFunction(FunctionOption function) {
    //     _selectedFunction = function;
    // }

    public void RunTutorialFunction(int index) {
        // _selectedFunction = tutorialListItems[index].functionOption;
        // ActivateSelectedFunction();
        // Debug.Log("TUTORIAL DIALOG: " + tutorialListItems[index].tutorialDialog);
        // Debug.Log(_functionLookup);
        TutorialController.Instance.ResetTutorialStates(); // make sure no blocks are showing, etc.
        _functionLookup[tutorialListItems[index].functionOption].Invoke();
    }


    public void ActivateSelectedFunction() {
        _functionLookup[_selectedFunction].Invoke();
    }


    private void HighlightWarriorDrawer() {
        // Debug.Log("TUTORIAL FUNC: " + "HighlightWarriorsDrawer");
        TutorialController.Instance.MoveHighlight(new Vector2(-612, 178));
        TutorialController.Instance.MoveBear(new Vector2(696, -375), true);
        DesignerController.Instance.ShowWarriorDrawer();
    }

    private void HighlightBlocksDrawer() {
        // Debug.Log("TUTORIAL FUNC: " + "HighlightBlocksDrawer");
        TutorialController.Instance.MoveHighlight(new Vector2(-843, 178));
        TutorialController.Instance.MoveBear(new Vector2(696, -375), true);
        DesignerController.Instance.ShowBlockDrawer();
    }

    private void HighlightEnemyDrawer() {
        // Debug.Log("TUTORIAL FUNC: " + "HighlightEnemyDrawer");
        TutorialController.Instance.MoveHighlight(new Vector2(-374, 178));
        TutorialController.Instance.MoveBear(new Vector2(696, -375), true);
        DesignerController.Instance.ShowEnemiesDrawer();
    }

    private void HighlightDrawer() {
        // Debug.Log("TUTORIAL FUNC: " + "HighlightDrawer (full drawer area)");
        TutorialController.Instance.ToggleHighlight("drawer", true);
    }

    private void HighlightLevelOneEnemy() {
        // Debug.Log("TUTORIAL FUNC: " + "HighlightLevelOneEnemy");
        TutorialController.Instance.MoveHighlight(new Vector2(68, 113));
    }

    private void HighlightPlayerFirstWarrior() {
        // Debug.Log("TUTORIAL FUNC: " + "HighlightPlayerFirstWarrior");
        TutorialController.Instance.MoveHighlight(new Vector2(-834, -440));
        TutorialController.Instance.MoveBear(new Vector2(595, 216), true);
    }

    private void HighlightWhiteboard() {
        // Debug.Log("TUTORIAL FUNC: " + "HighlightWhiteBoard");
        TutorialController.Instance.MoveBear(new Vector2(39, -403), false);
        TutorialController.Instance.ToggleHighlight("whiteboard", true);
    }

    private void ShowcaseBlock() {
        // Debug.Log("TUTORIAL FUNC: " + "ShowcaseBlock");
        TutorialController.Instance.ShowNewLevelBlocks();
        TutorialController.Instance.MoveBear(new Vector2(595, 216), true);
    }

    private void LoadFirstWarrior() {
        // Debug.Log("TUTORIAL FUNC: " + "LoadFirstWarrior");
        // note: this doesn't actually seem to load properly rn. not using it!
        DesignerController.Instance.LoadWarriorToWhiteboard(0, 0, true, false);
        HighlightBlocksDrawer();
    }
    private void LoadLastWarrior() {
        // Debug.Log("TUTORIAL FUNC: " + "LoadLastWarrior");
        // note: this doesn't actually seem to load properly rn. not using it!
        DesignerController.Instance.LoadWarriorToWhiteboard(WarriorListController.Instance.GetCount()-1, WarriorListController.Instance.GetCount()-1, false, false);
    }
    private void LoadFirstEnemy() {
        // Debug.Log("TUTORIAL FUNC: " + "LoadFirstEnemy");
        // note: this doesn't actually seem to load properly rn. not using it!
        DesignerController.Instance.LoadWarriorToWhiteboard(0, 0, false, true);
    }
    private void HighlightStrength() {
        TutorialController.Instance.ToggleHighlight("strength", true);
    }
    private void HighlightSaveButton() {

        TutorialController.Instance.ToggleHighlight("save", true);
    }
    private void HighlightLevelDataBox() {

        TutorialController.Instance.ToggleHighlight("levelData", true);
    }
    private void HighlightBattleSpeedSlider() {

        TutorialController.Instance.ToggleHighlight("battleSpeed", true);
    }
    private void HighlightSpriteDropdown() {

        TutorialController.Instance.ToggleHighlight("spriteDropdown", true);
    }

    private void HighlightToLevelButton() {
        TutorialController.Instance.ToggleHighlight("toLevel", true);
    }

    private void SwitchToLevelScene() {
        // Debug.Log("TUTORIAL FUNC: " + "SwitchToLevelScene");
        TutorialController.Instance.TutorialChangeSceneWithDelay("LevelScene");
    }

    private void SwitchToCodeEditor() {
        // Debug.Log("TUTORIAL FUNC: " + "SwitchToCodeEditor");
        TutorialController.Instance.TutorialChangeSceneWithDelay("CodeEditor");
    }

    private void None() {
        TutorialController.Instance.HideHighlight();
        TutorialController.Instance.ResetBearAndTextboxPositions();
    }

    // NOT IN USE:
    private void Highlight() {}
    private void HighlightBlocksArea() {}
    private void ShowArrow() {}

}

[System.Serializable]
public struct TutorialListItem {
    public string tutorialDialog;
    public TutorialFunctionalitySO.FunctionOptions functionOption;
}