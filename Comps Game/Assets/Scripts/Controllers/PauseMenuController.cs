using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuController : MonoBehaviour {


    [SerializeField] private GameObject pauseMenuCanvas;


    // persistent
    public static PauseMenuController Instance;

    public void CheckSingleton() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void Awake() {
        CheckSingleton();
    }


    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            TogglePauseMenuCanvas(!pauseMenuCanvas.activeSelf);
        }
    }

    public void TogglePauseMenuCanvas(bool value) {
        pauseMenuCanvas.SetActive(value);
    }


}
