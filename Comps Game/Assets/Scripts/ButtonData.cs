using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonData : MonoBehaviour {

    // meant to be placed on buttons!
    // some general functions that all buttons may need

    [SerializeField] private string sceneToLoad;
    [SerializeField] private GameObject interactableObject;


    public void JumpToScene() {
        SceneController.Instance.LoadSceneByName(sceneToLoad);
    }

    public void ContinueGame() {
        // go to the current level
        ProgressionController.Instance.currentLevel = ProgressionController.Instance.continueLevelFrom;
        ProgressionController.Instance.StartNewLevel(ProgressionController.Instance.currentLevel);
        SceneController.Instance.LoadSceneByName("LevelScene");
    }

    public void NextLevel() {
        // load game end screen if all levels complete
        if (ProgressionController.Instance.continueLevelFrom + 1 >= ProgressionController.Instance.levelDataList.Count) {
            SceneController.Instance.LoadSceneByName("GameEnd");
            return;
        }
        // updated stored level, load the next one
        ProgressionController.Instance.continueLevelFrom += 1;
        ProgressionController.Instance.currentLevel = ProgressionController.Instance.continueLevelFrom;
        ProgressionController.Instance.StartNewLevel(ProgressionController.Instance.currentLevel);
        SceneController.Instance.LoadSceneByName("LevelScene");
    }
    public void PromptNewGame() {
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
        WarriorListController.Instance.ResetWarriorsJSON("level_warriors");
        SceneController.Instance.LoadSceneByName("LevelScene");
    }

    public void BackToLevel() {
        SceneController.Instance.LoadSceneByName("LevelScene");
    }

    public void GoToSandbox() {
        ProgressionController.Instance.currentLevel = 0; // 0 is sandbox
        SceneController.Instance.LoadSceneByName("Sandbox");
    }

    public void PauseButton() {
        GameObject.Find("PauseMenuController").GetComponent<PauseMenuController>().Pause();
    }
}
