using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeObjectBehavior : MonoBehaviour {

    public float zRot;
    public float maxZRot;
    public float zStepPerFrame;

    void Awake() {
        zRot = 0;
        maxZRot = 180;
        zStepPerFrame = .75f;
    }

    void Update() {
        transform.Rotate(new Vector3(0, 0, zStepPerFrame));
        zRot += zStepPerFrame;
        if (zRot > maxZRot) {
            Destroy(this.gameObject);
            // Debug.Log("destroying, " + zRot);
        }
    }


}
