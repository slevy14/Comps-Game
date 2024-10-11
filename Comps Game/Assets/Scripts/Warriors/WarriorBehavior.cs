using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;

public class WarriorBehavior : MonoBehaviour, IDragHandler {

    // codeable properties
    [Header("Properties")]
    [SerializeField] public string warriorName;
    // [SerializeField] private float health;
    // [SerializeField] private float defense;
    // [SerializeField] private float moveSpeed;
    // [SerializeField] private float meleeAttackPower;
    // [SerializeField] private float meleeAttackSpeed;
    // [SerializeField] private float meleeAttackRange;
    // [SerializeField] private float rangedAttackPower;
    // [SerializeField] private float rangedAttackSpeed;
    // [SerializeField] private float distancedRange;
    // [SerializeField] private float specialPower;
    // [SerializeField] private float specialSpeed;
    // [SerializeField] private float healPower;
    // [SerializeField] private float healSpeed;
    [SerializeField] private Dictionary<BlockData.Property, float> propertiesDict;

    [Space(20)]
    [Header("Hidden Properties")]
    [Description("properties that exist for making the warriors move but not editable by the player")]
    [SerializeField] private Vector2 heading;

    [Space(20)]
    [Header("References")]
    private Sprite sprite;
    private InputManager inputManager;

    [Header("Dragging")]
    private bool isDragging;
    private Vector3 initialPos;
    public bool isNew = true;

    public int warriorIndex;
    public bool isEnemy;

    // block lists
    private List<BlockDataStruct> propertiesData;
    private List<BlockDataStruct> moveData;
    private List<BlockDataStruct> useWeaponData;
    private List<BlockDataStruct> useSpecialData;


    // setup
    void Awake() {
        // SetProperties();
        propertiesDict = new Dictionary<BlockData.Property, float>();
        // InitializeProperties();


        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();

        // first child is visual
        sprite = transform.GetChild(0).GetComponent<Sprite>();

        heading = new Vector2((int)1, (int)0);
    }

    // DRAGGING
    // ondrag needed to start drag from ui
    public void OnDrag(PointerEventData eventData) {
        // transform.position = inputManager.GetSelectedMapPosition();
    }

    public void StartDrag() {
        if (isDragging) {
            return;
        }

        // Debug.Log("started drag");
        isDragging = true;
        // save initial position
        initialPos = transform.position;

        // visuals
        this.transform.localScale *= 1.5f;
    }

