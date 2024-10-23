using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonData : MonoBehaviour {

    [SerializeField] private string sceneToLoad;
    [SerializeField] private GameObject interactableObject;

    public void JumpToScene() {
        SceneController.Instance.LoadSceneByName(sceneToLoad);
    }

    public void ContinueGame() {
        ProgressionController.Instance.currentLevel = ProgressionController.Instance.continueLevelFrom;
        ProgressionController.Instance.StartNewLevel(ProgressionController.Instance.currentLevel);
        SceneController.Instance.LoadSceneByName("LevelScene");
    }

    public void BackToLevel() {
        SceneController.Instance.LoadSceneByName("LevelScene");
    }

    public void PromptNewGame() {
        // FIXME

        // warn player before creating new game if save file already exists
        if (ProgressionController.Instance.continueLevelFrom == -1) {
            NewGame();
        } else {
            interactableObject.SetActive(true);
        }
    }

    public void NewGame() {
        // set continue level from to 1, current level to 1
        ProgressionController.Instance.StartNewLevel(1);
        SceneController.Instance.LoadSceneByName("LevelScene");
    }

    public void GoToSandbox() {
        ProgressionController.Instance.currentLevel = 0; // 0 is sandbox
        SceneController.Instance.LoadSceneByName("Sandbox");
    }

}
