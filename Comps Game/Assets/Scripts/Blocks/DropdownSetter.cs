using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropdownSetter : MonoBehaviour {
    void Start() {
        if (this.gameObject.GetComponent<TMP_Dropdown>()) {
            this.gameObject.GetComponent<TMP_Dropdown>().onValueChanged.AddListener(delegate{this.SetDropdownValue();});
        }
    }

    public void SetDropdownValue() {
        DesignerController.Instance.justSaved = false;
    }
}
