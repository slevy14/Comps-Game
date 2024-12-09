using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputFieldSetter : MonoBehaviour {

    // placed on all block input fields in the editor

    void Start() {
        if (this.gameObject.GetComponent<TMP_InputField>()) {
            // set the input field on this object to respond to value changes
            this.gameObject.GetComponent<TMP_InputField>().onValueChanged.AddListener(delegate{this.SetInputFieldValue();});
        }
    }

    public void SetInputFieldValue() {
        // if this block is a for loop, keep its value between 1 and 15
        if (transform.parent.GetComponent<BlockData>().behavior == BlockData.BehaviorType.FOR_LOOP) {
            int forLoopCount = 0;
            int.TryParse(this.gameObject.GetComponent<TMP_InputField>().text, out forLoopCount);
            this.gameObject.GetComponent<TMP_InputField>().text = ((int)Mathf.Clamp(forLoopCount, 1, 15)).ToString();
        }

        // if block changed on the whiteboard, update its value
        if (transform.parent.GetComponent<Draggable>()) {
            transform.parent.GetComponent<Draggable>().SetValueFromInputField();
            // value changed! user has to manually save now
            DesignerController.Instance.justSaved = false;
        }
    }
}
