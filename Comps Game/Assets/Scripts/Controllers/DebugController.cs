using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour {

    [SerializeField] private GameObject debugMenu;

    public static DebugController Instance = null; // for persistent

    public void Awake() {
        CheckSingleton();
        ToggleDebugMenu();
    }

    public void CheckSingleton() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            ToggleDebugMenu();
        }
    }

    private void ToggleDebugMenu() {
        debugMenu.SetActive(!debugMenu.activeSelf);
    }

}
