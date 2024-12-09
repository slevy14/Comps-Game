using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeObjectBehavior : MonoBehaviour {

    // placed on melee display prefab

    public float zRot;
    public float maxZRot;
    public float zStepPerFrame;

    void Awake() {
        // initialize values
        zRot = 0;
        maxZRot = 180;
        zStepPerFrame = 12f;
    }

    void FixedUpdate() {
        // spin for a set amount of time
        float rotAmt = zStepPerFrame;
        transform.Rotate(new Vector3(0, 0,rotAmt));
        zRot += rotAmt;
        if (zRot > maxZRot) {
            Destroy(this.gameObject);
        }
    }


}
