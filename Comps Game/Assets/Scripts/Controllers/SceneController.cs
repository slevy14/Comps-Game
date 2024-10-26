using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {

    // for transitions
    public float transitionTime;

    public static SceneController Instance = null; // for persistent

    public void Awake() {
        CheckSingleton();
        transitionTime = .5f;
    }

    public void CheckSingleton() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }
        // Make this object stay around when switching scenes
        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadSceneByName(string name) {
        // SceneManager.LoadScene(name);
        StartCoroutine(LoadSceneByNameCoroutine(name));
    }

    public string GetCurrentSceneName() {
        return SceneManager.GetActiveScene().name;
    }

    public IEnumerator LoadSceneByNameCoroutine(string name) {
        GameObject.Find("TransitionCanvas").GetComponent<Animator>().SetTrigger("Start");
        AudioController.Instance.PlaySoundEffect("Transition");

        yield return new WaitForSeconds(transitionTime);

        AudioController.Instance.PlaySoundEffect("Transition");
        SceneManager.LoadScene(name);
    }

}
