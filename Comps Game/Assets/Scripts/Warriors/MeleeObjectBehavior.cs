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
        zStepPerFrame = 12f;
        Debug.Log("created melee icon at" + Time.time);
    }

    void FixedUpdate() {
        float rotAmt = zStepPerFrame;
        transform.Rotate(new Vector3(0, 0,rotAmt));
        zRot += rotAmt;
        if (zRot > maxZRot) {
            Debug.Log("destroyed melee icon at" + Time.time);
            Destroy(this.gameObject);
            // Debug.Log("destroying, " + zRot);
        }
    }


}
