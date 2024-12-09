using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoStaminaDisplayBehavior : MonoBehaviour {

    // placed on no stamina display object, child of warrior
    // used to show in-battle errors

    [SerializeField] Vector2 initialPosition;
    float maxTime;
    float timer;

    void Awake() {
        // set initial object position
        initialPosition = this.gameObject.GetComponent<RectTransform>().anchoredPosition;
        Debug.Log("awake called");
        maxTime = 1f;
    }

    void OnEnable() {
        ShowNoStaminaText();
    }
    void OnDisable() {
        // reset object position when disabled
        this.gameObject.GetComponent<RectTransform>().anchoredPosition = initialPosition;
        Debug.Log("ondisable called");
    }

    void Update() {
        // slowly move up for set amount of time
        this.gameObject.GetComponent<RectTransform>().Translate(new Vector2(0, 0.0002f));
        timer += Time.deltaTime;
        if (timer >= maxTime) {
            timer = 0;
            this.gameObject.SetActive(false);
        }
    }

    // Show error text as needed
    // this object is used for both no stamina and no target
    // object should probably be renamed, but leaving it as this to not break unity references

    public void ShowNoStaminaText() {
        Debug.Log("anchored pos: " + this.gameObject.GetComponent<RectTransform>().anchoredPosition);
        this.gameObject.GetComponent<RectTransform>().anchoredPosition = initialPosition;
        this.transform.GetChild(0).GetComponent<TMP_Text>().text = "No Stamina!";
        Debug.Log("anchored pos: " + this.gameObject.GetComponent<RectTransform>().anchoredPosition);
        timer = 0;
    }

    public void ShowNoTargetText() {
        Debug.Log("anchored pos: " + this.gameObject.GetComponent<RectTransform>().anchoredPosition);
        this.gameObject.GetComponent<RectTransform>().anchoredPosition = initialPosition;
        this.transform.GetChild(0).GetComponent<TMP_Text>().text = "No Target!";
        Debug.Log("anchored pos: " + this.gameObject.GetComponent<RectTransform>().anchoredPosition);
        timer = 0;
    }
}
