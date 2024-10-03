using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlacementSystem : MonoBehaviour {

    [SerializeField] private GameObject cellIndicator;
    [SerializeField] private GameObject mouseIndicator;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Tilemap tilemap;

    private GameObject currentDraggingObject;

    void Update() {
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = tilemap.WorldToCell(mousePosition);
        gridPosition.z = 0;
        // Debug.Log(gridPosition);

        if (tilemap.HasTile(gridPosition)) {
            cellIndicator.transform.position = tilemap.GetCellCenterWorld(gridPosition);
            // mouseIndicator.transform.position = mousePosition;
        } else {
            // Debug.Log("no tile at " + gridPosition);
            cellIndicator.transform.position = new Vector3(0f, 0f, 1f);
            // mouseIndicator.transform.position = new Vector3(0f, 0f, 1f);
        }

        // dragging
        if (Input.GetMouseButtonDown(0)) {
            Vector2 ray = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            RaycastHit2D hit = Physics2D.Raycast(ray, ray);
            if (hit.collider != null && hit.collider.gameObject.tag == "warrior") {
                Debug.Log(hit.collider.gameObject.name);
                hit.collider.gameObject.GetComponent<WarriorBehavior>().isDragging = true;
                if (currentDraggingObject != hit.collider.gameObject) currentDraggingObject = hit.collider.gameObject;
            }
        }
        if (Input.GetMouseButtonUp(0)) {
            if (currentDraggingObject != null) {
                currentDraggingObject.GetComponent<WarriorBehavior>().isDragging = false;
                currentDraggingObject = null;
            }
        }
    }

}
