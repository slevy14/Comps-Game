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
        zStepPerFrame = 20f;
    }

    void FixedUpdate() {
        float rotAmt = zStepPerFrame / (1.01f - LevelController.Instance.battleSpeed);
        transform.Rotate(new Vector3(0, 0,rotAmt));
        zRot += rotAmt;
        if (zRot > maxZRot) {
            Destroy(this.gameObject);
            // Debug.Log("destroying, " + zRot);
        }
    }


}
