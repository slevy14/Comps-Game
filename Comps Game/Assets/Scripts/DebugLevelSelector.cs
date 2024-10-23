using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugLevelSelector : MonoBehaviour {

    public TMP_Dropdown dropdown;

    void Awake() {
        dropdown = transform.GetChild(0).GetComponent<TMP_Dropdown>();
    }

    void Start() {
        InitializeLevelSelectionOptions();
    }

    void InitializeLevelSelectionOptions() {
        dropdown.ClearOptions();
        for (int i = 1; i < ProgressionController.Instance.levelDataList.Count; i++) {
            dropdown.options.Add(new TMP_Dropdown.OptionData(ProgressionController.Instance.levelDataList[i].levelNumber + ": " + ProgressionController.Instance.levelDataList[i].levelName));
        }
        dropdown.onValueChanged.AddListener(ChangeStartingLevel);
        dropdown.value = ProgressionController.Instance.continueLevelFrom - 1;
        dropdown.RefreshShownValue();
    }

    void ChangeStartingLevel(Int32 newLevelIndex) {
        ProgressionController.Instance.continueLevelFrom = newLevelIndex + 1;
    }



}
