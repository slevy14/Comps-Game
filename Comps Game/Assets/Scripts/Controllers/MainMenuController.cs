using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuController : MonoBehaviour {

    // used in the main menu for updating buttons

    [SerializeField] private GameObject continueButton;

    public static MainMenuController Instance = null; // for persistent

    public void Awake() {
        CheckSingleton();
    }

    public void CheckSingleton() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }
    }

    public void Start() {
        // hide continue button if nothing to continue from
        if (ProgressionController.Instance.continueLevelFrom == -1) {
            continueButton.SetActive(false);
        } else { // otherwise update text to show continue from level
            continueButton.transform.GetChild(0).GetComponent<TMP_Text>().text = $"Continue\nLevel {ProgressionController.Instance.continueLevelFrom}";
        }

        // start music
        AudioController.Instance.ChangeBGM("Main Menu BGM");
    }


}
