using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalseStartDisplay : MonoBehaviour {

    [SerializeField] private float aliveSeconds;
    private float timer = 0;
    void Update() {
        // only display for an amount of time set in the inspector
        timer += Time.deltaTime;
        if (timer > aliveSeconds) {
            Destroy(this.gameObject);
        }
    }
}
