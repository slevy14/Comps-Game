using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour {

    // placed on audio controller object

    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] public List<AudioDictionaryElement> SFXList;
    [SerializeField] public List<AudioDictionaryElement> BGMList; 

    public static AudioController Instance = null; // for persistent

    void Awake() {
        CheckSingleton();
    }

    void Start() {
        // start background music for scene
        StartBGM();
    }

    public void CheckSingleton() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void StartBGM() {
        // FOR DEBUG
        // this will always be main menu when launching the game
        switch (SceneController.Instance.GetCurrentSceneName()) {
            case "MainMenu":
                bgmAudioSource.clip = FindBGMByName("Main Menu BGM");
                break;
            default:
                bgmAudioSource.clip = FindBGMByName("Coding BGM");
                break;
        }
        bgmAudioSource.Play();
    }

    // PLAY MUSIC:

    public void ChangeBGM(string name) {
        if (bgmAudioSource.clip != FindBGMByName(name) || !bgmAudioSource.isPlaying) {
            bgmAudioSource.clip = FindBGMByName(name);
            bgmAudioSource.Play();
        }
    }

    public void StopBGM() {
        bgmAudioSource.Stop();
    }

    public void PlaySoundEffect(string name) {
        sfxAudioSource.PlayOneShot(FindSFXByName(name));
    }

    // GET MUSIC:

    public AudioClip FindSFXByName(string name) {
        foreach (AudioDictionaryElement audioDictionaryElement in SFXList) {
            if (audioDictionaryElement.name == name) {
                return audioDictionaryElement.audioClip;
            }
        }
        Debug.Log("found no audio with the name " + name);
        return null;
    }

    public AudioClip FindBGMByName(string name) {
        foreach (AudioDictionaryElement audioDictionaryElement in BGMList) {
            if (audioDictionaryElement.name == name) {
                return audioDictionaryElement.audioClip;
            }
        }
        Debug.Log("found no audio with the name " + name);
        return null;
    }

}

// separate struct for pseudo-serializable dictionary

[System.Serializable]
public struct AudioDictionaryElement {
    public string name;
    public AudioClip audioClip;
}
