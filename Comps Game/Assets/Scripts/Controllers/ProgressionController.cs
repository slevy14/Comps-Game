using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionController : MonoBehaviour {

    [SerializeField] public List<LevelDataSO> levelDataList;

    public static ProgressionController Instance = null; // for persistent

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
        // Make this object stay around when switching scenes
        DontDestroyOnLoad(this.gameObject);
    }


}
