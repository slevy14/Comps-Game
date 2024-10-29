using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PropertySlider : MonoBehaviour {

    [SerializeField] TMP_Text valueText;

    void Awake() {
        valueText.text = "" + this.gameObject.GetComponent<Slider>().minValue;
    }

    public void DynamicUpdateValueText(float value) {
        valueText.text = "" + Mathf.RoundToInt(value);
    }


}
