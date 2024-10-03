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

    [Header("vars")]
    public bool isDragging;
    private Vector3 offset;

    // block lists
    private List<BlockData> propertiesData;
    private List<BlockData> moveData;
    private List<BlockData> useWeaponData;
    private List<BlockData> useSpecialData;


    // setup
    void Awake() {
        SetProperties();

        // first child is visual
        sprite = transform.GetChild(0).GetComponent<Sprite>();
    }

    // ondrag needed to start drag from ui
    public void OnDrag(PointerEventData eventData) {
        transform.position = InputManager.Instance.GetSelectedMapPosition();
    }
    


    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if (isDragging) transform.position = InputManager.Instance.GetSelectedMapPosition(); // this allows dragging from worldspace
        
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
