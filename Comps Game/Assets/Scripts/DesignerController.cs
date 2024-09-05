using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DesignerController : MonoBehaviour {

    [SerializeField] private bool DEBUG_MODE; // set in inspector

    private int counter = 0;

    void Start() {

    }

    void Update() {

    }

    public void SaveTower() {
        // debug code -- for testing!!
        if (DEBUG_MODE) {
            GameObject tempObject = new GameObject();
            tempObject.name = "tempObject_" + counter;
            tempObject.AddComponent<Rigidbody2D>();
            bool prefabSuccess;
            PrefabUtility.SaveAsPrefabAssetAndConnect(tempObject, $"Assets/SavedAssets/{tempObject.name}.prefab", InteractionMode.UserAction, out prefabSuccess);
            if (prefabSuccess == true) {
                Debug.Log("Prefab was saved successfully");
            } else {
                Debug.Log("Prefab failed to save" + prefabSuccess);
            }
            counter++;
        }
    }


}
