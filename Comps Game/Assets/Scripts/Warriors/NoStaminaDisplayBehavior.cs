using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoStaminaDisplayBehavior : MonoBehaviour {

    [SerializeField] Vector2 initialPosition;
    float maxTime;
    float timer;

    void Awake() {
        initialPosition = this.gameObject.GetComponent<RectTransform>().anchoredPosition;
        Debug.Log("awake called");
        maxTime = 1f;
    }

    void OnEnable() {
        ShowNoStaminaText();
    }
    void OnDisable() {
        this.gameObject.GetComponent<RectTransform>().anchoredPosition = initialPosition;
        Debug.Log("ondisable called");
    }

    void Update() {
        this.gameObject.GetComponent<RectTransform>().Translate(new Vector2(0, 0.0002f));
        timer += Time.deltaTime;
        if (timer >= maxTime) {
            timer = 0;
            this.gameObject.SetActive(false);
        }
    }

    public void ShowNoStaminaText() {
        Debug.Log("anchored pos: " + this.gameObject.GetComponent<RectTransform>().anchoredPosition);
        this.gameObject.GetComponent<RectTransform>().anchoredPosition = initialPosition;
        Debug.Log("anchored pos: " + this.gameObject.GetComponent<RectTransform>().anchoredPosition);
        timer = 0;
    }
}
