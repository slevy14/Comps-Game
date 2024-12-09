using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropdownSetter : MonoBehaviour {

    // placed on all block dropdowns in the editor

    void Start() {
        // set the dropdown on this object to respond to value changes
        if (this.gameObject.GetComponent<TMP_Dropdown>()) {
            this.gameObject.GetComponent<TMP_Dropdown>().onValueChanged.AddListener(delegate{this.SetDropdownValue();});
        }
    }

    public void SetDropdownValue() {
        // value changed! user has to manually save now
        DesignerController.Instance.justSaved = false;
    }
}
