using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PlacementSystem : MonoBehaviour {

    [Header("References")]
    [SerializeField] private GameObject cellIndicator;
    [SerializeField] private GameObject mouseIndicator;
    [SerializeField] private InputManager inputManager;
    [SerializeField] public Tilemap tilemap;

    [Space(20)]
    public GameObject currentDraggingObject;

    [Space(20)]
    [Header("Time")]
    private float clickTime;
    private float clickMaxTime = .1f;
    private bool isDrag = false;

    // private LevelController levelController;

    // SINGLETON
    public static PlacementSystem Instance = null; // for persistent

    public void CheckSingleton() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }
    }

    void Awake() {
        CheckSingleton();
        // levelController = GameObject.Find("LevelController").GetComponent<LevelController>();
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



        if (Input.GetMouseButton(0)) {
            clickTime += Time.deltaTime;


            if (clickTime >= clickMaxTime && isDrag == false) {
                if (LevelController.Instance.inBattle || (!LevelController.Instance.inBattle && LevelController.Instance.battleFinished)) {
                    return;
                }
                isDrag = true;
                GameObject warrior = WarriorClicked();
                // Debug.Log("drag started at " + Time.time);

                // dragging logic
                if (warrior != null && (!warrior.GetComponent<WarriorBehavior>().isEnemy || ProgressionController.Instance.currentLevel == 0)) {
                    // Debug.Log(hit.collider.gameObject.name);
                    warrior.GetComponent<WarriorBehavior>().StartDrag();
                    // Debug.Log("started drag from placement");
                    if (currentDraggingObject != warrior.gameObject) {
                        currentDraggingObject = warrior.gameObject;
                    }

                    // SHOW STATS SCREEN BASED ON WARRIOR INDEX
                    Debug.Log("show stats screen from drag warrior");
                    LevelController.Instance.ShowStatsPanel(warrior.GetComponent<WarriorBehavior>().warriorIndex, warrior.GetComponent<WarriorBehavior>().isEnemy);
                }
            }
        }

        // if (Input.GetMouseButtonDown(0)) {
        //     Vector2 ray = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        //     RaycastHit2D hit = Physics2D.Raycast(ray, ray);
        //     if (hit.collider != null && hit.collider.gameObject.tag == "warrior") {
        //         // Debug.Log(hit.collider.gameObject.name);
        //         hit.collider.gameObject.GetComponent<WarriorBehavior>().StartDrag();
        //         if (currentDraggingObject != hit.collider.gameObject) {
        //             currentDraggingObject = hit.collider.gameObject;
        //         }
        //     }
        // }

        if (currentDraggingObject != null) {
            // if (tilemap.HasTile(gridPosition)) {
            //     currentDraggingObject.transform.position = tilemap.GetCellCenterWorld(gridPosition);
            //     // mouseIndicator.transform.position = mousePosition;
            // } else {
            //     currentDraggingObject.transform.position = inputManager.GetSelectedMapPosition();
            // }

            currentDraggingObject.transform.position = inputManager.GetSelectedMapPosition();
        }

        if (Input.GetMouseButtonUp(0)) {
            if (currentDraggingObject != null) {
                currentDraggingObject.transform.position = tilemap.GetCellCenterWorld(gridPosition);
                Vector2 gridPosVec2 = new Vector2(gridPosition.x, gridPosition.y);
                int endIndex = 0; // figure out what collision, 0 if none
                if (IsOverUI()) { // if over drawer, index 3
                    endIndex = 3;
                } else if (LevelController.Instance.objectsOnGrid.ContainsValue(gridPosVec2)) { // if object list contains an object at that location, index 1
                    endIndex = 1;
                } else if (!tilemap.HasTile(gridPosition)) { // if no tile, index 2
                    endIndex = 2;
                } else {
                    LevelController.Instance.objectsOnGrid[currentDraggingObject] = gridPosVec2;
                }

                // special case -- if new and not on grid, just destroy it
                if (currentDraggingObject.GetComponent<WarriorBehavior>().isNew && endIndex != 0) {
                    endIndex = 3;
                } else {
                    currentDraggingObject.GetComponent<WarriorBehavior>().isNew = false;
                }

                // Debug.Log(endIndex);
                currentDraggingObject.GetComponent<WarriorBehavior>().EndDrag(endIndex);
                currentDraggingObject = null;
            }

            if (isDrag == false) {
                Debug.Log("twas a click at " + Time.time);
                GameObject thumbnail = ThumbnailClicked();
                if (thumbnail != null) {
                    // SHOW STATS SCREEN BASED ON WARRIOR INDEX
                    LevelController.Instance.ShowStatsPanel(thumbnail.GetComponent<WarriorLevelThumbnail>().warriorIndex, thumbnail.GetComponent<WarriorLevelThumbnail>().isEnemy);
                    Debug.Log("show stats screen from click thumbnail");
                }
                GameObject warrior = WarriorClicked();
                if (warrior != null) {
                    // SHOW STATS SCREEN BASED ON WARRIOR INDEX
                    LevelController.Instance.ShowStatsPanel(warrior.GetComponent<WarriorBehavior>().warriorIndex, warrior.GetComponent<WarriorBehavior>().isEnemy);
                    Debug.Log("show stats screen from click warrior");
                }
            }
            clickTime = 0f;
            isDrag = false;
        }
    }

    private bool IsOverUI() {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        for (int i = 0; i < raycastResults.Count; i++) {
            // Debug.Log(raycastResults[i].gameObject.name);
            if (raycastResults[i].gameObject.layer != 5) { // ui layer
                raycastResults.RemoveAt(i);
                i--;
            }
        }
        return raycastResults.Count > 0;
    }

    private GameObject ThumbnailClicked() { // meant to be called only when a click occurs
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        GameObject thumbnail = null;
        for (int i = 0; i < raycastResults.Count; i++) {
            // Debug.Log(raycastResults[i].gameObject.name);
            if (raycastResults[i].gameObject.tag == "thumbnail") {
                // Debug.Log("clicked on " + raycastResults[i].gameObject.name);
                thumbnail = raycastResults[i].gameObject;
            }
        }

        return thumbnail;
    }

    private GameObject WarriorClicked() {
        GameObject warrior = null;
        Vector2 ray = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        RaycastHit2D hit = Physics2D.Raycast(ray, ray);

        if (hit.collider != null && (hit.collider.gameObject.tag == "warrior" || hit.collider.gameObject.tag == "enemy")) {
            warrior = hit.collider.gameObject;
        }

        return warrior;
    }

}
