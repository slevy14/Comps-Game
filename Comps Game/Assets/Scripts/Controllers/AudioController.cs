using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour {

    [SerializeField] public List<AudioDictionaryElement> SFXList;
    [SerializeField] public List<AudioDictionaryElement> BGMList; 
    [SerializeField] private AudioSource audioSource;

    public static AudioController Instance = null; // for persistent

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
        DontDestroyOnLoad(this.gameObject);
    }

    public void PlaySoundEffect(string name) {
        audioSource.PlayOneShot(FindSoundByName(name));
    }

    public AudioClip FindSoundByName(string name) {
        foreach (AudioDictionaryElement audioDictionaryElement in SFXList) {
            if (audioDictionaryElement.name == name) {
                return audioDictionaryElement.audioClip;
            }
        }
        Debug.Log("found no audio with the name " + name);
        return null;
    }

}

[System.Serializable]
public struct AudioDictionaryElement {
    public string name;
    public AudioClip audioClip;
}
