using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour {

    public enum GameState {
        CODING,
        PREPARING_BATTLE,
        BATTLE,
        WIN,
        LOSE,
        CUTSCENE,
        DIALOGUE,
        PAUSED,
        MENU
    }

    public void Awake() {
        DontDestroyOnLoad(this.gameObject);
        GameObject[] found_objects = GameObject.FindGameObjectsWithTag("STATE_MACHINE");
        foreach (GameObject obj in found_objects) {
            if (obj != this.gameObject) {
                Destroy(obj);
            }
        }

    }

}
