using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PlacementSystem : MonoBehaviour {

    [SerializeField] private GameObject cellIndicator;
    [SerializeField] private GameObject mouseIndicator;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Tilemap tilemap;

    public GameObject currentDraggingObject;

    private LevelController levelController;

    // // SINGLETON
    // public static PlacementSystem Instance = null; // for persistent

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
    // }

    void Awake() {
        levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
    }

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
                // Debug.Log(hit.collider.gameObject.name);
                hit.collider.gameObject.GetComponent<WarriorBehavior>().StartDrag();
                if (currentDraggingObject != hit.collider.gameObject) {
                    currentDraggingObject = hit.collider.gameObject;
                }
            }
        }

        if (currentDraggingObject != null) {
            if (tilemap.HasTile(gridPosition)) {
                currentDraggingObject.transform.position = tilemap.GetCellCenterWorld(gridPosition);
                // mouseIndicator.transform.position = mousePosition;
            } else {
                currentDraggingObject.transform.position = inputManager.GetSelectedMapPosition();
            }

            // currentDraggingObject.transform.position = inputManager.GetSelectedMapPosition();
        }

        if (Input.GetMouseButtonUp(0)) {
            if (currentDraggingObject != null) {
                Vector2 gridPosVec2 = new Vector2(gridPosition.x, gridPosition.y);
                int endIndex = 0; // figure out what collision, 0 if none
                if (IsOverUI()) { // if over drawer, index 3
                    endIndex = 3;
                } else if (levelController.objectsOnGrid.ContainsValue(gridPosVec2)) { // if object list contains an object at that location, index 1
                    endIndex = 1;
                } else if (!tilemap.HasTile(gridPosition)) { // if no tile, index 2
                    endIndex = 2;
                } else {
                    levelController.objectsOnGrid[currentDraggingObject] = gridPosVec2;
                }

                // special case -- if new and not on grid, just destroy it
                if (currentDraggingObject.GetComponent<WarriorBehavior>().isNew && endIndex != 0) {
                    endIndex = 3;
                } else {
                    currentDraggingObject.GetComponent<WarriorBehavior>().isNew = false;
                }

                Debug.Log(endIndex);
                currentDraggingObject.GetComponent<WarriorBehavior>().EndDrag(endIndex);
                currentDraggingObject = null;
            }
        }
    }

    private bool IsOverUI() {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        for (int i = 0; i < raycastResults.Count; i++) {
            Debug.Log(raycastResults[i].gameObject.name);
            if (raycastResults[i].gameObject.layer != 5) { // ui layer
                raycastResults.RemoveAt(i);
                i--;
            }
        }
        return raycastResults.Count > 0;
    }

}
