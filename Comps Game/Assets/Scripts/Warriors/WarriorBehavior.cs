using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WarriorBehavior : MonoBehaviour, IDragHandler {

    // placed on the warrior prefab!
    // holds the actual interpreter for code blocks

    // codeable properties
    [Header("Properties")]
    [SerializeField] public string warriorName;
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
    [SerializeField] private bool magicShield;
    [SerializeField] private bool hasStamina;
    [SerializeField] private float maxHealth;
    [SerializeField] public bool isAlive = true;
    [SerializeField] public bool isCurrentTurn;
    [SerializeField] public int totalStrength;
    [SerializeField] public bool isValidStrengthAndBehaviors;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material greyOutMaterial;

    [Header("Conditionals/Looping")]
    private int MAX_INFINITY_COUNTER = 15;
    [SerializeField] private Dictionary<int, int> infinityCounters; // prevent infinite looping
    [SerializeField] private Dictionary<int, bool> conditionsDict;
    [SerializeField] private Dictionary<int, int> forCounters;
    [SerializeField] List<Vector2> tilesToHitRelative;

    [Header("attack visuals")]
    [SerializeField] private GameObject meleePrefab;
    [SerializeField] private Sprite damageSprite;
    [SerializeField] private Sprite healSprite;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Sprite projectileDamageSprite;
    [SerializeField] private Sprite projectileHealSprite;
    [SerializeField] private Animator animator;

    [Space(20)]
    [Header("References")]
    private Sprite sprite;
    private InputManager inputManager;
    private Slider healthBar;
    private GameObject pointer;
    [SerializeField] private GameObject staminaIndicator;
    [SerializeField] private GameObject noStaminaTextbox;

    [Header("Dragging")]
    private bool isDragging;
    private Vector3 initialPos;
    public Vector2 initialGridPos;
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
        // initialize dictionaries
        propertiesDict = new Dictionary<BlockData.Property, float>();
        conditionsDict = new();
        forCounters = new();

        infinityCounters = new();

        // get reference to input manager
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();

        // get reference to children on this object
        sprite = transform.GetChild(0).GetComponent<Sprite>();
        healthBar = transform.GetChild(1).transform.GetChild(0).GetComponent<Slider>();
        pointer = transform.GetChild(2).gameObject;
        SetImageFacing();

        // set default vectors
        heading = new Vector2((int)1, (int)0);
        tilesToHitRelative = new List<Vector2>{new Vector2((int)1, (int)0), new Vector2((int)2, (int)0), new Vector2((int)1, (int)1), new Vector2((int)1, (int)-1), new Vector2((int)2, (int)1), new Vector2((int)2, (int)-1)};
        
        // set default animator speed
        animator.speed = 2.01f - LevelController.Instance.battleSpeed;
    }

    public void SetIsEnemy() {
        // set enemy visuals
        isEnemy = true;
        heading = new Vector2((int)-1, (int)0);
        SetImageFacing();
        this.gameObject.tag = "enemy";
        healthBar.gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color(167f/255f, 85/255f, 255/255f);
        healthBar.gameObject.transform.GetChild(2).GetComponent<TMP_Text>().color = Color.white;
    }

    // DRAGGING
    public void OnDrag(PointerEventData eventData) {
        // needed to start drag from ui
        // doesn't do anything otherwise
    }

    public void StartDrag() {
        // if already dragging, return
        if (isDragging) {
            return;
        }

        // start drag
        isDragging = true;
        // save initial position
        initialPos = transform.position;

        // remove this object from grid when picked up
        if (LevelController.Instance.objectsOnGrid.ContainsKey(this.gameObject)) {
            Debug.Log("former position for dragging object: " + LevelController.Instance.objectsOnGrid[this.gameObject]);
            initialGridPos = LevelController.Instance.objectsOnGrid[this.gameObject];
            LevelController.Instance.objectsOnGrid.Remove(this.gameObject);
        }

        // rescale to look picked up
        this.transform.localScale *= 1.5f;

        // play sound
        AudioController.Instance.PlaySoundEffect("Warrior Pickup");
    }

    public void EndDrag(int endIndex) { // index for switch
        // determine what to do after drag (destroy if need to)
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

        // save grid with warrior on it
        GridSaveLoader.Instance.SaveGridToJSON();

        // rescale visuals to place
        this.transform.localScale /= 1.5f;

        // play sound
        AudioController.Instance.PlaySoundEffect("Warrior Drop");
    }

    // Highlight and Un-highlight on mouseover

    void OnMouseEnter() {
        transform.GetChild(0).GetComponent<SpriteOutline>().enabled = true;
    }

    void OnMouseExit() {
        transform.GetChild(0).GetComponent<SpriteOutline>().enabled = false;
    }


    
    // Update is called once per frame
    void Update() {
        // update battle speed as changed mid-battle
        animator.speed = 2.01f - LevelController.Instance.battleSpeed;
        
    }

    private void InitializeProperties() {
        // set the value for each property to 0
        // name and none are spectial properties that will be set to 0 but those values don't mean anything
        foreach (BlockData.Property property in Enum.GetValues(typeof(BlockData.Property))) {
            propertiesDict[property] = 0;
        }
    }

    public void MarkCurrentTurn(bool value) {
        // activate or deactivate turn
        isCurrentTurn = value;
        pointer.SetActive(value);
        ToggleStaminaIndicator(true);
    }

    public void SetPropertiesAndBehaviors(List<BlockDataStruct> properties, List<BlockDataStruct> move, List<BlockDataStruct> useWeapon, List<BlockDataStruct> useSpecials) {
        // intialize data on this object from stored data lists
        InitializeProperties();
        propertiesData = properties;
        SetProperties();

        moveData = move;
        useWeaponData = useWeapon;
        useSpecialData = useSpecials;

        // set health visual
        UpdateHealthDisplay();
        // recalculate strength for this object
        totalStrength = CalculateTotalStrength();
    }

    public void UpdateHealthDisplay() {
        // update health bar, used when taking damage or healing
        healthBar.transform.GetChild(2).GetComponent<TMP_Text>().text = Mathf.CeilToInt(propertiesDict[BlockData.Property.HEALTH]) + " / " + maxHealth;
    }

    public float GetProperty(BlockData.Property property) {
        return propertiesDict[property];
    }

    public int CalculateTotalStrength() {
        return HelperController.Instance.CalculateWarriorStrength((int)propertiesDict[BlockData.Property.MELEE_ATTACK_POWER], (int)propertiesDict[BlockData.Property.MELEE_ATTACK_RANGE], (int)propertiesDict[BlockData.Property.HEAL_POWER], (int)propertiesDict[BlockData.Property.RANGED_ATTACK_POWER], (int)propertiesDict[BlockData.Property.MOVE_SPEED], (int)maxHealth, (int)propertiesDict[BlockData.Property.DEFENSE]);
    }

    public void CheckValidOnGrid() {
        // check if warrior preexisting on grid is valid strength and behaviors
        // automatically true if in sandbox or enemy
        if (isEnemy || ProgressionController.Instance.currentLevel == 0) {
            isValidStrengthAndBehaviors = true;
            return;
        }

        if (!HelperController.Instance.ValidateBehaviorCount(warriorIndex) || !HelperController.Instance.ValidateStrength(warriorIndex)) {
            isValidStrengthAndBehaviors = false;
            transform.GetChild(0).GetComponent<SpriteRenderer>().material = greyOutMaterial;
        } else {
            isValidStrengthAndBehaviors = true;
        }
    }

    private void SetProperties() {
        // loop through all stored property blocks
        for (int i = 0; i < propertiesData.Count; i++) {
            float newVal = 0;
            // for each property, parse its value as a string
            // store data to properties dict
            // store data to additional vars as needed
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
                        maxHealth = newVal;
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
                case BlockData.Property.MAGIC_SHIELD:
                    magicShield = true;
                    break;
                
            }
        }
    }

    /*----------------------------------------*/
    /*                                        */
    /*          BEHAVIOR INTERPRETER          */
    /*                                        */
    /*----------------------------------------*/

    // DONE: Turn, Step, Run, Teleport, For Loop, End Loop, Melee Settings, Ranged Settings, End If, Else, Set Target, While Loop, If, Melee Attack, Fire Projectile, Recharge Stamina
    // IN PROGRESS: 
    // NOT STARTED: Foreach

    // // Possible future refactor: add projectile range

    // Major Functions
    public IEnumerator Move() {
        // reset target each time because it's coded in
        // this will prevent targeting errors after destroying a warrior
        target = null;
        Debug.Log("MOVE FUNCTIONS:");
        // run all move functions first
        if (gameObject.activeSelf) {
            yield return StartCoroutine(RunBehaviorFunctions(moveData));
        }
    }

    public IEnumerator UseWeapon() {
        Debug.Log("USE WEAPON FUNCTIONS:");
        // run all use weapon functions second
        if (gameObject.activeSelf) {
            yield return StartCoroutine(RunBehaviorFunctions(useWeaponData));
        }
    }

    // Not in use -- for a future update (additional header that would have different functionality)
    public IEnumerator UseSpecial() {
        Debug.Log("USE SPECIAL FUNCTIONS:");
        if (gameObject.activeSelf) {
            yield return StartCoroutine(RunBehaviorFunctions(useSpecialData));
        }
    }

    // Interpreter:
    // loop through all behaviors, run code for each as needed
    public IEnumerator RunBehaviorFunctions(List<BlockDataStruct> behaviorList) {
        // reset counters
        conditionsDict.Clear(); // any stored conditions to check
        forCounters.Clear(); // any for loops that may have been started
        infinityCounters.Clear(); // any counters for infinite while loops that may have been started
        // loop through all functions in behavior list
        for (int i = 0; i < behaviorList.Count; i++) {
            // quit if battle over
            if (!isAlive || LevelController.Instance.battleFinished) {
                break;
            }

            // wait between actions
            yield return new WaitForSeconds(LevelController.Instance.battleSpeed);

            // hold if paused
            while (LevelController.Instance.isPaused) {
                yield return new WaitForSeconds(LevelController.Instance.battleSpeed);
            }
            // hold if active projectile
            while (LevelController.Instance.activeProjectile != null) {
                yield return new WaitForSeconds(LevelController.Instance.battleSpeed);
            }

            // hold if warrior in death delay
            while (LevelController.Instance.activeDeathDelay) {
                yield return new WaitForSeconds(LevelController.Instance.battleSpeed);
            }

            // switch on behavior
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
                    Vector2 newHeading = heading;
                    // change heading based on value
                    if (behaviorList[i].values[0] == "0") { // TURN LEFT
                        newHeading = RotateLeft(heading);
                    } else { // TURN RIGHT
                        newHeading = RotateRight(heading);
                    }
                    this.heading = newHeading;
                    // update visual to match new heading
                    SetImageFacing();
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
                    // use heading to determine relative movement step
                    if (behaviorList[i].values[0] == "0") { // FORWARD
                        stepPos += heading;
                    } else if (behaviorList[i].values[0] == "1") { // BACKWARD
                        stepPos -= heading;
                    } else if (behaviorList[i].values[0] == "2") { // LEFT
                        stepPos += RotateLeft(heading);
                    } else { // RIGHT
                        stepPos += RotateRight(heading);
                    }
                    // move if able to
                    if (!LevelController.Instance.objectsOnGrid.ContainsValue(stepPos) && PlacementSystem.Instance.tilemap.HasTile(new Vector3Int((int)stepPos.x, (int)stepPos.y, 0))) {
                        this.gameObject.transform.position = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)stepPos.x, (int)stepPos.y, 0));
                        LevelController.Instance.objectsOnGrid[this.gameObject] = stepPos;
                    } else { // otherwise, don't move
                        Debug.Log("either tile full or would move off map");
                    }
                    // play sound
                    AudioController.Instance.PlaySoundEffect("Footsteps");
                    break;

                /*-----------*/
                /*    RUN    */
                /*-----------*/ /*   Current Status: DONE
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
                    // use heading to determine relative movement step
                    if (behaviorList[i].values[0] == "0") { // FORWARD
                        runPos += heading;
                        runPos += heading;
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
                    // move if able to
                    if (!LevelController.Instance.objectsOnGrid.ContainsValue(runPos) && PlacementSystem.Instance.tilemap.HasTile(new Vector3Int((int)runPos.x, (int)runPos.y, 0))) {
                        this.gameObject.transform.position = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)runPos.x, (int)runPos.y, 0));
                        LevelController.Instance.objectsOnGrid[this.gameObject] = runPos;
                    } else { // otherwise, don't move
                        Debug.Log("either tile full or would move off map");
                    }
                    // play sound
                    AudioController.Instance.PlaySoundEffect("Footsteps");
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
                    // break if no target
                    if (target == null) {
                        IndicateNoTarget();
                        break;
                    }
                    Vector2 teleportPos = new();
                    if (behaviorList[i].values[0] == "0") { // behind
                        // break if target stops existing at an inconvenient time
                        try {
                            // get target position minus heading
                            teleportPos = new Vector2((int)LevelController.Instance.objectsOnGrid[target.gameObject].x, (int)LevelController.Instance.objectsOnGrid[target.gameObject].y) - target.heading;
                            // try set this position to new position
                            if (!LevelController.Instance.objectsOnGrid.ContainsValue(teleportPos) && PlacementSystem.Instance.tilemap.HasTile(new Vector3Int((int)teleportPos.x, (int)teleportPos.y, 0))) {
                                this.gameObject.transform.position = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)teleportPos.x, (int)teleportPos.y, 0));
                                LevelController.Instance.objectsOnGrid[this.gameObject] = teleportPos;
                            } else { // otherwise, don't move
                                Debug.Log("either tile full or would teleport off map");
                            }
                        } catch {
                            IndicateNoTarget();
                            break;
                        }
                    } else if (behaviorList[i].values[0] == "1") { // flank
                        // break if target stops existing at an inconvenient time
                        try {
                            // if left flank free:
                            // add to target position to check flank
                            Vector2 leftFlank = new Vector2((int)(LevelController.Instance.objectsOnGrid[target.gameObject].x + RotateLeft(target.heading).x), (int)(LevelController.Instance.objectsOnGrid[target.gameObject].y + RotateLeft(target.heading).y));
                            if (!LevelController.Instance.objectsOnGrid.ContainsValue(leftFlank) && PlacementSystem.Instance.tilemap.HasTile(new Vector3Int((int)leftFlank.x, (int)leftFlank.y, 0))) {
                                // get target position plus rotated left heading
                                // try set this position to new position
                                this.gameObject.transform.position = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)leftFlank.x, (int)leftFlank.y, 0));
                                LevelController.Instance.objectsOnGrid[this.gameObject] = leftFlank;
                                break;
                            }
                            // else if right flank free
                            Vector2 rightFlank = new Vector2((int)(LevelController.Instance.objectsOnGrid[target.gameObject].x + RotateRight(target.heading).x), (int)(LevelController.Instance.objectsOnGrid[target.gameObject].y + RotateRight(target.heading).y));
                            if (!LevelController.Instance.objectsOnGrid.ContainsValue(rightFlank) && PlacementSystem.Instance.tilemap.HasTile(new Vector3Int((int)rightFlank.x, (int)rightFlank.y, 0))) {
                                // get target position plus rotated right heading
                                // try set this position to new position
                                this.gameObject.transform.position = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)rightFlank.x, (int)rightFlank.y, 0));
                                LevelController.Instance.objectsOnGrid[this.gameObject] = rightFlank;
                                break;
                            }
                        } catch {
                            IndicateNoTarget();
                            break;
                        }
                    }
                    // play sound
                    AudioController.Instance.PlaySoundEffect("Teleport");
                    break;

                /*--------------------*/
                /*    MELEE ATTACK    */
                /*--------------------*/ /*   Current Status: DONE
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
                    Debug.Log("melee attack");
                    // don't attack if no stamina
                    if (!UseStamina()) {
                        break;
                    }

                    // get list of relative range to hit
                    List<Vector2> adjustedList = GetMeleeRange();

                    // play attack animation
                    this.animator.SetTrigger("Attack");
                    yield return new WaitForSeconds(LevelController.Instance.battleSpeed/1.5f);

                    // loop through adjusted list:
                        // deal damage or heal to square if enemy or ally is there
                    foreach (Vector2 tile in adjustedList) {
                        // show attack visuals
                        Vector2 tileToAttack = new Vector2((int)(LevelController.Instance.objectsOnGrid[this.gameObject].x + tile.x), (int)(LevelController.Instance.objectsOnGrid[this.gameObject].y + tile.y));
                        Vector3 meleeIconPlacement = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)tileToAttack.x, (int)tileToAttack.y, 0));
                        meleeIconPlacement.z = -1; // keep on top
                        GameObject icon = Instantiate(meleePrefab, meleeIconPlacement, transform.rotation, this.transform);
                        if (isMeleeHeal) {
                            icon.GetComponent<SpriteRenderer>().sprite = healSprite;
                        }
                        // if tile has something to hit, hit warrior in tile
                        if (LevelController.Instance.objectsOnGrid.ContainsValue(tileToAttack)) {
                            WarriorBehavior hitWarrior = LevelController.Instance.objectsOnGrid.FirstOrDefault(x => x.Value == tileToAttack).Key.GetComponent<WarriorBehavior>();
                            if (hitWarrior.isEnemy == this.isMeleeTargetAllies) {
                                continue;
                            }
                            if (isMeleeHeal) {
                                hitWarrior.DoDamageOrHeal(this.propertiesDict[BlockData.Property.HEAL_POWER], isMeleeHeal);
                            } else {
                                hitWarrior.DoDamageOrHeal(this.propertiesDict[BlockData.Property.MELEE_ATTACK_POWER], isMeleeHeal);
                            }
                        }
                    }
                    // play sound
                    AudioController.Instance.PlaySoundEffect("Melee Attack");
                    break;

                /*------------------*/
                /*    SET TARGET    */
                /*------------------*/ /*   Current Status: Done
                two dropdowns
                    choose team to target [1]
                        0: enemy
                        1: ally

                    find warrior to set as target [0]
                        0: nearest
                        1: strongest
                        2: farthest
                        3: weakest
                        4: random
                        5: healthiest
                        6: frailest
                pick a warrior to target with various attacks */
                case BlockData.BehaviorType.SET_TARGET:
                    Debug.Log("target");

                    // assign to target value of this warrior
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

                    // set default target to first in list
                    target = targetTeam[0];
                    float distance = -1;
                    // store distance between this warrior and its target
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
                            }
                        }
                    } else if (behaviorList[i].values[0] == "1") { // strongest
                        // initialize strongest
                        // loop through target team:
                            // calculate strongest (is this just most health? highest attack? combination?)
                            // if stronger, replace object
                        // set target to strongest
                        target = CalculateStrongest(targetTeam);
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
                            }
                        }
                    } else if (behaviorList[i].values[0] == "3") { // weakest
                        // initialize weakest
                        // loop through target team:
                            // calculate strongest (is this just most health? highest attack? combination?)
                            // if weaker, replace object
                        // set target to weakest
                        target = CalculateWeakest(targetTeam);
                    } else if (behaviorList[i].values[0] == "4") { // random
                        // pick a random warrior from target team
                        int randIndex = UnityEngine.Random.Range(0, targetTeam.Count);
                        target = targetTeam[randIndex];
                    } else if (behaviorList[i].values[0] == "5") { // healthiest
                        // loop through target team
                            // find warrior with most health
                        target = CalculateHealthiest(targetTeam);
                    } else if (behaviorList[i].values[0] == "6") { // frailest
                        // loop through target team
                            // find warrior with least health
                        target = CalculateFrailest(targetTeam);
                    }
                    Debug.Log("set target to " + target.warriorName);
                    break;

                /*------------------*/
                /*    WHILE LOOP    */
                /*------------------*/ /*   Current Status: DONE
                two dropdowns
                    choose condition [0]
                        0: target in range
                        1: target low health
                        2: facing target
                        3: self low health
                        4: target is healer
                        5: target is melee
                        6: target is ranged
                        7: target has shield
                        8: in target range


                    set boolean [1]
                        0: true
                        1: false
                    
                    JUMP INDEX [2]
                basic while looping */
                case BlockData.BehaviorType.WHILE_LOOP:
                    Debug.Log("while loop");
                    // add this loop to the infinity counters dict
                    if (!infinityCounters.ContainsKey(i)) {
                        infinityCounters[i] = MAX_INFINITY_COUNTER;
                    }
                    // check if condition matches expected result ("0"==true, "1"==false)
                    // add index and result to conditions dict
                    bool whileLoopCondition = behaviorList[i].values[1] == "0" ? true : false;

                    if (behaviorList[i].values[0] == "0") { // target in range
                        conditionsDict[i] = TargetInRange() == whileLoopCondition;
                        Debug.Log("looping for target in range at index " + i);
                    } else if (behaviorList[i].values[0] == "1") { // target low health
                        conditionsDict[i] = TargetLowHealth() == whileLoopCondition;
                        Debug.Log("looping for target low heatlh at index " + i);
                    }  else if (behaviorList[i].values[0] == "2") { // facing target
                        conditionsDict[i] = FacingTarget() == whileLoopCondition;
                    } else if (behaviorList[i].values[0] == "3") { // self low health
                        conditionsDict[i] = SelfLowHealth() == whileLoopCondition;
                    } else if (behaviorList[i].values[0] == "4") { // target is healer
                        conditionsDict[i] = CheckTargetAttackType(BlockData.Property.HEAL_POWER) == whileLoopCondition;
                    } else if (behaviorList[i].values[0] == "5") { // target is melee
                        conditionsDict[i] = CheckTargetAttackType(BlockData.Property.MELEE_ATTACK_POWER) == whileLoopCondition;
                    } else if (behaviorList[i].values[0] == "6") { // target is ranged
                        conditionsDict[i] = CheckTargetAttackType(BlockData.Property.RANGED_ATTACK_POWER) == whileLoopCondition;
                    } else if (behaviorList[i].values[0] == "7") { // target has shield
                        conditionsDict[i] = CheckMagicShield() == whileLoopCondition;
                    } else if (behaviorList[i].values[0] == "8") { // in target melee range
                        conditionsDict[i] = CheckInTargetRange() == whileLoopCondition;
                    }

                    // if false or loop is infinite, just jump past
                    if (conditionsDict[i] == false || infinityCounters[i] <= 0) {
                        infinityCounters[i] = MAX_INFINITY_COUNTER;
                        Debug.Log("resetting infinity counter for index: " + i);
                        i = int.Parse(behaviorList[i].values[2]) - 1;
                        continue;
                    }
                    // otherwise tick down infinity counter
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
                    // add this loop to the for counters dict
                    if (!forCounters.ContainsKey(i)) {
                        forCounters[i] = int.Parse(behaviorList[i].values[0]);
                    }

                    // if looped enough times, jump to end of for loop
                    if (forCounters[i] <= 0) {
                        i = int.Parse(behaviorList[i].values[1]) - 1;
                        continue;
                    }
                    // otherwise, tick down counter
                    forCounters[i]--;
                    Debug.Log("ticking down for counter at " + i + " to " + forCounters[i]);
                    break;

                /*----------------*/
                /*    END LOOP    */
                /*----------------*/ /*   Current Status: Done
                no dropdowns

                    JUMP INDEX [0]
                end a while loop*/
                case BlockData.BehaviorType.END_LOOP:
                    Debug.Log("end loop");

                    // jump back to beginning of loop
                    i = int.Parse(behaviorList[i].values[0])-1;

                    break;
                
                /*----------------*/
                /*    END FOR    */
                /*----------------*/ /*   Current Status: Done
                no dropdowns

                    JUMP INDEX [0]
                end a for loop*/
                case BlockData.BehaviorType.END_FOR:
                    Debug.Log("end for");

                    // jump back to beginning of loop
                    i = int.Parse(behaviorList[i].values[0])-1;
                    break;
                
                /*----------*/
                /*    IF    */
                /*----------*/ /*   Current Status: DONE
                two dropdowns
                    choose condition [0]
                        0: target in range
                        1: target low health
                        2: facing target
                        3: self low health
                        4: target is healer
                        5: target is melee
                        6: target is ranged
                        7: target has shield
                        8: in target range

                    set boolean [1]
                        0: true
                        1: false
                    
                    JUMP INDEX [2]
                basic conditional */
                case BlockData.BehaviorType.IF:
                    Debug.Log("if");

                    // check if condition matches expected result ("0"==true, "1"==false)
                    // add index and result to conditions dict
                    bool ifCondition = behaviorList[i].values[1] == "0" ? true : false;

                    if (behaviorList[i].values[0] == "0") { // target in range
                        conditionsDict[i] = TargetInRange() == ifCondition;
                    } else if (behaviorList[i].values[0] == "1") { // target low health
                        conditionsDict[i] = TargetLowHealth() == ifCondition;
                    } else if (behaviorList[i].values[0] == "2") { // facing target
                        conditionsDict[i] = FacingTarget() == ifCondition;
                    } else if (behaviorList[i].values[0] == "3") { // self low health
                        conditionsDict[i] = SelfLowHealth() == ifCondition;
                    } else if (behaviorList[i].values[0] == "4") { // target is healer
                        conditionsDict[i] = CheckTargetAttackType(BlockData.Property.HEAL_POWER) == ifCondition;
                    } else if (behaviorList[i].values[0] == "5") { // target is melee
                        conditionsDict[i] = CheckTargetAttackType(BlockData.Property.MELEE_ATTACK_POWER) == ifCondition;
                    } else if (behaviorList[i].values[0] == "6") { // target is ranged
                        conditionsDict[i] = CheckTargetAttackType(BlockData.Property.RANGED_ATTACK_POWER) == ifCondition;
                    } else if (behaviorList[i].values[0] == "7") { // target has shield
                        conditionsDict[i] = CheckMagicShield() == ifCondition;
                    } else if (behaviorList[i].values[0] == "8") { // in target melee range
                        conditionsDict[i] = CheckInTargetRange() == ifCondition;
                    }

                    // if false just jump past
                    if (conditionsDict[i] == false) {
                        i = int.Parse(behaviorList[i].values[2]) - 1;
                        continue;
                    }
                    break;
                
                /*------------*/
                /*    ELSE    */
                /*------------*/ /*   Current Status: Done
                no dropdowns

                    JUMP INDEX [0]
                    END INDEX  [1]
                jumps to end if need to pass
                basic else case */
                case BlockData.BehaviorType.ELSE:
                    Debug.Log("else");
                    // if matching IF condition is true, skip the ELSE
                    if (conditionsDict[int.Parse(behaviorList[i].values[0])] == true) { // skip else, jump to endif
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
                    // update melee settings variables
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
                    // update magic settings variables
                    isRangedHeal = behaviorList[i].values[0] == "0" ? false : true;
                    isRangedTargetAllies = behaviorList[i].values[1] == "0" ? false : true;
                    break;
                
                /*-----------------------*/
                /*    FIRE PROJECTILE    */
                /*-----------------------*/ /*   Current Status: DONE
                no dropdowns
                do the actual ranged attack */
                case BlockData.BehaviorType.FIRE_PROJECTILE:
                    Debug.Log("fire projectile");
                    // don't fire projectile if no stamina
                    if (!UseStamina()) {
                        break;
                    }
                    // don't fire projectile if no target
                    if (target == null) {
                        IndicateNoTarget();
                        break;
                    }
                    
                    // play animation, ranged if it exists, regular if not
                    if (System.Array.Exists(animator.parameters, p => p.name == "RangedAttack")) {
                        this.animator.SetTrigger("RangedAttack");
                    } else {
                        this.animator.SetTrigger("Attack");
                    }
                    yield return new WaitForSeconds(LevelController.Instance.battleSpeed/1.5f);

                    // break if missing target at odd time
                    try {
                        // instantiate projectile
                        Vector3 projectileInitialPlacement = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)LevelController.Instance.objectsOnGrid[this.gameObject].x, (int)LevelController.Instance.objectsOnGrid[this.gameObject].y, 0));
                        projectileInitialPlacement.z = -1; // keep on top
                        GameObject projectile = Instantiate(projectilePrefab, projectileInitialPlacement, transform.rotation, this.transform);
                        ProjectileBehavior projectileBehavior = projectile.GetComponent<ProjectileBehavior>();
                        // set projectile target to current target
                        if (isRangedHeal) {
                            projectileBehavior.InitializeProjectile(this.target.gameObject, propertiesDict[BlockData.Property.HEAL_POWER], isRangedHeal);
                            projectile.GetComponent<SpriteRenderer>().sprite = projectileHealSprite;
                        } else {
                            Debug.Log(target.gameObject.name);
                            projectileBehavior.InitializeProjectile(this.target.gameObject, propertiesDict[BlockData.Property.RANGED_ATTACK_POWER], isRangedHeal);
                        }
                        // doing damage handled on projectile object

                        // play sound
                        AudioController.Instance.PlaySoundEffect("Fire Projectile");
                    } catch {
                        IndicateNoTarget();
                        break;
                    }
                    break;
                
                /*---------------*/
                /*    FOREACH    */
                /*---------------*/ /*   Current Status: NOT STARTED
                one dropdown
                    choose which team to look at
                        0: enemies
                        1: allies
                loop through all warriors of the given team */
                case BlockData.BehaviorType.FOREACH_LOOP:
                    break;

                /*------------------------*/
                /*    RECHARGE STAMINA    */
                /*------------------------*/ /*   Current Status: Done
                no dropdowns
                recharge stamina to full */
                case BlockData.BehaviorType.RECHARGE_STAMINA:
                    // re-enable stamina indicator
                    ToggleStaminaIndicator(true);
                    break;
            }
            // if no health now, this warrior is dead
            if (propertiesDict[BlockData.Property.HEALTH] <= 0 && isCurrentTurn) {
                Die();
            }
        }
    }

    // HELPER FUNCTIONS

    // rotate heading vector left or right
    public Vector2 RotateLeft(Vector2 vector) {
        return new Vector2((int)-vector.y, (int)vector.x);
    } 
    public Vector2 RotateRight(Vector2 vector) {
        return new Vector2((int)(- vector.y * (-1)), (int)(vector.x * (-1)));
    }

    // check that target is in melee range
    public bool TargetInRange() {
        // return if no target
        if (target == null) {
            IndicateNoTarget();
            return false;
        }

        // return false if missing target at odd time
        try {
            // loop through melee range
            List<Vector2> meleeRange = GetMeleeRange();
            foreach (Vector2 tile in meleeRange) {
                Vector2 tileToCheck = new Vector2((int)(LevelController.Instance.objectsOnGrid[this.gameObject].x + tile.x), (int)(LevelController.Instance.objectsOnGrid[this.gameObject].y + tile.y));
                if (LevelController.Instance.objectsOnGrid.ContainsValue(tileToCheck)) {
                    GameObject hitWarrior = LevelController.Instance.objectsOnGrid.FirstOrDefault(x => x.Value == tileToCheck).Key;
                    if (hitWarrior.GetComponent<WarriorBehavior>() == target) {
                        // target found
                        return true;
                    }
                }
            }
        } catch {
            Debug.Log("target was destroyed during check for in target range!");
            IndicateNoTarget();
            return false;
        }
        return false; // target not in range
    }

    public bool InTargetRange() {
        // return if no target
        if (target == null) {
            IndicateNoTarget();
            return false;
        }

        // return false if missing target at odd time
        try {
            // loop through target's melee range
            List<Vector2> targetMeleeRange = target.GetMeleeRange();
            foreach (Vector2 tile in targetMeleeRange) {
                Vector2 tileToCheck = new Vector2((int)(LevelController.Instance.objectsOnGrid[target.gameObject].x + tile.x), (int)(LevelController.Instance.objectsOnGrid[target.gameObject].y + tile.y));
                if (LevelController.Instance.objectsOnGrid.ContainsValue(tileToCheck)) {
                    GameObject hitWarrior = LevelController.Instance.objectsOnGrid.FirstOrDefault(x => x.Value == tileToCheck).Key;
                    if (hitWarrior.GetComponent<WarriorBehavior>() == this) {
                        // found this warrior in range
                        return true;
                    }
                }
            }
        } catch {
            Debug.Log("target was destroyed during check for in target range!");
            IndicateNoTarget();
            return false;
        }
        return false; // not in target's range
    }

    // DEPRECATED
    // checks if warrior passed in is within melee range of this warrior
    public bool WarriorInRange(GameObject warriorGameObject) {
        Debug.Log("warrior is at " + LevelController.Instance.objectsOnGrid[warriorGameObject]);
        List<Vector2> meleeRange = GetMeleeRange();

        try {
            foreach (Vector2 tile in meleeRange) {
                Vector2 tileToCheck = new Vector2((int)(LevelController.Instance.objectsOnGrid[this.gameObject].x + tile.x), (int)(LevelController.Instance.objectsOnGrid[this.gameObject].y + tile.y));
                Debug.Log("checking tile " + tileToCheck);
                if (LevelController.Instance.objectsOnGrid.ContainsValue(tileToCheck)) {
                    GameObject hitWarrior = LevelController.Instance.objectsOnGrid.FirstOrDefault(x => x.Value == tileToCheck).Key;
                    if (hitWarrior == warriorGameObject) {
                        Debug.Log("warrior in range of target");
                        return true;
                    }
                }
            }
        } catch {
            Debug.Log("target was destroyed during check for in target range!");
            IndicateNoTarget();
            return false;
        }
        Debug.Log("warrior not in range of target");
        return false;
    }

    public bool FacingTarget() {
        // return if no target
        if (target == null) {
            IndicateNoTarget();
            return false;
        }

        if (target == this) { // edge case
            return true;
        }

        // return false if no target at odd time
        try {
            // get target location
            Vector2 targetPos = LevelController.Instance.objectsOnGrid[target.gameObject];
            // calculate angle between this heading and target heading
            Vector2 hereToTarget = new Vector2((int)(targetPos.x - LevelController.Instance.objectsOnGrid[this.gameObject].x), (int)(targetPos.y - LevelController.Instance.objectsOnGrid[this.gameObject].y));
            // if within 45 degree vision cone, then facing
            if (Vector2.Angle(heading, hereToTarget) <= 45) {
                return true;
            }
        } catch {
            Debug.Log("couldn't find target in dictionary, likely");
            return false;
        }
        return false; // not facing
    }

    public bool TargetLowHealth() {
        // return if no target
        if (target == null) {
            IndicateNoTarget();
            return false;
        }

        // return false if no target at odd time
        try {
            // true if target less than 1/3 health
            if (target.propertiesDict[BlockData.Property.HEALTH] / target.maxHealth < 0.34) {
                return true;
            }
        } catch {
            Debug.Log("target was destroyed during check for in target range!");
            IndicateNoTarget();
            return false; // no target
        }
        return false; // not low health
    }

    public bool CheckTargetAttackType(BlockData.Property property) {
        // return if no target
        if (target == null) {
            IndicateNoTarget();
            return false;
        }

        // return false if no target at odd time
        try {
            // true if target has this property as an attached block (not just default 0 val)
            if (target.propertiesDict[property] > 1) {
                return true;
            }
        } catch {
            Debug.Log("target was destroyed during check for in target range!");
            IndicateNoTarget();
            return false; // no target
        }
        return false; // target attack type doesn't match
    }

    public bool CheckMagicShield() {
        // return if no target
        if (target == null) {
            IndicateNoTarget();
            return false;
        }

        // return magic shield value
        return target.GetMagicShield();
    }

    public bool CheckInTargetRange() {
        // return if no target
        if (target == null) {
            IndicateNoTarget();
            return false;
        }

        // run the in range function on the enemy with this warrior as the object
        return InTargetRange();
    }

    public bool SelfLowHealth() {
        // return true if self at less than 1/3 health, false otherwise
        if (propertiesDict[BlockData.Property.HEALTH] / maxHealth < 0.34) {
            return true;
        }
        return false;
    }

    public WarriorBehavior CalculateStrongest(List<WarriorBehavior> warriorList) {
        // returns the warrior with highest strength
        // loop through all warriors to find
        WarriorBehavior strongest = warriorList[0];
        foreach (WarriorBehavior warrior in warriorList) {
            if (warrior.totalStrength > strongest.totalStrength) {
                strongest = warrior;
            }
        }
        return strongest;
    }

    public WarriorBehavior CalculateWeakest(List<WarriorBehavior> warriorList) {
        // returns the warrior with the lowest strength
        // loop through all warriors to find
        WarriorBehavior weakest = warriorList[0];
        foreach (WarriorBehavior warrior in warriorList) {
            if (warrior.totalStrength < weakest.totalStrength) {
                weakest = warrior;
            }
        }
        return weakest;
    }

    public WarriorBehavior CalculateHealthiest(List<WarriorBehavior> warriorList) {
        // returns the warrior with the highest health;
        // loop through all warriors to find
        WarriorBehavior healthiest = warriorList[0];
        foreach (WarriorBehavior warrior in warriorList) {
            if (warrior.propertiesDict[BlockData.Property.HEALTH] > healthiest.propertiesDict[BlockData.Property.HEALTH]) {
                healthiest = warrior;
            }
        }
        return healthiest;
    }

    public WarriorBehavior CalculateFrailest(List<WarriorBehavior> warriorList) {
        // returns the warrior with the lowest health
        // loop through all warriors to find
        WarriorBehavior frailest = warriorList[0];
        foreach (WarriorBehavior warrior in warriorList) {
            if (warrior.propertiesDict[BlockData.Property.HEALTH] < frailest.propertiesDict[BlockData.Property.HEALTH]) {
                frailest = warrior;
            }
        }
        return frailest;
    }

    public void SetImageFacing() {
        // set visual facing based on header
        Transform visualTransform = transform.GetChild(0);
        if (heading == new Vector2((int)-1, (int)0)) {
            visualTransform.rotation = Quaternion.Euler(0, 180, 0);
        } else if (heading == new Vector2((int)1, (int)0)) {
            visualTransform.rotation = Quaternion.Euler(0, 0, 0);
        } else if (heading == new Vector2((int)0, (int)1)) {
            visualTransform.rotation = Quaternion.Euler(0, 0, 90);
        } else if (heading == new Vector2((int)0, (int)-1)) {
            visualTransform.rotation = Quaternion.Euler(0, 0, -90);
        } else {
            // handle case where the heading doesn't match for some reason
            Debug.Log("ERROR: heading didn't match for set image facing: " + heading);
        }
    }

    public List<Vector2> GetMeleeRange() {
        // create list of tiles in range
        List<Vector2> rotatedList = new List<Vector2>(tilesToHitRelative);
        while (rotatedList[0] != heading) {
            // rotate range tiles to be relative to heading
            for (int j = 0; j < rotatedList.Count; j++) {
                rotatedList[j] = RotateRight(rotatedList[j]);
            }
        }
        Debug.Log("range is facing the correct way!");

        // find the actual range
        // create list of tiles in range based on set value, chosen from list of all possible range tiles
        List<Vector2> adjustedList = new List<Vector2>();
        switch(propertiesDict[BlockData.Property.MELEE_ATTACK_RANGE]) {
            case 1:
                adjustedList.Add(rotatedList[0]);
                break;
            case 2:
                adjustedList.Add(rotatedList[0]);
                adjustedList.Add(rotatedList[1]);
                break;
            case 3:
                adjustedList.Add(rotatedList[0]);
                adjustedList.Add(rotatedList[2]);
                adjustedList.Add(rotatedList[3]);
                break;
            case 4:
                adjustedList.Add(rotatedList[0]);
                adjustedList.Add(rotatedList[1]);
                adjustedList.Add(rotatedList[2]);
                adjustedList.Add(rotatedList[3]);
                break;
            case 5:
                adjustedList = new List<Vector2>(rotatedList);
                break;
            default:
                adjustedList.Add(rotatedList[0]);
                break;
        }
        return adjustedList;
    }

    public bool GetMagicShield() {
        return this.magicShield;
    }

    public bool UseStamina() {
        // return true if we use stamina
        if (hasStamina) {
            ToggleStaminaIndicator(false);
            return true;
        } else {
            IndicateNoStamina();
            return false;
        }
    }

    public void ToggleStaminaIndicator(bool value) {
        // show visual and set variable for stamina
        hasStamina = value;
        staminaIndicator.GetComponent<Image>().color = value ? Color.white : Color.black;
    }

    // INDICATE NO STAMINA AND TARGET ERRORS MID BATTLE
    public void IndicateNoStamina() {
        Debug.Log("no stamina! cannot attack");
        if (noStaminaTextbox.activeSelf) {
            noStaminaTextbox.GetComponent<NoStaminaDisplayBehavior>().ShowNoStaminaText();
        } else {
            noStaminaTextbox.SetActive(true);
        }
    }

    public void IndicateNoTarget() {
        Debug.Log("no target! cannot do target thing");
        if (noStaminaTextbox.activeSelf) {
            noStaminaTextbox.GetComponent<NoStaminaDisplayBehavior>().ShowNoTargetText();
        } else {
            noStaminaTextbox.SetActive(true);
        }
    }

    public void DoDamageOrHeal(float value, bool isHeal) {
        // if damage to be done ie heal, add it to health up to maximum
        if (isHeal) {
            if (propertiesDict[BlockData.Property.HEALTH] + value >= maxHealth) {
                propertiesDict[BlockData.Property.HEALTH] = maxHealth;
            } else {
                propertiesDict[BlockData.Property.HEALTH] += value;
            }
            // play sound
            AudioController.Instance.PlaySoundEffect("Heal");
            Debug.Log("healed");
        } else { // otherwise, take damage and subtract from health
            this.animator.SetTrigger("TakeDamage");
            AudioController.Instance.PlaySoundEffect("Take Damage");
            propertiesDict[BlockData.Property.HEALTH] -= DamageCalculator(value);
            if (propertiesDict[BlockData.Property.HEALTH] <= 0 && !isCurrentTurn) { // if it is current turn, delay death til end of action
                Die();
            }
        }

        // update health
        healthBar.value = propertiesDict[BlockData.Property.HEALTH];
        UpdateHealthDisplay();
    }

    public float DamageCalculator(float damage) {
        // calculate damage with defense taken into account
        return damage * (1 - (propertiesDict[BlockData.Property.DEFENSE] / (propertiesDict[BlockData.Property.DEFENSE] + maxHealth)));
    }

    public void Die() {
        if (isAlive) { // additional check, prevents inconsistent error if battle speed too fast
            isAlive = false;
            Debug.Log("dead!");
            // remove this object from grid
            LevelController.Instance.objectsOnGrid.Remove(this.gameObject);
            // remove this object from ally or enemy list
            if (!isEnemy) {
                LevelController.Instance.yourWarriorsList.Remove(this);
            } else {
                LevelController.Instance.enemyWarriorsList.Remove(this);
            }

            // delay death in case it's active turn
            // this also lets animation play out
            LevelController.Instance.activeDeathDelay = true;
            StartCoroutine(DeathDelay());
            LevelController.Instance.activeDeathDelay = false;
        }
    }

    public IEnumerator DeathDelay() {
        // wait for animation to play out before dying
        Debug.Log("started death delay");
        this.animator.SetTrigger("Die");
        AudioController.Instance.PlaySoundEffect("Die");
        yield return new WaitForSeconds(LevelController.Instance.battleSpeed + .1f);
        Debug.Log("ended death delay");
        // last, set game object active to false
        this.gameObject.SetActive(false);
        yield return new WaitForSeconds(LevelController.Instance.battleSpeed + .3f);
    }
}
