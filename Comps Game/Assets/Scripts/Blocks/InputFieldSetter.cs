using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputFieldSetter : MonoBehaviour {

    void Start() {
        if (this.gameObject.GetComponent<TMP_InputField>()) {
            Debug.Log("found input field");
            this.gameObject.GetComponent<TMP_InputField>().onValueChanged.AddListener(delegate{this.SetInputFieldValue();});
        }
    }

    public void SetInputFieldValue() {
        if (transform.parent.GetComponent<Draggable>()) {
            transform.parent.GetComponent<Draggable>().SetValueFromInputField();
        }
    }
}
