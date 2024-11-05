using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PropertySlider : MonoBehaviour, IPointerUpHandler {

    [SerializeField] TMP_Text valueText;

    void Awake() {
        valueText.text = "" + this.gameObject.GetComponent<Slider>().minValue;
    }

    public void DynamicUpdateValueText(float value) {
        valueText.text = "" + Mathf.RoundToInt(value);
    }

    public void OnPointerUp(PointerEventData eventData) {
        // Debug.Log("done sliding");
        if (transform.parent.GetComponent<Draggable>()) {
            transform.parent.GetComponent<Draggable>().SetValueFromSlider();
            DesignerController.Instance.justSaved = false;
            DesignerController.Instance.UpdateStrengthDisplay();
        }
    }
}