    public void EndDrag(int endIndex) { // index for switch
        isDragging = false;
        switch (endIndex) {
            case 0: // end over empty grid tile
                break;
            case 1: // end over full grid tile
            case 2: // end out of bounds
                transform.position = initialPos;
                break;
            case 3: // end over drawer
                Destroy(this.gameObject);
                break;
            default:
                break;
        }
        // visuals
        this.transform.localScale /= 1.5f;
    }
    


    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        // // DRAG
        // if (isDragging) { // this allows dragging from worldspace
        //     transform.position = inputManager.GetSelectedMapPosition();
        // }
        
    }

    private void InitializeProperties() {
        // set the value for each property to 0
        // name and none are spectial properties that will be set to 0 but those values don't mean anything
        foreach (BlockData.Property property in Enum.GetValues(typeof(BlockData.Property))) {
            // Debug.Log(property);
            propertiesDict[property] = 0;
        }
        // Debug.Log(this.propertiesDict);
    }

    public void SetPropertiesAndBehaviors(List<BlockDataStruct> properties, List<BlockDataStruct> move, List<BlockDataStruct> useWeapon, List<BlockDataStruct> useSpecials) {
        InitializeProperties();
        propertiesData = properties;
        SetProperties();

        moveData = move;
        useWeaponData = useWeapon;
        useSpecialData = useSpecials;
    }

    public float GetProperty(BlockData.Property property) {
        return propertiesDict[property];
    }


    // FUNCTIONS FROM WHITEBOARD HEADERS
    private void SetProperties() {
        for (int i = 0; i < propertiesData.Count; i++) {
            float newVal = 0;
            switch (propertiesData[i].property) {
                case BlockData.Property.NAME:
                    // explicitly set name variable
                    warriorName = propertiesData[i].values[0];
                    break;
                case BlockData.Property.HEALTH:
                    float.TryParse(propertiesData[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.HEALTH] = newVal;
                    break;
                case BlockData.Property.DEFENSE:
                    float.TryParse(propertiesData[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.DEFENSE] = newVal;
                    break;
                case BlockData.Property.MOVE_SPEED:
                    float.TryParse(propertiesData[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.MOVE_SPEED] = newVal;
                    break;
                case BlockData.Property.MELEE_ATTACK_RANGE:
                    float.TryParse(propertiesData[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.MELEE_ATTACK_RANGE] = newVal;
                    break;
                case BlockData.Property.MELEE_ATTACK_POWER:
                    float.TryParse(propertiesData[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.MELEE_ATTACK_POWER] = newVal;
                    break;
                case BlockData.Property.MELEE_ATTACK_SPEED:
                    float.TryParse(propertiesData[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.MELEE_ATTACK_SPEED] = newVal;
                    break;
                case BlockData.Property.DISTANCED_RANGE:
                    float.TryParse(propertiesData[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.DISTANCED_RANGE] = newVal;
                    break;
                case BlockData.Property.RANGED_ATTACK_POWER:
                    float.TryParse(propertiesData[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.RANGED_ATTACK_POWER] = newVal;
                    break;
                case BlockData.Property.RANGED_ATTACK_SPEED:
                    float.TryParse(propertiesData[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.RANGED_ATTACK_SPEED] = newVal;
                    break;
                case BlockData.Property.SPECIAL_POWER:
                    float.TryParse(propertiesData[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.SPECIAL_POWER] = newVal;
                    break;
                case BlockData.Property.SPECIAL_SPEED:
                    float.TryParse(propertiesData[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.SPECIAL_SPEED] = newVal;
                    break;
                case BlockData.Property.HEAL_POWER:
                    float.TryParse(propertiesData[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.HEAL_POWER] = newVal;
                    break;
                case BlockData.Property.HEAL_SPEED:
                    float.TryParse(propertiesData[i].values[0], out newVal);
                    propertiesDict[BlockData.Property.HEAL_SPEED] = newVal;
                    break;
                
            }
        }
    }

    /*------------------------------------*/
    /*          BEHAVIOR PARSING          */
    /*------------------------------------*/

    public IEnumerator Move() {
        Debug.Log("MOVE FUNCTIONS:");
        yield return StartCoroutine(RunBehaviorFunctions(moveData));
    }

    public IEnumerator UseWeapon() {
        Debug.Log("USE WEAPON FUNCTIONS:");
        yield return StartCoroutine(RunBehaviorFunctions(useWeaponData));
    }

    public IEnumerator UseSpecial() {
        Debug.Log("USE SPECIAL FUNCTIONS:");
        yield return StartCoroutine(RunBehaviorFunctions(useSpecialData));
    }

    public IEnumerator RunBehaviorFunctions(List<BlockDataStruct> behaviorList) {
        for (int i = 0; i < behaviorList.Count; i++) {
            yield return new WaitForSeconds(LevelController.Instance.battleSpeed);
            switch (behaviorList[i].behavior) {
                case BlockData.BehaviorType.TURN:
                    // rotate either left or right, set heading. affects what "forward" movement looks like
                    // one dropdown
                    Debug.Log("turn");
                    Debug.Log("heading before turn: " + heading);
                    // List<Vector2> possibleHeadings = new List<Vector2> {new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(0, 1)};
                    // int headingIndex = possibleHeadings.IndexOf(heading);
                    Vector2 newHeading = heading;
                    if (behaviorList[i].values[0] == "0") { // TURN LEFT
                        newHeading = new Vector2((int)(heading.x * Mathf.Cos(Mathf.Deg2Rad*90) - heading.y * Mathf.Sin(Mathf.Deg2Rad*90)),
                                                 (int)(heading.x * Mathf.Sin(Mathf.Deg2Rad*90) + heading.y * Mathf.Cos(Mathf.Deg2Rad*90)));
                        Debug.Log("after left rotation, " + newHeading);
                    } else { // TURN RIGHT
                        newHeading = new Vector2((int)(heading.x * Mathf.Cos(Mathf.Deg2Rad*(-90)) - heading.y * Mathf.Sin(Mathf.Deg2Rad*(-90))),
                                                 (int)(heading.x * Mathf.Sin(Mathf.Deg2Rad*(-90)) + heading.y * Mathf.Cos(Mathf.Deg2Rad*(-90))));
                        Debug.Log("after right rotation, " + newHeading);
                    }
                    this.heading = newHeading;
                    // Debug.Log("heading after turn: " + heading);
                    break;
                case BlockData.BehaviorType.STEP:
                    // move one tile in the chosen direction
                    // one dropdown
                    Debug.Log("step");
                    Vector2 stepPos = new Vector2((int)LevelController.Instance.objectsOnGrid[this.gameObject].x, (int)LevelController.Instance.objectsOnGrid[this.gameObject].y);
                    Debug.Log("current pos: " + LevelController.Instance.objectsOnGrid[this.gameObject]);
                    if (behaviorList[i].values[0] == "0") { // FORWARD
                        // do something with heading
                        stepPos += heading;
                        Debug.Log("newPos forward: " + stepPos);
                    } else if (behaviorList[i].values[0] == "1") { // BACKWARD
                        stepPos -= heading;
                    } else if (behaviorList[i].values[0] == "2") { // LEFT
                        stepPos += new Vector2((int)(heading.x * Mathf.Cos(Mathf.Deg2Rad*90) - heading.y * Mathf.Sin(Mathf.Deg2Rad*90)),
                                              (int)(heading.x * Mathf.Sin(Mathf.Deg2Rad*90) + heading.y * Mathf.Cos(Mathf.Deg2Rad*90)));
                    } else { // RIGHT
                        stepPos += new Vector2((int)(heading.x * Mathf.Cos(Mathf.Deg2Rad*(-90)) - heading.y * Mathf.Sin(Mathf.Deg2Rad*(-90))),
                                              (int)(heading.x * Mathf.Sin(Mathf.Deg2Rad*(-90)) + heading.y * Mathf.Cos(Mathf.Deg2Rad*(-90))));
                    }
                    if (!LevelController.Instance.objectsOnGrid.ContainsValue(stepPos) && PlacementSystem.Instance.tilemap.HasTile(new Vector3Int((int)stepPos.x, (int)stepPos.y, 0))) {
                        this.gameObject.transform.position = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)stepPos.x, (int)stepPos.y, 0));
                        LevelController.Instance.objectsOnGrid[this.gameObject] = stepPos;
                    } else {
                        Debug.Log("either tile full or would move off map");
                    }
                    break;
                case BlockData.BehaviorType.RUN:
                    // move two tiles in the chosen direction
                    // can be used to run past existing units
                    // one dropdown
                    Debug.Log("run");
                    Vector2 runPos = new Vector2((int)LevelController.Instance.objectsOnGrid[this.gameObject].x, (int)LevelController.Instance.objectsOnGrid[this.gameObject].y);
                    Debug.Log("current pos: " + LevelController.Instance.objectsOnGrid[this.gameObject]);
                    if (behaviorList[i].values[0] == "0") { // FORWARD
                        // do something with heading
                        runPos += heading;
                        runPos += heading;
                        Debug.Log("newPos forward: " + runPos);
                    } else if (behaviorList[i].values[0] == "1") { // BACKWARD
                        runPos -= heading;
                        runPos -= heading;
                    } else if (behaviorList[i].values[0] == "2") { // LEFT
                        runPos += new Vector2((int)(heading.x * Mathf.Cos(Mathf.Deg2Rad*90) - heading.y * Mathf.Sin(Mathf.Deg2Rad*90)),
                                              (int)(heading.x * Mathf.Sin(Mathf.Deg2Rad*90) + heading.y * Mathf.Cos(Mathf.Deg2Rad*90)));
                        runPos += new Vector2((int)(heading.x * Mathf.Cos(Mathf.Deg2Rad*90) - heading.y * Mathf.Sin(Mathf.Deg2Rad*90)),
                                              (int)(heading.x * Mathf.Sin(Mathf.Deg2Rad*90) + heading.y * Mathf.Cos(Mathf.Deg2Rad*90)));
                    } else { // RIGHT
                        runPos += new Vector2((int)(heading.x * Mathf.Cos(Mathf.Deg2Rad*(-90)) - heading.y * Mathf.Sin(Mathf.Deg2Rad*(-90))),
                                              (int)(heading.x * Mathf.Sin(Mathf.Deg2Rad*(-90)) + heading.y * Mathf.Cos(Mathf.Deg2Rad*(-90))));
                        runPos += new Vector2((int)(heading.x * Mathf.Cos(Mathf.Deg2Rad*(-90)) - heading.y * Mathf.Sin(Mathf.Deg2Rad*(-90))),
                                              (int)(heading.x * Mathf.Sin(Mathf.Deg2Rad*(-90)) + heading.y * Mathf.Cos(Mathf.Deg2Rad*(-90))));
                    }
                    if (!LevelController.Instance.objectsOnGrid.ContainsValue(runPos) && PlacementSystem.Instance.tilemap.HasTile(new Vector3Int((int)runPos.x, (int)runPos.y, 0))) {
                        this.gameObject.transform.position = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)runPos.x, (int)runPos.y, 0));
                        LevelController.Instance.objectsOnGrid[this.gameObject] = runPos;
                    } else {
                        Debug.Log("either tile full or would move off map");
                    }
                    break;
                case BlockData.BehaviorType.TELEPORT:
                    // one dropdown
                    Debug.Log("teleport");
                    break;
                case BlockData.BehaviorType.MELEE_ATTACK:
                    // no dropdowns
                    Debug.Log("melee attack");
                    break;
                case BlockData.BehaviorType.SET_TARGET:
                    // two dropdowns
                    Debug.Log("set target");
                    break;
                case BlockData.BehaviorType.WHILE_LOOP:
                    // two dropdowns
                    Debug.Log("while loop");
                    break;
                case BlockData.BehaviorType.FOR_LOOP:
                    // input field
                    Debug.Log("for loop");
                    break;
                case BlockData.BehaviorType.END_LOOP:
                    // no dropdowns
                    Debug.Log("end loop");
                    break;
                case BlockData.BehaviorType.IF:
                    // two dropdowns
                    Debug.Log("if");
                    break;
                case BlockData.BehaviorType.ELSE:
                    // no dropdowns
                    Debug.Log("else");
                    break;
                case BlockData.BehaviorType.END_IF:
                    // no dropdowns
                    Debug.Log("end if");
                    break;
                case BlockData.BehaviorType.MELEE_SETTINGS:
                    // two dropdowns
                    Debug.Log("melee settings");
                    break;
                case BlockData.BehaviorType.RANGED_SETTINGS:
                    // two dropdowns
                    Debug.Log("ranged settings");
                    break;
                case BlockData.BehaviorType.FIRE_PROJECTILE:
                    // no dropdowns
                    Debug.Log("fire projectile");
                    break;
            }
        }
    }
}
