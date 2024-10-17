using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    [SerializeField] private Dictionary<int, bool> conditionsDict;
    [SerializeField] private Dictionary<int, int> forCounters;

    private int MAX_INFINITY_COUNTER = 10;
    [SerializeField] private Dictionary<int, int> infinityCounters; // prevent infinite looping

    [Space(20)]
    [Header("References")]
    private Sprite sprite;
    private InputManager inputManager;
    private Slider healthBar;

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
        propertiesDict = new Dictionary<BlockData.Property, float>();
        conditionsDict = new();
        forCounters = new();

        infinityCounters = new();

        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();

        // first child is visual
        sprite = transform.GetChild(0).GetComponent<Sprite>();
        healthBar = transform.GetChild(1).transform.GetChild(0).GetComponent<Slider>();

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
                LevelController.Instance.objectsOnGrid.Remove(this.gameObject);
                Destroy(this.gameObject);
                break;
            default:
                break;
        }
        GridSaveLoader.Instance.SaveGridToJSON();
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
                    if (newVal > 0) {
                        healthBar.maxValue = newVal;
                        healthBar.value = healthBar.maxValue;
                    }
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

    /*----------------------------------------*/
    /*                                        */
    /*          BEHAVIOR INTERPRETER          */
    /*                                        */
    /*----------------------------------------*/

    // DONE: Turn, Step, Run, Teleport, For Loop, End Loop, Melee Settings, Ranged Settings, End If, Else
    // IN PROGRESS: Set Target, While Loop, If
    // NOT STARTED: Melee Attack, Fire Projectile

    // Major Functions
    public IEnumerator Move() {
        // reset target each time bc it's coded in
        // this will prevent targeting errors after destroying a warrior
        target = null;
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

    // Parser:
    // loop through all behavior, run code for each as needed
    public IEnumerator RunBehaviorFunctions(List<BlockDataStruct> behaviorList) {
        conditionsDict.Clear();
        forCounters.Clear();
        infinityCounters.Clear();
        for (int i = 0; i < behaviorList.Count; i++) {
            Debug.Log("i is " + i);
            yield return new WaitForSeconds(LevelController.Instance.battleSpeed);
            switch (behaviorList[i].behavior) {
                /*------------*/
                /*    TURN    */
                /*------------*/ /*   Current Status: DONE
                one dropdown
                    choose which direction to rotate [0]
                        0: left
                        1: right
                rotate either left or right, set heading. affects what "forward" movement looks like */
                case BlockData.BehaviorType.TURN:
                    Debug.Log("turn");
                    Debug.Log("heading before turn: " + heading);
                    // List<Vector2> possibleHeadings = new List<Vector2> {new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(0, 1)};
                    // int headingIndex = possibleHeadings.IndexOf(heading);
                    Vector2 newHeading = heading;
                    if (behaviorList[i].values[0] == "0") { // TURN LEFT
                        newHeading = RotateLeft(heading);
                        Debug.Log("after left rotation, " + newHeading);
                    } else { // TURN RIGHT
                        newHeading = RotateRight(heading);
                        Debug.Log("after right rotation, " + newHeading);
                    }
                    this.heading = newHeading;
                    // Debug.Log("heading after turn: " + heading);
                    break;

                /*------------*/
                /*    STEP    */
                /*------------*/ /*   Current Status: DONE
                one dropdown
                    choose direction [0]
                        0: forward
                        1: backward
                        2: left
                        3: right
                move one tile in the chosen direction */
                case BlockData.BehaviorType.STEP:
                    Debug.Log("step");
                    Vector2 stepPos = new Vector2((int)LevelController.Instance.objectsOnGrid[this.gameObject].x, (int)LevelController.Instance.objectsOnGrid[this.gameObject].y);
                    // Debug.Log("current pos: " + LevelController.Instance.objectsOnGrid[this.gameObject]);
                    if (behaviorList[i].values[0] == "0") { // FORWARD
                        // do something with heading
                        stepPos += heading;
                        // Debug.Log("newPos forward: " + stepPos);
                    } else if (behaviorList[i].values[0] == "1") { // BACKWARD
                        stepPos -= heading;
                    } else if (behaviorList[i].values[0] == "2") { // LEFT
                        stepPos += RotateLeft(heading);
                    } else { // RIGHT
                        stepPos += RotateRight(heading);
                    }
                    if (!LevelController.Instance.objectsOnGrid.ContainsValue(stepPos) && PlacementSystem.Instance.tilemap.HasTile(new Vector3Int((int)stepPos.x, (int)stepPos.y, 0))) {
                        this.gameObject.transform.position = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)stepPos.x, (int)stepPos.y, 0));
                        LevelController.Instance.objectsOnGrid[this.gameObject] = stepPos;
                    } else {
                        Debug.Log("either tile full or would move off map");
                    }
                    break;

                /*-----------*/
                /*    RUN    */
                /*-----------*/ /*   Current Status: Done
                one dropdown
                    choose direction [0]
                        0: forward
                        1: backward
                        2: left
                        3: right
                move two tiles in one direction. can be used to run past existing units */
                case BlockData.BehaviorType.RUN:
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
                        runPos += RotateLeft(heading);
                        runPos += RotateLeft(heading);
                    } else { // RIGHT
                        runPos += RotateRight(heading);
                        runPos += RotateRight(heading);
                    }
                    if (!LevelController.Instance.objectsOnGrid.ContainsValue(runPos) && PlacementSystem.Instance.tilemap.HasTile(new Vector3Int((int)runPos.x, (int)runPos.y, 0))) {
                        this.gameObject.transform.position = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)runPos.x, (int)runPos.y, 0));
                        LevelController.Instance.objectsOnGrid[this.gameObject] = runPos;
                    } else {
                        Debug.Log("either tile full or would move off map");
                    }
                    break;

                /*----------------*/
                /*    TELEPORT    */
                /*----------------*/ /*   Current Status: Done
                one dropdown
                    set where to teleport [0]
                        0: behind target
                        1: flank target
                sneak behind enemies */
                case BlockData.BehaviorType.TELEPORT:
                    Debug.Log("teleport");
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
                        if (target == null) {
                            break;
                        }
                        // if left flank free:
                        // rotate left and add to target position to check flank
                        Vector2 leftFlank = new Vector2((int)(LevelController.Instance.objectsOnGrid[target.gameObject].x + RotateLeft(target.heading).x), (int)(LevelController.Instance.objectsOnGrid[target.gameObject].y + RotateLeft(target.heading).y));
                        if (!LevelController.Instance.objectsOnGrid.ContainsValue(leftFlank) && PlacementSystem.Instance.tilemap.HasTile(new Vector3Int((int)leftFlank.x, (int)leftFlank.y, 0))) {
                            // get target position plus rotated left heading
                            // try set this position to new position
                            this.gameObject.transform.position = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)leftFlank.x, (int)leftFlank.y, 0));
                            LevelController.Instance.objectsOnGrid[this.gameObject] = leftFlank;
                            heading = RotateLeft(target.heading);
                            break;
                        }
                        // else if right flank free
                        Vector2 rightFlank = new Vector2((int)(LevelController.Instance.objectsOnGrid[target.gameObject].x + RotateRight(target.heading).x), (int)(LevelController.Instance.objectsOnGrid[target.gameObject].y + RotateRight(target.heading).y));
                        if (!LevelController.Instance.objectsOnGrid.ContainsValue(rightFlank) && PlacementSystem.Instance.tilemap.HasTile(new Vector3Int((int)rightFlank.x, (int)rightFlank.y, 0))) {
                            // get target position plus rotated right heading
                            // try set this position to new position
                            this.gameObject.transform.position = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)rightFlank.x, (int)rightFlank.y, 0));
                            LevelController.Instance.objectsOnGrid[this.gameObject] = rightFlank;
                            heading = RotateRight(target.heading);
                            break;
                        }
                    }
                    break;

                /*--------------------*/
                /*    MELEE ATTACK    */
                /*--------------------*/ /*   Current Status: NOT STARTED
                no dropdowns,
                use melee range values to attack in facing direction:
                    1: directly in front
                    2: two in a straight line
                    3: three wide
                    4: T-shape in front
                    5: 2x3 blocks in front
                    clamp at 5
                do the actual melee attack*/
                case BlockData.BehaviorType.MELEE_ATTACK:
                    // 
                    Debug.Log("melee attack");
                    // 



                    break;

                /*------------------*/
                /*    SET TARGET    */
                /*------------------*/ /*   Current Status: IN PROGRESS
                two dropdowns
                    choose team to target [1]
                        0: enemy
                        1: ally

                    find warrior to set as target [0]
                        0: nearest
                        1: strongest
                        2: farthest
                        3: weakest
                pick a warrior to target with various attacks */
                case BlockData.BehaviorType.SET_TARGET:
                    

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

                /*------------------*/
                /*    WHILE LOOP    */
                /*------------------*/ /*   Current Status: IN PROGRESS
                two dropdowns
                    choose condition [0]
                        0: target in range
                        1: target low health

                    set boolean [1]
                        0: true
                        1: false
                    
                    JUMP INDEX [2]
                basic while looping */
                case BlockData.BehaviorType.WHILE_LOOP:
                    Debug.Log("while loop");
                    if (!infinityCounters.ContainsKey(i)) {
                        infinityCounters[i] = MAX_INFINITY_COUNTER;
                    }
                    // check condition
                    // add index and result to conditions dict
                    bool whileLoopCondition = behaviorList[i].values[1] == "0" ? true : false;

                    if (behaviorList[i].values[0] == "0") { // target in range
                        conditionsDict[i] = TargetInRange(i) == whileLoopCondition;
                        Debug.Log("looping for target in range at index " + i);
                    } else if (behaviorList[i].values[0] == "1") { // target low health
                        conditionsDict[i] = TargetLowHealth(i) == whileLoopCondition;
                        Debug.Log("looping for target low heatlh at index " + i);
                    }

                    // if false just jump past
                    if (conditionsDict[i] == false || infinityCounters[i] <= 0) {
                        infinityCounters[i] = MAX_INFINITY_COUNTER;
                        Debug.Log("resetting infinity counter for index: " + i);
                        i = int.Parse(behaviorList[i].values[2]) - 1;
                        continue;
                    }
                    infinityCounters[i]--;
                    Debug.Log("ticking down infinity counter at " + i + " is " + infinityCounters[i]);
                    break;

                /*----------------*/
                /*    FOR LOOP    */
                /*----------------*/ /*   Current Status: Done
                input field
                    set loop amount [0]
                        0: loop amount

                    JUMP INDEX [1]
                loop x times */
                case BlockData.BehaviorType.FOR_LOOP:
                    Debug.Log("for loop");
                    if (!forCounters.ContainsKey(i)) {
                        forCounters[i] = int.Parse(behaviorList[i].values[0]);
                    }

                    if (forCounters[i] <= 0) {
                        i = int.Parse(behaviorList[i].values[1]) - 1;
                        continue;
                    }
                    forCounters[i]--;
                    Debug.Log("ticking down for counter at " + i + " to " + forCounters[i]);
                    break;

                /*----------------*/
                /*    END LOOP    */
                /*----------------*/ /*   Current Status: Done
                no dropdowns

                    JUMP INDEX [0]
                end either a for or while loop*/
                case BlockData.BehaviorType.END_LOOP:
                    // if conditions dict at jump index false, jump back
                    // else continue
                    // if (conditionsDict[int.Parse(behaviorList[i].values[0])])

                    // OR
                    // just jump back to the beginning because the loop jumps past this
                    Debug.Log("end loop");

                    i = int.Parse(behaviorList[i].values[0])-1;

                    break;
                
                /*----------*/
                /*    IF    */
                /*----------*/ /*   Current Status: IN PROGRESS
                two dropdowns
                    choose condition [0]
                        0: option
                        1: option

                    set boolean [1]
                        0: true
                        1: false
                    
                    JUMP INDEX [2]
                basic conditional */
                case BlockData.BehaviorType.IF:
                    Debug.Log("if");

                    // check condition
                    // add index and result to conditions dict
                    bool ifCondition = behaviorList[i].values[1] == "0" ? true : false;

                    if (behaviorList[i].values[0] == "0") { // target in range
                        conditionsDict[i] = TargetInRange(i) == ifCondition;
                    } else if (behaviorList[i].values[0] == "1") { // target low health
                        conditionsDict[i] = TargetLowHealth(i) == ifCondition;
                    }

                    // if false just jump past
                    if (conditionsDict[i] == false) {
                        i = int.Parse(behaviorList[i].values[2]) - 1;
                        continue;
                    }
                    break;
                
                /*------------*/
                /*    ELSE    */
                /*------------*/ /*   Current Status: IN PROGRESS
                no dropdowns

                    JUMP INDEX [0]
                    END INDEX  [1]
                jumps to end if need to pass
                basic else case */
                case BlockData.BehaviorType.ELSE:
                    Debug.Log("else");
                    Debug.Log("checking to see if condition from IF at index " + behaviorList[i].values[0] + " is true");
                    if (conditionsDict[int.Parse(behaviorList[i].values[0])] == true) { // skip else, jump to endif
                        Debug.Log("try jump to " + (int.Parse(behaviorList[i].values[1]) - 1));
                        i = int.Parse(behaviorList[i].values[1]) - 1;
                        continue;
                    }
                    break;

                /*--------------*/
                /*    END IF    */
                /*--------------*/ /*   Current Status: Done
                no dropdowns

                    JUMP INDEX [0]
                this block does nothing itself, just a jump point
                end conditional */
                case BlockData.BehaviorType.END_IF:
                    Debug.Log("end if");
                    break;
                
                /*----------------------*/
                /*    MELEE SETTINGS    */
                /*----------------------*/ /*   Current Status: Done
                two dropdowns
                    choose if heal or attack [0]
                        0: attack
                        1: heal

                    choose which team to attack [1]
                        0: enemies
                        1: allies
                */
                case BlockData.BehaviorType.MELEE_SETTINGS:
                    Debug.Log("melee settings");
                    

                    isMeleeHeal = behaviorList[i].values[0] == "0" ? false : true;
                    isMeleeTargetAllies = behaviorList[i].values[1] == "0" ? false : true;
                    break;
                
                /*-----------------------*/
                /*    RANGED SETTINGS    */
                /*-----------------------*/ /*   Current Status: Done
                two dropdowns
                    choose if heal or attack [0]
                        0: attack
                        1: heal

                    choose which team to attack [1]
                        0: enemies
                        1: allies
                */
                case BlockData.BehaviorType.RANGED_SETTINGS:
                    Debug.Log("ranged settings");

                    isRangedHeal = behaviorList[i].values[0] == "0" ? false : true;
                    isRangedTargetAllies = behaviorList[i].values[1] == "0" ? false : true;
                    break;
                
                /*-----------------------*/
                /*    FIRE PROJECTILE    */
                /*-----------------------*/ /*   Current Status: NOT STARTED
                no dropdowns
                do the actual ranged attack */
                case BlockData.BehaviorType.FIRE_PROJECTILE:
                    
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

    public bool TargetInRange(int index) {
        // FIXME!!! placeholder always true
        return true;
    }

    public bool TargetLowHealth(int index) {
        // FIXME!!! placeholder always false
        return false;
    }
}
