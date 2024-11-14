using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputFieldSetter : MonoBehaviour {

    void Start() {
        if (this.gameObject.GetComponent<TMP_InputField>()) {
            // Debug.Log("found input field");
            this.gameObject.GetComponent<TMP_InputField>().onValueChanged.AddListener(delegate{this.SetInputFieldValue();});
        }
    }

    public void SetInputFieldValue() {
        if (transform.parent.GetComponent<BlockData>().behavior == BlockData.BehaviorType.FOR_LOOP) {
            int forLoopCount = 0;
            int.TryParse(this.gameObject.GetComponent<TMP_InputField>().text, out forLoopCount);
            this.gameObject.GetComponent<TMP_InputField>().text = ((int)Mathf.Clamp(forLoopCount, 1, 10)).ToString();
        }
        if (transform.parent.GetComponent<Draggable>()) {
            transform.parent.GetComponent<Draggable>().SetValueFromInputField();
            DesignerController.Instance.justSaved = false;
        }
    }
}
