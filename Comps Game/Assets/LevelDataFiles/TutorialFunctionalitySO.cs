using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Objects/TutorialFunctionality")]
public class TutorialFunctionalitySO : ScriptableObject {

    [SerializeField] public List<TutorialListItem> tutorialListItems;

    [System.Serializable]
    public enum FunctionOption {
        HighlightWarriorDrawer = 0,
        HighlightBlocksDrawer = 1,
        HighlihgtEnemyDrawer = 2,
        LoadFirstWarrior = 3,
        LoadLastWarrior = 4,
        LoadFirstEnemy = 5,
        ShowArrow = 6
    };

    [SerializeField]
    private FunctionOption _selectedFunction;
    private Dictionary<FunctionOption, System.Action> _functionLookup;


    private void Awake()
    {
        _functionLookup = new Dictionary<FunctionOption, System.Action>()
        {
            { FunctionOption.HighlightWarriorDrawer, HighlightWarriorDrawer },
            { FunctionOption.HighlightBlocksDrawer, HighlightBlocksDrawer },
            { FunctionOption.HighlihgtEnemyDrawer, HighlightEnemyDrawer }
        };
    }

    public void SetSelectedFunction(FunctionOption function) {
        _selectedFunction = function;
    }


    public void ActivateSelectedFunction()
    {
        _functionLookup[_selectedFunction].Invoke();
    }


    private void HighlightWarriorDrawer() {
        Debug.Log("HighlightWarriorsDrawer");
    }

    private void HighlightBlocksDrawer() {
        Debug.Log("HighlightBlocksDrawer");
    }

    private void HighlightEnemyDrawer() {
        Debug.Log("HighlightEnemyDrawer");
    }

}

[System.Serializable]
public struct TutorialListItem {
    public string tutorialDialog;
    public TutorialFunctionalitySO.FunctionOption functionOption;
}