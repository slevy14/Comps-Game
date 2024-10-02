using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonData : MonoBehaviour {

    [SerializeField] private string sceneToLoad;

    public void JumpToScene() {
        SceneController.Instance.LoadSceneByName(sceneToLoad);
    }

}
