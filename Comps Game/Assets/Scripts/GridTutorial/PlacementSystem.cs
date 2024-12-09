using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PlacementSystem : MonoBehaviour {

    [Header("References")]
    [SerializeField] private GameObject cellIndicator;
    private Color initialCellIndicatorColor;
    private bool isPlayerSide;
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
        initialCellIndicatorColor = cellIndicator.GetComponent<SpriteRenderer>().color;

    }

    void Update() {
        // check for mouse position
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = tilemap.WorldToCell(mousePosition);
        gridPosition.z = 0;

        // check if currently a tile under the mouse position
        if (tilemap.HasTile(gridPosition)) {
            // highlight tile under mouse
            cellIndicator.transform.position = tilemap.GetCellCenterWorld(gridPosition);
            // change highlihgt color based on position
            if (LevelController.Instance.isSandbox) {
                cellIndicator.GetComponent<SpriteRenderer>().material.color = initialCellIndicatorColor;
                isPlayerSide = true;
            } else if (gridPosition.x < 0) {
                cellIndicator.GetComponent<SpriteRenderer>().material.color = new Color(104f/255f, 104/255f, 241/255f, initialCellIndicatorColor.a*2);
                isPlayerSide = true;
            } else {
                cellIndicator.GetComponent<SpriteRenderer>().material.color = new Color(241/255f, 104/255f, 104/255f, initialCellIndicatorColor.a*2);
                isPlayerSide = false;
            }
        } else { // hide indicator if not on tile
            cellIndicator.transform.position = new Vector3(0f, 0f, 10f);
            cellIndicator.GetComponent<SpriteRenderer>().material.color = initialCellIndicatorColor;
        }


        // CLICK AND DRAG LOGIC:
        if (Input.GetMouseButton(0)) {
            // start timer of how long mouse has been clicked
            clickTime += Time.deltaTime;

            // if click has been held down long enough, this is a drag!
            if (clickTime >= clickMaxTime && isDrag == false) {
                // quit if not allowed to drag
                if (LevelController.Instance.inBattle || (!LevelController.Instance.inBattle && LevelController.Instance.battleFinished)) {
                    return;
                }

                // get data about warrior we are dragging
                isDrag = true;
                GameObject warrior = WarriorClicked();

                // dragging logic
                // check that we are allowed to drag the current warrior
                if (warrior != null && (!warrior.GetComponent<WarriorBehavior>().isEnemy || ProgressionController.Instance.currentLevel == 0)) {

                    // set dragging data
                    warrior.GetComponent<WarriorBehavior>().StartDrag();
                    if (currentDraggingObject != warrior.gameObject) {
                        currentDraggingObject = warrior.gameObject;
                    }

                    // SHOW STATS SCREEN BASED ON WARRIOR INDEX
                    LevelController.Instance.ShowStatsPanel(warrior.GetComponent<WarriorBehavior>().warriorIndex, warrior.GetComponent<WarriorBehavior>().isEnemy);
                }
            }
        }

        // keep object at mouse position
        if (currentDraggingObject != null) {
            currentDraggingObject.transform.position = inputManager.GetSelectedMapPosition();
        }

        // WHEN MOUSE RELEASED:
        if (Input.GetMouseButtonUp(0)) {
            // null check dragging object
            if (currentDraggingObject != null) {
                // attempt to snap warrior to grid position
                currentDraggingObject.transform.position = tilemap.GetCellCenterWorld(gridPosition);
                Vector2 gridPosVec2 = new Vector2(gridPosition.x, gridPosition.y);

                // check if placement collision
                // if no collision, add warrior to grid
                int endIndex = 0; // 0 if none
                if (IsOverUI() || IsTooManyWarriors()) { // if over drawer or too many placed, index 3
                    endIndex = 3;
                } else if (LevelController.Instance.objectsOnGrid.ContainsValue(gridPosVec2)) { // if object list contains an object at that location, index 1
                    endIndex = 1;
                    LevelController.Instance.objectsOnGrid[currentDraggingObject] = currentDraggingObject.GetComponent<WarriorBehavior>().initialGridPos;
                } else if (!tilemap.HasTile(gridPosition) || (!isPlayerSide && !LevelController.Instance.isSandbox)) { // if no tile or on wrong side, index 2
                    endIndex = 2;
                    LevelController.Instance.objectsOnGrid[currentDraggingObject] = currentDraggingObject.GetComponent<WarriorBehavior>().initialGridPos;
                } else {
                    LevelController.Instance.objectsOnGrid[currentDraggingObject] = gridPosVec2;
                }

                // special case -- if new and not on grid, just destroy it
                if (currentDraggingObject.GetComponent<WarriorBehavior>().isNew && endIndex != 0) {
                    endIndex = 3;
                } else {
                    currentDraggingObject.GetComponent<WarriorBehavior>().isNew = false;
                }

                // end drag
                currentDraggingObject.GetComponent<WarriorBehavior>().EndDrag(endIndex);
                currentDraggingObject = null;

                // grey out thumbnails if need to
                if (ProgressionController.Instance.currentLevel != 0) { // not sandbox
                    LevelController.Instance.SetAllWarriorThumbnailsGrey(GetPlacedWarriorCount() >= HelperController.Instance.GetCurrentLevelData().maxWarriorsToPlace);
                }
            }

            // if not dragging, show stats
            if (isDrag == false) {
                GameObject thumbnail = ThumbnailClicked();
                if (thumbnail != null) {
                    // SHOW STATS SCREEN BASED ON WARRIOR INDEX
                    LevelController.Instance.ShowStatsPanel(thumbnail.GetComponent<WarriorLevelThumbnail>().warriorIndex, thumbnail.GetComponent<WarriorLevelThumbnail>().isEnemy);
                }
                GameObject warrior = WarriorClicked();
                if (warrior != null) {
                    // SHOW STATS SCREEN BASED ON WARRIOR INDEX
                    LevelController.Instance.ShowStatsPanel(warrior.GetComponent<WarriorBehavior>().warriorIndex, warrior.GetComponent<WarriorBehavior>().isEnemy);
                }
            }
            clickTime = 0f;
            isDrag = false;
        }
    }

    // check if dropping object over UI
    private bool IsOverUI() {
        List<RaycastResult> overUI = HelperController.Instance.OverUI();
        for (int i = 0; i < overUI.Count; i++) {
            if (overUI[i].gameObject.tag == "notMainUI") {
                overUI.RemoveAt(i);
                i--;
            }
        }
        return overUI.Count > 0;
    }

    private bool IsTooManyWarriors() {
        // if sandbox, return false
        if (ProgressionController.Instance.currentLevel == 0) {
            return false;
        }

        // if too many placed warriors, return false
        int warriorCount = GetPlacedWarriorCount();
        if (warriorCount < ProgressionController.Instance.levelDataList[ProgressionController.Instance.currentLevel].maxWarriorsToPlace) {
            return false;
        }

        return true;
    }

    public int GetPlacedWarriorCount() {
        // loop through all warriors and enemies, only count if warrior
        int warriorCount = 0;
        foreach (KeyValuePair<GameObject, Vector2> warriorObject in LevelController.Instance.objectsOnGrid) {
            if (warriorObject.Key.tag == "warrior") {
                warriorCount += 1;
            }
        }
        return warriorCount;
    }

    private GameObject ThumbnailClicked() { // meant to be called only when a click occurs
        // raycast below mouse, return true if thumbnail below
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        GameObject thumbnail = null;
        for (int i = 0; i < raycastResults.Count; i++) {
            if (raycastResults[i].gameObject.tag == "thumbnail") {
                thumbnail = raycastResults[i].gameObject;
            }
        }

        return thumbnail;
    }

    private GameObject WarriorClicked() {
        // check if object at mouse position is a warrior or enemy
        GameObject warrior = null;
        Vector2 ray = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        RaycastHit2D hit = Physics2D.Raycast(ray, ray);

        if (hit.collider != null && (hit.collider.gameObject.tag == "warrior" || hit.collider.gameObject.tag == "enemy")) {
            warrior = hit.collider.gameObject;
        }
        return warrior;
    }

}
