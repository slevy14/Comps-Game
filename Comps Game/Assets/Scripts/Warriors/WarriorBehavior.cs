using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WarriorBehavior : MonoBehaviour, IDragHandler {

    // codeable properties
    [Header("Properties")]
    [SerializeField] private string warriorName;
    [SerializeField] private float health;
    [SerializeField] private float defense;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float meleeAttackPower;
    [SerializeField] private float meleeAttackSpeed;
    [SerializeField] private float meleeAttackRange;
    [SerializeField] private float rangedAttackPower;
    [SerializeField] private float rangedAttackSpeed;
    [SerializeField] private float distancedRange;
    [SerializeField] private float specialPower;
    [SerializeField] private float specialSpeed;
    [SerializeField] private float healPower;
    [SerializeField] private float healSpeed;

    [Space(20)]
    [Header("References")]
    private Sprite sprite;
    private InputManager inputManager;

    [Header("Dragging")]
    private bool isDragging;
    private Vector3 initialPos;
    public bool isNew = true;

    // block lists
    private List<BlockData> propertiesData;
    private List<BlockData> moveData;
    private List<BlockData> useWeaponData;
    private List<BlockData> useSpecialData;


    // setup
    void Awake() {
        SetProperties();
        inputManager = GameObject.Find("InputManager").GetComponent<InputManager>();

        // first child is visual
        sprite = transform.GetChild(0).GetComponent<Sprite>();
    }

    // DRAGGING
    // ondrag needed to start drag from ui
    public void OnDrag(PointerEventData eventData) {
        // transform.position = inputManager.GetSelectedMapPosition();
    }

    public void StartDrag() {
        isDragging = true;
        // save initial position
        initialPos = transform.position;
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
        // if over empty grid tile, place
        // if over full grid tile, return to initial
        // if out of bounds, return to initial
        // if over drawer at bottom, destroy
    }
    


    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        // // DRAG
        if (isDragging) { // this allows dragging from worldspace
            transform.position = inputManager.GetSelectedMapPosition();
        }
        
    }

    


    // FUNCTIONS FROM WHITEBOARD HEADERS
    private void SetProperties() {

    }

    private void Move() {

    }

    private void UseWeapon() {

    }

    private void UseSpecial() {

    }
}
