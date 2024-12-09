using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PropertySlider : MonoBehaviour, IPointerUpHandler {

    // placed on all block sliders in the editor

    [SerializeField] TMP_Text valueText;

    void Awake() {
        valueText.text = "" + this.gameObject.GetComponent<Slider>().minValue;
    }

    public void DynamicUpdateValueText(float value) {
        // update text while sliding
        valueText.text = "" + Mathf.RoundToInt(value);
        DesignerController.Instance.UpdateStrengthDisplay();
    }

    public void OnPointerUp(PointerEventData eventData) {
        // update strength and set value when done sliding
        if (transform.parent.GetComponent<Draggable>()) {
            transform.parent.GetComponent<Draggable>().SetValueFromSlider();
            // value changed! user has to manually save now
            DesignerController.Instance.justSaved = false;
            DesignerController.Instance.UpdateStrengthDisplay();
        }
    }
}
