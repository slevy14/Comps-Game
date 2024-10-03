using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlacementSystem : MonoBehaviour {

    [SerializeField] private GameObject mouseIndicator, cellIndicator;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Tilemap tilemap;
    void Update() {
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = tilemap.WorldToCell(mousePosition);
        gridPosition.z = 0;
        // Debug.Log(gridPosition);

        if (tilemap.HasTile(gridPosition)) {
            cellIndicator.transform.position = tilemap.GetCellCenterWorld(gridPosition);
            mouseIndicator.transform.position = mousePosition;
        } else {
            Debug.Log("no tile at " + gridPosition);
            cellIndicator.transform.position = new Vector3(0f, 0f, 1f);
            mouseIndicator.transform.position = new Vector3(0f, 0f, 1f);
        }
    }

}
