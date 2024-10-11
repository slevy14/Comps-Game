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
    [SerializeField] private WarriorBehavior target;
    [SerializeField] private bool isMeleeHeal;
    [SerializeField] private bool isMeleeTargetAllies;
    [SerializeField] private bool isRangedHeal;
    [SerializeField] private bool isRangedTargetAllies;

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

    public void SetIsEnemy() {
        isEnemy = true;
        heading = new Vector2((int)-1, (int)0);
        this.gameObject.tag = "enemy";
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

    // Major Functions
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

    // Parser
    public IEnumerator RunBehaviorFunctions(List<BlockDataStruct> behaviorList) {
        for (int i = 0; i < behaviorList.Count; i++) {
            yield return new WaitForSeconds(LevelController.Instance.battleSpeed);
            switch (behaviorList[i].behavior) {
                case BlockData.BehaviorType.TURN:
                    // one dropdown
                    // rotate either left or right, set heading. affects what "forward" movement looks like [0]
                    // 0: left
                    // 1: right
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
                    // one dropdown
                    // move one tile in the chosen direction [0]
                    // 0: forward
                    // 1: backward
                    // 2: left
                    // 3: right
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
                    // one dropdown
                    // move two tiles in the chosen direction [0]
                    // 0: forward
                    // 1: backward
                    // 2: left
                    // 3: right
                    // can be used to run past existing units
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
                    // set where to teleport [0]
                    // 0: behind target
                    // 1: flank target
                    // 2: retreat

                    Vector2 teleportPos = new();
                    if (behaviorList[i].values[0] == "0") { // behind
                        if (target == null) {
                            break;
                        }
                        // get target position minus heading
                        teleportPos = new Vector2((int)LevelController.Instance.objectsOnGrid[target.gameObject].x, (int)LevelController.Instance.objectsOnGrid[target.gameObject].y) - target.heading;
                        // try set this position to new position
                        if (!LevelController.Instance.objectsOnGrid.ContainsValue(teleportPos) && PlacementSystem.Instance.tilemap.HasTile(new Vector3Int((int)teleportPos.x, (int)teleportPos.y, 0))) {
                            this.gameObject.transform.position = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)teleportPos.x, (int)teleportPos.y, 0));
                            LevelController.Instance.objectsOnGrid[this.gameObject] = teleportPos;
                            heading = target.heading;
                        } else {
                            Debug.Log("either tile full or would teleport off map");
                        }
                    } else if (behaviorList[i].values[0] == "1") { // flank
                        if (target = null) {
                            break;
                        }
                        // if left flank free:
                            // get target position plus rotated left heading
                            // try set this position to new position
                        // else if right flank free
                            // get target position plus rotated right heading
                            // try set this position to new position

                    } else { // retreat
                        // if ally, retreat to left wall
                        // if enemy, retreat to right wall
                    }


                    break;
                case BlockData.BehaviorType.MELEE_ATTACK:
                    // no dropdowns
                    Debug.Log("melee attack");
                    // use melee range to attack in facing direction

                    // range values and what they mean:
                    // 1: directly in front
                    // 2: two in a straight line
                    // 3: three wide
                    // 4: T-shape in front
                    // 5: 2x3 blocks in front
                    // clamp at 5



                    break;
                case BlockData.BehaviorType.SET_TARGET:
                    // two dropdowns
                    // choose team to target [1]
                    // 0: enemy
                    // 1: ally

                    // find warrior to set as target [0]
                    // 0: nearest
                    // 1: strongest
                    // 2: farthest
                    // 3: weakest

                    // assign to target value of this warrior
                    Debug.Log("target");

                    List<WarriorBehavior> targetTeam = new();
                    if (behaviorList[i].values[1] == "0") { // enemy target
                        targetTeam = LevelController.Instance.enemyWarriorsList;
                    } else { // ally target
                        targetTeam = LevelController.Instance.yourWarriorsList;
                    }

                    // check if empty (shouldn't ever be empty, but just in case):
                    if (targetTeam.Count == 0) {
                        break;
                    }

                    target = targetTeam[0];
                    float distance = -1;
                    if (target != this) {
                        distance = Vector2.Distance(LevelController.Instance.objectsOnGrid[this.gameObject], LevelController.Instance.objectsOnGrid[target.gameObject]);
                    }

                    if (behaviorList[i].values[0] == "0") { // nearest
                        // initialize nearest manhattan distance
                        if (distance == -1) {
                            distance = 1000;
                        }
                        // loop through target team:
                        foreach (WarriorBehavior warrior in targetTeam) {
                            if (warrior == this && targetTeam.Count != 1) {
                                continue;
                            }
                            // calculate manhattan distance from current pos to warrior
                            // if closer, replace object
                            if (Vector2.Distance(LevelController.Instance.objectsOnGrid[this.gameObject], LevelController.Instance.objectsOnGrid[warrior.gameObject]) < distance) {
                                distance = Vector2.Distance(LevelController.Instance.objectsOnGrid[this.gameObject], LevelController.Instance.objectsOnGrid[warrior.gameObject]);
                                target = warrior;
                                Debug.Log(warrior.warriorName + " was closer");
                            } else {
                                Debug.Log(warrior.warriorName + " was not closer");
                            }
                        }
                    } else if (behaviorList[i].values[0] == "1") { // strongest
                        // initialize strongest
                        // loop through target team:
                            // calculate strongest (is this just most health? highest attack? combination?)
                            // if stronger, replace object
                        // set target to strongest
                        Debug.Log("will set target to strongest, for now setting to first in list");

                    } else if (behaviorList[i].values[0] == "2") { // farthest
                        // initialize farthest manhattan distance
                        if (distance == -1) {
                            distance = 0;
                        }
                        // loop through target team:
                        foreach (WarriorBehavior warrior in targetTeam) {
                            if (warrior == this && targetTeam.Count != 1) {
                                continue;
                            }
                            // calculate manhattan distance from current pos to warrior
                            // if farther, replace object
                            if (Vector2.Distance(LevelController.Instance.objectsOnGrid[this.gameObject], LevelController.Instance.objectsOnGrid[warrior.gameObject]) > distance) {
                                distance = Vector2.Distance(LevelController.Instance.objectsOnGrid[this.gameObject], LevelController.Instance.objectsOnGrid[warrior.gameObject]);
                                target = warrior;
                                Debug.Log(warrior.warriorName + " was further");
                            } else {
                                Debug.Log(warrior.warriorName + " was not further");
                            }
                        }
                    } else { // weakest
                        // initialize weakest
                        // loop through target team:
                            // calculate strongest (is this just most health? highest attack? combination?)
                            // if weaker, replace object
                        // set target to weakest
                        Debug.Log("will set target to weakest, for now setting to first in list");
                    }
                    Debug.Log("set target to " + target.warriorName);
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

    // Helper Functions
    public Vector2 RotateLeft(Vector2 vector) {
        return new Vector2((int)(vector.x * Mathf.Cos(Mathf.Deg2Rad*90) - vector.y * Mathf.Sin(Mathf.Deg2Rad*90)),
                           (int)(vector.x * Mathf.Sin(Mathf.Deg2Rad*90) + vector.y * Mathf.Cos(Mathf.Deg2Rad*90)));
    } 
    public Vector2 RotateRight(Vector2 vector) {
        return new Vector2((int)(vector.x * Mathf.Cos(Mathf.Deg2Rad*(-90)) - vector.y * Mathf.Sin(Mathf.Deg2Rad*(-90))),
                           (int)(vector.x * Mathf.Sin(Mathf.Deg2Rad*(-90)) + vector.y * Mathf.Cos(Mathf.Deg2Rad*(-90))));
    }
}
