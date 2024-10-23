using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonData : MonoBehaviour {

    [SerializeField] private string sceneToLoad;

    public void JumpToScene() {
        SceneController.Instance.LoadSceneByName(sceneToLoad);
    }

    public void ContinueGame() {
        ProgressionController.Instance.currentLevel = ProgressionController.Instance.continueLevelFrom;
        SceneController.Instance.LoadSceneByName("LevelScene");
    }

    public void NewGame() {
        // FIXME

        // warn player before creating new game if save file already exists

        // set continue level from to 0, current level to 0
    }

}
