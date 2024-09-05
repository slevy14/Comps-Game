using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DesignerController : MonoBehaviour {

    [Header("DEBUG")]
    [SerializeField] private bool DEBUG_MODE; // set in inspector

    [Header("References")]
    [SerializeField] private GameObject propertiesHeaderObject;
    [SerializeField] private GameObject behaviorHeaderObject;

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
            Destroy(tempObject);
        }

        SaveIntoJSON();
    }


    [SerializeField] private TowerProperties _TowerProperties = new TowerProperties();
    public void SaveIntoJSON() {
        string tower = JsonUtility.ToJson(_TowerProperties);
        string filePath = Application.persistentDataPath + $"/{_TowerProperties.towerName}.json";
        System.IO.File.WriteAllText(filePath, tower);
        print("saving json at " + filePath);
    }


}

[System.Serializable]
public class TowerProperties {
    public string towerName;
    public float targetingRange;
    public float rotationSpeed;
    public float bps; //bullets per second
}
