using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuController : MonoBehaviour {


    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private GameObject confirmPromptMenu;
    [SerializeField] private GameObject quitButton;
    [SerializeField] private GameObject menuButton;
    [SerializeField] private GameObject backButton;


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
        confirmPromptMenu.SetActive(false);
    }


    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            TogglePauseMenuCanvas(!pauseMenuCanvas.activeSelf);
            confirmPromptMenu.SetActive(false);
        }
    }

    public void TogglePauseMenuCanvas(bool value) {
        pauseMenuCanvas.SetActive(value);
    }

    public void PromptQuitGame(bool fullQuit) {
        confirmPromptMenu.SetActive(true);
        Debug.Log("showing prompt quit menu");
        quitButton.SetActive(fullQuit);
        menuButton.SetActive(!fullQuit);
    }

    public void CancelQuit() {
        confirmPromptMenu.SetActive(false);
    }

    public void BackToMenu() {
        SceneController.Instance.LoadSceneByName("MainMenu");
        TutorialController.Instance.EndTutorial();
        TogglePauseMenuCanvas(false);
    }

    public void QuitGame() {
        TogglePauseMenuCanvas(false);
        Application.Quit();
    }


}
