using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

    [SerializeField] private Camera sceneCamera;

    private Vector3 lastPosition;

    [SerializeField] private LayerMask placementLayerMask;

    // // SINGLETON
    // public static InputManager Instance = null; // for persistent

    // public void Awake() {
    //     CheckSingleton();
    // }

    // public void CheckSingleton() {
    //     if (Instance == null) {
    //         Instance = this;
    //     } else {
    //         Destroy(this.gameObject);
    //         return;
    //     }
    //     // Make this object stay around when switching scenes
    //     DontDestroyOnLoad(this.gameObject);
    // }

    public Vector3 GetSelectedMapPosition() {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = sceneCamera.nearClipPlane;
        // Ray ray = sceneCamera.ScreenPointToRay(mousePos);
        // RaycastHit hit;
        // if (Physics.Raycast(ray, out hit, 100, placementLayerMask)) {
        //     lastPosition = hit.point;
        // }
        // return lastPosition;

        return Camera.main.ScreenToWorldPoint(mousePos);
    }

}
