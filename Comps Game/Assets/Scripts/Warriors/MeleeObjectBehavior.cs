using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeObjectBehavior : MonoBehaviour {

    public float zRot;
    public float maxZRot = 180;
    public float zStepPerFrame = .75f;

    void Awake() {
        zRot = 0;
    }

    void Update() {
        transform.Rotate(new Vector3(0, 0, zStepPerFrame));
        zRot += zStepPerFrame;
        if (zRot > 90f) {
            Destroy(this.gameObject);
        }
    }


}
