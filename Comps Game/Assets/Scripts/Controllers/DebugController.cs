using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour {

    // used for a debug controller object
    // not visible to the player

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
        // check if "`D" is pressed, load debug menu
        if (Input.GetKey(KeyCode.BackQuote) && Input.GetKeyDown(KeyCode.D)) {
            ToggleDebugMenu();
        }
    }

    private void ToggleDebugMenu() {
        // display debug menu
        debugMenu.SetActive(!debugMenu.activeSelf);
    }

}
