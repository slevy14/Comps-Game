using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlacementSystem : MonoBehaviour {

    [SerializeField] private GameObject mouseIndicator, cellIndicator;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Vector3 gridOffset;

    void Update() {
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = tilemap.WorldToCell(mousePosition + gridOffset);

        if (tilemap.HasTile(gridPosition)) {
            cellIndicator.transform.position = tilemap.GetCellCenterWorld(gridPosition);
            mouseIndicator.transform.position = mousePosition;
        } else {
            cellIndicator.transform.position = new Vector3(0f, 0f, 1f);
            mouseIndicator.transform.position = new Vector3(0f, 0f, 1f);
        }
    }

}
