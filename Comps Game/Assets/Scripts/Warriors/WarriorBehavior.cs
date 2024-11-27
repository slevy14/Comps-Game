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
    private int MAX_INFINITY_COUNTER = 10;
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
        propertiesDict = new Dictionary<BlockData.Property, float>();
        conditionsDict = new();
        forCounters = new();

        infinityCounters = new();

        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();

        // first child is visual
        sprite = transform.GetChild(0).GetComponent<Sprite>();
        healthBar = transform.GetChild(1).transform.GetChild(0).GetComponent<Slider>();
        pointer = transform.GetChild(2).gameObject;
        SetImageFacing();

        heading = new Vector2((int)1, (int)0);
        tilesToHitRelative = new List<Vector2>{new Vector2((int)1, (int)0), new Vector2((int)2, (int)0), new Vector2((int)1, (int)1), new Vector2((int)1, (int)-1), new Vector2((int)2, (int)1), new Vector2((int)2, (int)-1)};
        animator.speed = 2.01f - LevelController.Instance.battleSpeed;
    }

    public void SetIsEnemy() {
        isEnemy = true;
        heading = new Vector2((int)-1, (int)0);
        SetImageFacing();
        this.gameObject.tag = "enemy";
        healthBar.gameObject.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color(167f/255f, 85/255f, 255/255f);
        healthBar.gameObject.transform.GetChild(2).GetComponent<TMP_Text>().color = Color.white;
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

        if (LevelController.Instance.objectsOnGrid.ContainsKey(this.gameObject)) {
            Debug.Log("former position for dragging object: " + LevelController.Instance.objectsOnGrid[this.gameObject]);
            initialGridPos = LevelController.Instance.objectsOnGrid[this.gameObject];
            LevelController.Instance.objectsOnGrid.Remove(this.gameObject);
        }

        // visuals
        this.transform.localScale *= 1.5f;

        // audio
        AudioController.Instance.PlaySoundEffect("Warrior Pickup");
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

        // audio
        AudioController.Instance.PlaySoundEffect("Warrior Drop");
    }

    void OnMouseEnter() {
        transform.GetChild(0).GetComponent<SpriteOutline>().enabled = true;
        // if (!isEnemy) {
        //     transform.GetChild(0).GetComponent<SpriteOutline>().SetColor(new Color(104, 104, 241));
        // } else {
        //     transform.GetChild(0).GetComponent<SpriteOutline>().SetColor(new Color(241, 104, 104));
        // }
    }

    void OnMouseExit() {
        transform.GetChild(0).GetComponent<SpriteOutline>().enabled = false;
    }


    
    // Update is called once per frame
    void Update() {
        // // DRAG
        // if (isDragging) { // this allows dragging from worldspace
        //     transform.position = inputManager.GetSelectedMapPosition();
        // }
        animator.speed = 3.01f - LevelController.Instance.battleSpeed;
        
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

    public void MarkCurrentTurn(bool value) {
        isCurrentTurn = value;
        pointer.SetActive(value);
        ToggleStaminaIndicator(true);
    }

    public void SetPropertiesAndBehaviors(List<BlockDataStruct> properties, List<BlockDataStruct> move, List<BlockDataStruct> useWeapon, List<BlockDataStruct> useSpecials) {
        InitializeProperties();
        propertiesData = properties;
        SetProperties();

        moveData = move;
        useWeaponData = useWeapon;
        useSpecialData = useSpecials;

        UpdateHealthDisplay();

        totalStrength = CalculateTotalStrength();
    }

    public void UpdateHealthDisplay() {
        healthBar.transform.GetChild(2).GetComponent<TMP_Text>().text = Mathf.CeilToInt(propertiesDict[BlockData.Property.HEALTH]) + " / " + maxHealth;
    }

    public float GetProperty(BlockData.Property property) {
        return propertiesDict[property];
    }

    public int CalculateTotalStrength() {
        // FORMULA:
        // strength = attackPower*attackRange + healPower*attackRange + projectilePower + maxHealth*(1+ (defense) / (defense+maxHealth+1)) + speed/10;
        // (propertiesDict[BlockData.Property.DEFENSE] / (propertiesDict[BlockData.Property.DEFENSE] + maxHealth))
        
        return HelperController.Instance.CalculateWarriorStrength((int)propertiesDict[BlockData.Property.MELEE_ATTACK_POWER], (int)propertiesDict[BlockData.Property.MELEE_ATTACK_RANGE], (int)propertiesDict[BlockData.Property.HEAL_POWER], (int)propertiesDict[BlockData.Property.RANGED_ATTACK_POWER], (int)propertiesDict[BlockData.Property.MOVE_SPEED], (int)maxHealth, (int)propertiesDict[BlockData.Property.DEFENSE]);
    }

    // check if warrior preexisting on grid is valid strength and behaviors
    public void CheckValidOnGrid() {
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

    // BIG TODO: projectile range???

    // Major Functions
    public IEnumerator Move() {
        // reset target each time bc it's coded in
        // this will prevent targeting errors after destroying a warrior
        target = null;
        Debug.Log("MOVE FUNCTIONS:");
        if (gameObject.activeSelf) {
            yield return StartCoroutine(RunBehaviorFunctions(moveData));
        }
    }

    public IEnumerator UseWeapon() {
        Debug.Log("USE WEAPON FUNCTIONS:");
        if (gameObject.activeSelf) {
            yield return StartCoroutine(RunBehaviorFunctions(useWeaponData));
        }
    }

    public IEnumerator UseSpecial() {
        Debug.Log("USE SPECIAL FUNCTIONS:");
        if (gameObject.activeSelf) {
            yield return StartCoroutine(RunBehaviorFunctions(useSpecialData));
        }
    }

    // Parser:
    // loop through all behavior, run code for each as needed
    public IEnumerator RunBehaviorFunctions(List<BlockDataStruct> behaviorList) {
        conditionsDict.Clear();
        forCounters.Clear();
        infinityCounters.Clear();
        for (int i = 0; i < behaviorList.Count; i++) {
            // Debug.Log("i is " + i);
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

            // switch behavior
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
                    SetImageFacing();
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
                    Vector2 teleportPos = new();
                    if (behaviorList[i].values[0] == "0") { // behind
                        if (target == null) {
                            IndicateNoTarget();
                            break;
                        }
                        // get target position minus heading
                        teleportPos = new Vector2((int)LevelController.Instance.objectsOnGrid[target.gameObject].x, (int)LevelController.Instance.objectsOnGrid[target.gameObject].y) - target.heading;
                        // try set this position to new position
                        if (!LevelController.Instance.objectsOnGrid.ContainsValue(teleportPos) && PlacementSystem.Instance.tilemap.HasTile(new Vector3Int((int)teleportPos.x, (int)teleportPos.y, 0))) {
                            this.gameObject.transform.position = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)teleportPos.x, (int)teleportPos.y, 0));
                            LevelController.Instance.objectsOnGrid[this.gameObject] = teleportPos;

                            // auto turn (no longer used):
                            // heading = target.heading;
                            // SetImageFacing();
                        } else {
                            Debug.Log("either tile full or would teleport off map");
                        }
                    } else if (behaviorList[i].values[0] == "1") { // flank
                        if (target == null) {
                            IndicateNoTarget();
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
                            // auto turn (no longer used):
                            // heading = RotateRight(target.heading);
                            // SetImageFacing();
                            break;
                        }
                        // else if right flank free
                        Vector2 rightFlank = new Vector2((int)(LevelController.Instance.objectsOnGrid[target.gameObject].x + RotateRight(target.heading).x), (int)(LevelController.Instance.objectsOnGrid[target.gameObject].y + RotateRight(target.heading).y));
                        if (!LevelController.Instance.objectsOnGrid.ContainsValue(rightFlank) && PlacementSystem.Instance.tilemap.HasTile(new Vector3Int((int)rightFlank.x, (int)rightFlank.y, 0))) {
                            // get target position plus rotated right heading
                            // try set this position to new position
                            this.gameObject.transform.position = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)rightFlank.x, (int)rightFlank.y, 0));
                            LevelController.Instance.objectsOnGrid[this.gameObject] = rightFlank;
                            // auto turn (no longer used):
                            // heading = RotateLeft(target.heading);
                            // SetImageFacing();
                            break;
                        }
                    }
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
                    List<Vector2> adjustedList = GetMeleeRange();

                    // loop through adjusted list:
                        // deal damage to square

                    this.animator.SetTrigger("Attack");
                    yield return new WaitForSeconds(LevelController.Instance.battleSpeed/1.5f);

                    foreach (Vector2 tile in adjustedList) {
                        Vector2 tileToAttack = new Vector2((int)(LevelController.Instance.objectsOnGrid[this.gameObject].x + tile.x), (int)(LevelController.Instance.objectsOnGrid[this.gameObject].y + tile.y));
                        Vector3 meleeIconPlacement = PlacementSystem.Instance.tilemap.GetCellCenterWorld(new Vector3Int((int)tileToAttack.x, (int)tileToAttack.y, 0));
                        meleeIconPlacement.z = -1; // keep on top
                        GameObject icon = Instantiate(meleePrefab, meleeIconPlacement, transform.rotation, this.transform);
                        if (isMeleeHeal) {
                            icon.GetComponent<SpriteRenderer>().sprite = healSprite;
                        }
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
                                // Debug.Log(warrior.warriorName + " was further");
                            } else {
                                // Debug.Log(warrior.warriorName + " was not further");
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
                        int randIndex = UnityEngine.Random.Range(0, targetTeam.Count);
                        target = targetTeam[randIndex];
                    } else if (behaviorList[i].values[0] == "5") { // healthiest
                        target = CalculateHealthiest(targetTeam);
                    } else if (behaviorList[i].values[0] == "6") { // frailest
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
                    if (!infinityCounters.ContainsKey(i)) {
                        infinityCounters[i] = MAX_INFINITY_COUNTER;
                    }
                    // check condition
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

                    // check condition
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
                /*-----------------------*/ /*   Current Status: DONE
                no dropdowns
                do the actual ranged attack */
                case BlockData.BehaviorType.FIRE_PROJECTILE:
                    Debug.Log("fire projectile");
                    // don't fire projectile if no stamina
                    if (!UseStamina()) {
                        break;
                    }
                    if (target == null) {
                        IndicateNoTarget();
                        break;
                    }
                    if (System.Array.Exists(animator.parameters, p => p.name == "RangedAttack")) {
                        this.animator.SetTrigger("RangedAttack");
                    } else {
                        this.animator.SetTrigger("Attack");
                    }
                    yield return new WaitForSeconds(LevelController.Instance.battleSpeed/1.5f);

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
                    AudioController.Instance.PlaySoundEffect("Fire Projectile");
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
                        ToggleStaminaIndicator(true);
                        // FIXME: add sound effect
                        break;
            }
            if (propertiesDict[BlockData.Property.HEALTH] <= 0 && isCurrentTurn) {
                Die();
            }
        }
    }

    // Helper Functions
    public Vector2 RotateLeft(Vector2 vector) {
        return new Vector2((int)-vector.y, (int)vector.x);
    } 
    public Vector2 RotateRight(Vector2 vector) {
        return new Vector2((int)(- vector.y * (-1)), (int)(vector.x * (-1)));
    }

    public bool TargetInRange() {
        if (target == null) {
            IndicateNoTarget();
            return false;
        }

        List<Vector2> meleeRange = GetMeleeRange();
        foreach (Vector2 tile in meleeRange) {
            Vector2 tileToCheck = new Vector2((int)(LevelController.Instance.objectsOnGrid[this.gameObject].x + tile.x), (int)(LevelController.Instance.objectsOnGrid[this.gameObject].y + tile.y));
            if (LevelController.Instance.objectsOnGrid.ContainsValue(tileToCheck)) {
                GameObject hitWarrior = LevelController.Instance.objectsOnGrid.FirstOrDefault(x => x.Value == tileToCheck).Key;
                if (hitWarrior.GetComponent<WarriorBehavior>() == target) {
                    return true;
                }
            }
        }
        return false;
    }

    public bool InTargetRange() {
        if (target == null) {
            IndicateNoTarget();
            return false;
        }

        try {
            List<Vector2> targetMeleeRange = target.GetMeleeRange();
            foreach (Vector2 tile in targetMeleeRange) {
                Vector2 tileToCheck = new Vector2((int)(LevelController.Instance.objectsOnGrid[target.gameObject].x + tile.x), (int)(LevelController.Instance.objectsOnGrid[target.gameObject].y + tile.y));
                if (LevelController.Instance.objectsOnGrid.ContainsValue(tileToCheck)) {
                    GameObject hitWarrior = LevelController.Instance.objectsOnGrid.FirstOrDefault(x => x.Value == tileToCheck).Key;
                    if (hitWarrior.GetComponent<WarriorBehavior>() == this) {
                        return true;
                    }
                }
            }
        } catch {
            Debug.Log("target was destroyed during check for in target range!");
            IndicateNoTarget();
            return false;
        }
        return false;
    }

    public bool WarriorInRange(GameObject warriorGameObject) {
        Debug.Log("warrior is at " + LevelController.Instance.objectsOnGrid[warriorGameObject]);
        List<Vector2> meleeRange = GetMeleeRange();

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
        Debug.Log("warrior not in range of target");
        return false;
    }

    public bool FacingTarget() {
        if (target == null) {
            IndicateNoTarget();
            return false;
        }

        if (target == this) { // edge case
            return true;
        }

        // Debug.Log("checking if facing " +target.warriorName);
        try {
            Vector2 targetPos = LevelController.Instance.objectsOnGrid[target.gameObject];
            Vector2 hereToTarget = new Vector2((int)(targetPos.x - LevelController.Instance.objectsOnGrid[this.gameObject].x), (int)(targetPos.y - LevelController.Instance.objectsOnGrid[this.gameObject].y));
            if (Vector2.Angle(heading, hereToTarget) <= 45) {
                Debug.Log ("distance vec: " + hereToTarget + ", current heading: " + heading);
                Debug.Log("facing " + target.warriorName + ", angle " + Vector2.Angle(heading, hereToTarget));
                return true;
            }
        } catch {
            Debug.Log("couldn't find target in dictionary, likely");
        }
        
        return false;
    }

    public bool TargetLowHealth() {
        if (target == null) {
            IndicateNoTarget();
            return false;
        }

        if (target.propertiesDict[BlockData.Property.HEALTH] / target.maxHealth < 0.34) {
            return true;
        }
        return false;
    }

    public bool CheckTargetAttackType(BlockData.Property property) {
        if (target == null) {
            IndicateNoTarget();
            return false;
        }

        if (target.propertiesDict[property] > 1) {
            return true;
        }
        return false;
    }

    public bool CheckMagicShield() {
        if (target == null) {
            IndicateNoTarget();
            return false;
        }

        return target.GetMagicShield();
    }

    public bool CheckInTargetRange() {
        if (target == null) {
            IndicateNoTarget();
            return false;
        }

        // run the in range function on the enemy with this warrior as the object
        // return WarriorInRange(this.gameObject);
        return InTargetRange();
    }

    public bool SelfLowHealth() {
        if (propertiesDict[BlockData.Property.HEALTH] / maxHealth < 0.34) {
            return true;
        }
        return false;
    }

    public WarriorBehavior CalculateStrongest(List<WarriorBehavior> warriorList) {
        // returns the warrior with highest strength
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
        WarriorBehavior frailest = warriorList[0];
        foreach (WarriorBehavior warrior in warriorList) {
            if (warrior.propertiesDict[BlockData.Property.HEALTH] < frailest.propertiesDict[BlockData.Property.HEALTH]) {
                frailest = warrior;
            }
        }
        return frailest;
    }

    public void SetImageFacing() {
        Transform visualTransform = transform.GetChild(0);
        Debug.Log("setting image facing from heading " + heading);
        if (heading == new Vector2((int)-1, (int)0)) {
            visualTransform.rotation = Quaternion.Euler(0, 180, 0);
        } else if (heading == new Vector2((int)1, (int)0)) {
            visualTransform.rotation = Quaternion.Euler(0, 0, 0);
        } else if (heading == new Vector2((int)0, (int)1)) {
            visualTransform.rotation = Quaternion.Euler(0, 0, 90);
        } else if (heading == new Vector2((int)0, (int)-1)) {
            visualTransform.rotation = Quaternion.Euler(0, 0, -90);
        } else {
            Debug.Log("ERROR: heading didn't match for set image facing: " + heading);
        }
    }

    public List<Vector2> GetMeleeRange() {
        List<Vector2> rotatedList = new List<Vector2>(tilesToHitRelative);
        while (rotatedList[0] != heading) {
            Debug.Log("need to rotate range");
            for (int j = 0; j < rotatedList.Count; j++) {
                rotatedList[j] = RotateRight(rotatedList[j]);
            }
        }
        Debug.Log("range is facing the correct way!");

        // find the actual range
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
        hasStamina = value;
        staminaIndicator.GetComponent<Image>().color = value ? Color.white : Color.black;
    }

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
        if (isHeal) {
            if (propertiesDict[BlockData.Property.HEALTH] + value >= maxHealth) {
                propertiesDict[BlockData.Property.HEALTH] = maxHealth;
            } else {
                propertiesDict[BlockData.Property.HEALTH] += value;
            }
            AudioController.Instance.PlaySoundEffect("Heal");
            Debug.Log("healed");
        } else {
            this.animator.SetTrigger("TakeDamage");
            AudioController.Instance.PlaySoundEffect("Take Damage");
            propertiesDict[BlockData.Property.HEALTH] -= DamageCalculator(value);
            if (propertiesDict[BlockData.Property.HEALTH] <= 0 && !isCurrentTurn) { // if it is current turn, delay death til end of action
                Die();
            }
        }
        healthBar.value = propertiesDict[BlockData.Property.HEALTH];
        UpdateHealthDisplay();
    }

    public float DamageCalculator(float damage) {
        return damage * (1 - (propertiesDict[BlockData.Property.DEFENSE] / (propertiesDict[BlockData.Property.DEFENSE] + maxHealth)));
    }

    public void Die() {
        if (isAlive) { // adding check bc it runs multiple times if battle speed too fast
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
            // set warrior dead

            // this.gameObject.SetActive(false);
            // delay death in case it's active turn
            StartCoroutine(DeathDelay());
        }
    }

    public IEnumerator DeathDelay() {
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
